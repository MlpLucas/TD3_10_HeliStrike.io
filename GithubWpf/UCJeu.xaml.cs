using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        private static BitmapImage Helico1;
        private DispatcherTimer movementTimer;
        private static bool Agauche, Adroite;

        //Test
        int enemieSpriteCounter;
        int enemieCounter;
        int limit = 50;
        int score = 0;
        int damage = 0;
        Rect playerHitBox;

        public UCJeu()
        {
            InitializeComponent();
            ChargeImageAnimation();
            // démarrage de la logique d'animation/déplacement
            InitTimer();

            imgHelico.Source = Helico1;

            // garantir que Loaded/Unloaded sont pris en compte
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
        }

        private void ChargeImageAnimation()
        {
            //A COMPLETER pour gérer les différents hélicoptères
            //Helico1 = new BitmapImage(new Uri($"pack://application:,,,/Images/Helicoptere/helico{MainWindow.Perso}-1.png"));
            Helico1 = new BitmapImage(new Uri($"pack://application:,,,/Images/Helicoptere/helico1-1.png"));
        }

        private void InitTimer()
        {
            movementTimer = new DispatcherTimer();
            movementTimer.Interval = TimeSpan.FromMilliseconds(16);
            movementTimer.Tick += MovementTimer_Tick;
            movementTimer.Start();
        }

        private void MovementTimer_Tick(object? sender, EventArgs e)
        {
            // récupération sûre de la position left
            double left = Canvas.GetLeft(imgHelico);
            if (double.IsNaN(left))
                left = 0;

            // cas exclusif : droite ou gauche
            if (Adroite && !Agauche)
            {
                double maxLeft = canvasJeu.ActualWidth - imgHelico.Width;
                double newLeft = left + MainWindow.PasHelico;
                if (newLeft > maxLeft) newLeft = maxLeft;
                Canvas.SetLeft(imgHelico, newLeft);
            }
            else if (Agauche && !Adroite)
            {
                double newLeft = left - MainWindow.PasHelico;
                if (newLeft < 0) newLeft = 0;
                Canvas.SetLeft(imgHelico, newLeft);
            }

#if DEBUG
            Console.WriteLine("Position Left hélicopère :" + Canvas.GetLeft(imgHelico));
#endif
        }

        // Abonné keyDown et keyUp de la mainWindow via Loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // attacher aux événements clavier de la fenêtre principale
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.KeyDown += canvasJeu_KeyDown;
                Application.Current.MainWindow.KeyUp += canvasJeu_KeyUp;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // détacher pour éviter fuites et arrêter le timer
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.KeyDown -= canvasJeu_KeyDown;
                Application.Current.MainWindow.KeyUp -= canvasJeu_KeyUp;
            }

            if (movementTimer != null)
            {
                movementTimer.Stop();
                movementTimer.Tick -= MovementTimer_Tick;
                movementTimer = null;
            }
        }

        private void canvasJeu_KeyDown(object sender, KeyEventArgs e)
        {
            // ne pas déplacer ici : juste mettre à true le booléen
            if (e.Key == Key.Right)
            {
                Adroite = true;
            }
            else if (e.Key == Key.Left)
            {
                Agauche = true;
            }

            //Création de balles/tires lors de l'appui sur la barre espace
            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                // place the bullet on top of the player location
                Canvas.SetTop(newBullet, Canvas.GetTop(imgHelico) - newBullet.Height);
                // place the bullet middle of the player image
                Canvas.SetLeft(newBullet, Canvas.GetLeft(imgHelico) + imgHelico.Width / 2);
                // add the bullet to the screen
                canvasJeu.Children.Add(newBullet);
            }
        }

        private void canvasJeu_KeyUp(object sender, KeyEventArgs e)
        {
            // remettre à false le booléen correspondant
            if (e.Key == Key.Right)
            {
                Adroite = false;
            }
            else if (e.Key == Key.Left)
            {
                Agauche = false;
            }
        }
    }
}
