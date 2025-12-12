using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
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
        private BitmapImage[] Helico1 = new BitmapImage[6];
        private DispatcherTimer movementTimer;
        private static bool Agauche, Adroite;
        Random rand = new Random();
        int cadenceTir = 8;
        int tempsRecharge = 0;
        int nb_animation_helico = 0;
        int score = 0;
        int damage = 0;
        int pointVie = 3;

        //Test
        int enemieCounter;
        int limit = 50;
        
        Rect playerHitBox;

        public UCJeu()
        {
            InitializeComponent();
            ChargeImageAnimation();
            // démarrage de la logique d'animation/déplacement
            InitTimer();
 

            // garantir que Loaded/Unloaded sont pris en compte
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
            // Permet au Canvas de capter les touches du clavier
            canvasJeu.Focusable = true;
            canvasJeu.Focus();
        }

        private void ChargeImageAnimation()
        {
            try
            {
                // Charge les images de l'hélico
               
                for (int i = 0; i < Helico1.Length; i++)
                {
                    Helico1[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/Helicoptere/helico{MainWindow.Perso}-{i + 1}.png"));
                }
            }
            catch
            {
                MessageBox.Show("Attention : Images manquantes dans le dossier !");
            }

        }

        private void InitTimer()
        {
            movementTimer = new DispatcherTimer();
            movementTimer.Interval = TimeSpan.FromMilliseconds(20);
            movementTimer.Tick += MovementTimer_Tick;
            movementTimer.Start();
        }

        private void MovementTimer_Tick(object? sender, EventArgs e)
        { 
            //GESTION TIR
            if (tempsRecharge > 0)
            {
                tempsRecharge--;
            }

            // Si on appuie sur ESPACE ET que l'arme est prête (tempsRecharge == 0)
            if (Keyboard.IsKeyDown(Key.Space) && tempsRecharge <= 0)
            {
                CreerBalle();           // On tire !
                tempsRecharge = cadenceTir; // On réinitialise le délai (on doit attendre 10 tours)
            }

            //GESTION DU JOUEUR
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
           
            // GESTION ENNEMIS

            enemieCounter--; // Diminution compteur
            if (enemieCounter < 0)
            {
                MakeEnemies(); // Un nouvel ennemi apparaît
                enemieCounter = limit; // On remet le compteur à zéro
            }
            // DEPLACEMENT ET COLLISIONS
            List<UIElement> magasinItemMouv = new List<UIElement>();

            #if DEBUG
            Console.WriteLine("Position Left hélicopère :" + Canvas.GetLeft(imgHelico));
            #endif


            foreach (var x in canvasJeu.Children.OfType<Rectangle>()) //enlever .OfType<Rectangle>() une fois les images ajoutées
            {
                //CAS : C'est un ennemi
                if ((string)x.Tag == "ennemi")
                {
                    // On le fait descendre
                    Canvas.SetTop(x, Canvas.GetTop(x) + 5);

                    // Rectangles de collision
                    Rect ennemiRect = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    Rect joueurRect = new Rect(Canvas.GetLeft(imgHelico), Canvas.GetTop(imgHelico), imgHelico.Width, imgHelico.Height);

                    // Si l'ennemi touche le joueur
                    if (joueurRect.IntersectsWith(ennemiRect))
                    {
                        magasinItemMouv.Add(x); // L'ennemi disparaît
                        damage += 5; // Aïe !
                    }
                    // Si l'ennemi sort de l'écran en bas
                    else if (Canvas.GetTop(x) > canvasJeu.ActualHeight)
                    {
                        magasinItemMouv.Add(x); // On le supprime pour libérer la mémoire
                    }
                }

                //C'est une balle (tir)
                if ((string)x.Tag == "balle")
                {
                    // La balle monte
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);

                    // Rectangles de collision
                    Rect balleRect = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    // Si la balle sort de l'écran en haut
                    if (Canvas.GetTop(x) < -20)
                    {
                        magasinItemMouv.Add(x);
                    }
                    else
                    {
                        // Vérifie si la balle touche un ennemi
                        foreach (var y in canvasJeu.Children.OfType<Rectangle>())
                        {
                            if ((string)y.Tag == "ennemi")
                            {
                                Rect ennemiRect = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                                if (balleRect.IntersectsWith(ennemiRect))
                                {
                                    magasinItemMouv.Add(x); // Supprime la balle
                                    magasinItemMouv.Add(y); // Supprime l'ennemi
                                    score++;
                                    AffichageScore();
                                }
                            }
                        }
                    }
                }
            }

            
            // On supprime vraiment les objets marqués
            foreach (UIElement i in magasinItemMouv)
            {
                canvasJeu.Children.Remove(i);
            }
            AnimationHelico();
        }
        
        public void AffichageScore()
        {
            labelScore.Content = score.ToString();
        }

        private void AnimationHelico() //Animation des hélices
        {
            nb_animation_helico++;
            if (nb_animation_helico == Helico1.Length * 4)
                nb_animation_helico = 0;
            if (nb_animation_helico % 4 == 0)
                imgHelico.Source = Helico1[nb_animation_helico / 4];
        }

        // Cette méthode crée un ennemi (carré rouge)
        private void MakeEnemies()
        {
            Rectangle nouveauEnnemi = new Rectangle
            {
                Tag = "ennemi", // Pour le reconnaître plus tard
                Height = 40,
                Width = 40,
                Fill = Brushes.Red, // A REMPLACER PAR UNE IMAGE PLUS TARD
                Stroke = Brushes.Black
            };

            // On le place aléatoirement en largeur (X)
            Canvas.SetLeft(nouveauEnnemi, rand.Next(0, (int)(canvasJeu.ActualWidth - 40)));

            // On le place juste au-dessus de l'écran en hauteur (Y)
            Canvas.SetTop(nouveauEnnemi, -50);

            // On l'ajoute au jeu
            canvasJeu.Children.Add(nouveauEnnemi);
        }

        //GESTION DES TOUCHES
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.KeyDown += canvasJeu_KeyDown;
                Application.Current.MainWindow.KeyUp += canvasJeu_KeyUp;
            }
            canvasJeu.Focus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.KeyDown -= canvasJeu_KeyDown;
                Application.Current.MainWindow.KeyUp -= canvasJeu_KeyUp;
            }
            movementTimer?.Stop();
        }

        private void canvasJeu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right) Adroite = true;
            else if (e.Key == Key.Left) Agauche = true;
        }

        private void canvasJeu_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right) Adroite = false;
            else if (e.Key == Key.Left) Agauche = false;  
        }
        private void CreerBalle()
        {
            Rectangle nouveauTir = new Rectangle
            {
                Tag = "balle",
                Height = 20,
                Width = 5,
                Fill = Brushes.OrangeRed,
                Stroke = Brushes.Yellow
            };

            // Place la balle
            Canvas.SetTop(nouveauTir, Canvas.GetTop(imgHelico) - nouveauTir.Height);
            Canvas.SetLeft(nouveauTir, Canvas.GetLeft(imgHelico) + imgHelico.Width / 2);

            // L'ajoute au jeu
            canvasJeu.Children.Add(nouveauTir);
        }

    }
}