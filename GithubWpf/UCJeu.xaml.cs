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
using System.Media;
using System.Diagnostics.Eventing.Reader;

namespace GithubWpf
{
    // Classe simple pour représenter un ennemi avec sa propre vie
    public class Ennemi : Image
    {
        public int nbVieEnnemi { get; set; }
        // Ajout d'une propriété pour le type d'ennemi
        public string TypeEnnemi { get; set; } // <--- AJOUT

        public Ennemi()
        {
            Tag = "ennemi";
            TypeEnnemi = "meteor"; // <--- Par défaut, c'est une météorite
        }
    }

    /// <summary>
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        //Son
        private MediaPlayer SonTirJoueur = new MediaPlayer();
        private MediaPlayer SonMeteorDetruit = new MediaPlayer();
        private MediaPlayer MusiqueFond = new MediaPlayer();

        // Images et animations
        private BitmapImage[] Helico1 = new BitmapImage[6];
        private BitmapImage[] BarreDeVie = new BitmapImage[6];
        private BitmapImage[] Meteor = new BitmapImage[5];
        private BitmapImage Avion;

        // Timer
        private DispatcherTimer movementTimer;
        // Controle
        private static bool Agauche, Adroite;

        Random rand = new Random();
        int cadenceTir = 8;
        int tempsRecharge = 0;
        int nb_animation_helico = 0;
        int nb_animation_meteor = 0;
        int pointVie = 5; // Ne pas changer !!
        int enemieCounter;
        int limit = 50;
        double scoreLimit = 31.25 ,scoreTemp;
        // Vie propre à chaque météorite / avion
        private const int VieMeteorInitiale = 2;
        private const int VieAvionInitiale = 4;

        // Score minimum pour apparition
        private const int scoreMinApparitionAvion = 50; // A CHANGER PLUS TARD

        Rect playerHitBox;

        public UCJeu()
        {
            InitializeComponent();
            // Charge les images d'animation
            ChargeImageAnimation();
            // démarrage de la logique d'animation/déplacement
            InitTimer();
            //Lancement des sons
            InitialisationSonTirHelicoptere();
            LancerMusique();
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
                // Charge les images de la barre de vie
                for (int i = 0; i < BarreDeVie.Length; i++)
                {
                    BarreDeVie[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/BarreDeVie/Barre{i}.png"));
                }
                // Charger les images météorites
                for (int i = 0; i < Meteor.Length; i++)
                {
                    Meteor[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/Ennemis/meteor{i}.png"));
                }
                // Charger l'image avion
                Avion = new BitmapImage(new Uri($"pack://application:,,,/Images/Ennemis/Avion/avion1.png"));
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
                SonTirJoueur.Stop();

                // Formule : Master * Bruitages
                SonTirJoueur.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeBruitages;

                SonTirJoueur.Play();
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

            // --- Traiter les ennemis (Enemy) ---
            foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
            {
                // On le fait descendre
                Canvas.SetTop(ennemiImage, Canvas.GetTop(ennemiImage) + 5);

                // Rectangles de collision
                Rect ennemiRect = new Rect(Canvas.GetLeft(ennemiImage), Canvas.GetTop(ennemiImage), ennemiImage.Width, ennemiImage.Height);
                Rect joueurRect = new Rect(Canvas.GetLeft(imgHelico), Canvas.GetTop(imgHelico), imgHelico.Width, imgHelico.Height);

                // Si l'ennemi touche le joueur
                if (joueurRect.IntersectsWith(ennemiRect))
                {
                    magasinItemMouv.Add(ennemiImage); // L'ennemi disparaît
                    pointVie -= 1; // Aïe !
                    imgPointVie.Source = BarreDeVie[pointVie];
                    if (pointVie <= 0)
                        FinDeJeu();
                }
                // Si l'ennemi sort de l'écran en bas
                else if (Canvas.GetTop(ennemiImage) > canvasJeu.ActualHeight)
                {
                    magasinItemMouv.Add(ennemiImage); // On le supprime pour libérer la mémoire
                }
            }

            // --- Traiter les balles (Rectangles) et collisions avec ennemis (Enemy) ---
            foreach (Rectangle balle in canvasJeu.Children.OfType<Rectangle>())
            {
                string? tagBalle = balle.Tag as string;
                if (tagBalle == "balle")
                {
                    // La balle monte
                    Canvas.SetTop(balle, Canvas.GetTop(balle) - 20);

                    // Rectangles de collision
                    Rect balleRect = new Rect(Canvas.GetLeft(balle), Canvas.GetTop(balle), balle.Width, balle.Height);

                    // Si la balle sort de l'écran en haut
                    if (Canvas.GetTop(balle) < -20)
                    {
                        magasinItemMouv.Add(balle);
                    }
                    else
                    {
                        // Vérifie si la balle touche un ennemi
                        foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
                        {
                            Rect ennemiRect = new Rect(Canvas.GetLeft(ennemiImage), Canvas.GetTop(ennemiImage), ennemiImage.Width, ennemiImage.Height);

                            if (balleRect.IntersectsWith(ennemiRect))
                            {
                                // Vie propre à cet ennemi
                                ennemiImage.nbVieEnnemi--;
                                // Supprime la balle
                                magasinItemMouv.Add(balle);

                                if (ennemiImage.nbVieEnnemi <= 0)
                                {
                                    // Gestion du son de destruction
                                    SonMeteorDetruit.Stop();
                                    // Formule : Master * Bruitages
                                    SonMeteorDetruit.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeBruitages;
                                    SonMeteorDetruit.Play();

                                    // L'ennemi est détruit
                                    magasinItemMouv.Add(balle); // Supprime la balle
                                    magasinItemMouv.Add(ennemiImage); // Supprime l'ennemi
                                    MainWindow.Score += 3;
                                    AffichageScore();
                                    break; // la balle est détruite, sortir de la boucle ennemis
                                }
                            }
                        }
                    }
                }
            }

            scoreTemp--;
            if (scoreTemp <0)
            {
                MainWindow.Score++;
                scoreTemp = scoreLimit;
                AffichageScore();
            }

            
            // On supprime vraiment les objets marqués
            foreach (UIElement i in magasinItemMouv)
            {
                canvasJeu.Children.Remove(i);
            }
            AnimationHelico();
            AnimationMeteor();
            imgPointVie.Source = BarreDeVie[pointVie];
        }
        
        public void AffichageScore()
        {
            labelScore.Content = MainWindow.Score.ToString();
        }

        private void AnimationHelico() //Animation des hélices
        {
            nb_animation_helico++;
            if (nb_animation_helico == Helico1.Length * 4)
                nb_animation_helico = 0;
            if (nb_animation_helico % 4 == 0)
                imgHelico.Source = Helico1[nb_animation_helico / 4];
        }

        private void AnimationMeteor() //Animation des hélices
        {
            nb_animation_meteor++;
            if (nb_animation_meteor == Meteor.Length * 4)
                nb_animation_meteor = 0;

            // Ne changer d'image que tous les 4 ticks (comme pour l'hélico)
            if (nb_animation_meteor % 4 == 0)
            {
                BitmapImage frame = Meteor[nb_animation_meteor / 4];

                // Parcours de tous les Ennemi du Canvas et application au(x) ennemi(s)
                foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
                {
                    if (ennemiImage.TypeEnnemi == "meteor")
                    {
                        ennemiImage.Source = frame;
                    }
                }
            }
        }

        // Cette méthode crée un ennemi (Image) avec sa vie propre
        private void MakeEnemies()
        {
            if (MainWindow.Score < scoreMinApparitionAvion)
            {
                // Créer un meteor (Ennemi)
                Ennemi nouveauMeteor = new Ennemi
                {
                    Height = 128,
                    Width = 64,
                    Stretch = Stretch.UniformToFill,
                    nbVieEnnemi = VieMeteorInitiale,
                    TypeEnnemi = "meteor"
                };

                // On le place aléatoirement en largeur (X)
                Canvas.SetLeft(nouveauMeteor, rand.Next(100, (int)(canvasJeu.ActualWidth - 100)));
                // On le place juste au-dessus de l'écran en hauteur (Y)
                Canvas.SetTop(nouveauMeteor, -50);
                // On l'ajoute au jeu
                canvasJeu.Children.Add(nouveauMeteor);
            }
            else if (MainWindow.Score >= scoreMinApparitionAvion)
            {
                // Créer un avion (Ennemi)
                Ennemi nouvelAvion = new Ennemi
                {
                    Height = 160,
                    Width = 160,
                    Stretch = Stretch.UniformToFill,
                    Source = Avion,
                    nbVieEnnemi = VieAvionInitiale,
                    TypeEnnemi = "avion"
                };
                // On le place aléatoirement en largeur (X)
                Canvas.SetLeft(nouvelAvion, rand.Next(100, (int)(canvasJeu.ActualWidth - 100)));
                // On le place juste au-dessus de l'écran en hauteur (Y)
                Canvas.SetTop(nouvelAvion, -50);
                // On l'ajoute au jeu
                canvasJeu.Children.Add(nouvelAvion);
            }
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
        
        private void FinDeJeu()
        {
            movementTimer.Stop();
            MainWindow.FinJeu = true;
            Console.WriteLine("Fin du jeu UCJeu" + MainWindow.FinJeu);
        }

        private void InitialisationSonTirHelicoptere()
        {
            // MediaPlayer a besoin d'un chemin relatif simple (puisqu'on a mis le fichier en "Contenu")
            try
            {
                SonTirJoueur.Open(new Uri("Son/SonTir1.wav", UriKind.Relative));
                SonMeteorDetruit.Open(new Uri("Son/SonMeteorDetruit.wav", UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement son : " + ex.Message);
            }
        }
        private void LancerMusique()
        {
            MusiqueFond.Open(new Uri("Son/SonJeu.wav", UriKind.Relative));

            // Formule : Master * Musique
            MusiqueFond.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeMusique;

            MusiqueFond.MediaEnded += (s, e) => { MusiqueFond.Position = TimeSpan.Zero; MusiqueFond.Play(); }; // Boucle
            MusiqueFond.Play();
        }
    }
}