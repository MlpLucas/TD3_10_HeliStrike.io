using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GithubWpf
{
    // --- CLASSE ENNEMI ---
    public class Ennemi : Image
    {
        public int nbVieEnnemi { get; set; }
        public string TypeEnnemi { get; set; }
        public int scoreBonus { get; set; }

        public Ennemi()
        {
            Tag = "ennemi";
            TypeEnnemi = "meteor";
        }
    }

    /// <summary>
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        #region 1. VARIABLES & CONFIGURATION

        // --- SON ---
        private readonly MediaPlayer SonTirJoueur = new();
        private readonly MediaPlayer SonMeteorDetruit = new();
        private readonly MediaPlayer MusiqueFondJeu = new();

        // --- IMAGES ---
        private readonly BitmapImage[] Helico1 = new BitmapImage[6];
        private readonly BitmapImage[] BarreDeVie = new BitmapImage[6];
        private readonly BitmapImage[] Meteor = new BitmapImage[5];
        private BitmapImage Avion;

        // --- TIMER & CONTROLES ---
        private DispatcherTimer movementTimer;
        private static bool Agauche, Adroite;
        Random rand = new Random();

        // --- PARAMETRES ARME (Chargeur) ---
        int cadenceTir = 8;
        int tempsRecharge = 0;
        int tailleChargeur = 10;
        int ballesRestantes = 10;
        int tempsRechargementArme = 0;
        int dureePauseRechargement = 40;

        // --- PARAMETRES JEU ---
        int pointVie = 5;
        int enemieCounter;
        int limit = 50;
        double scoreLimit = 31.25, scoreTemp;

        // --- CONSTANTES ENNEMIS ---
        private const int VieMeteorInitiale = 2;
        private const int VieAvionInitiale = 3;
        private const int scoreMinApparitionAvion = 70;
        private const int bonusScoreMeteor = 5;
        private const int bonusScoreAvion = 10;

        // --- ANIMATIONS & UI ---
        int nb_animation_helico = 0;
        int nb_animation_meteor = 0;
        private bool alerteAfficheLabelNouveauEnnemis = false;

        #endregion

        public UCJeu()
        {
            InitializeComponent();

            // Initialisations
            ChargeImageAnimation();
            InitTimer();
            InitialisationSon();
            LancerMusique();

            // Evénements
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;

            // Focus Clavier
            canvasJeu.Focusable = true;
            canvasJeu.Focus();
        }

        #region 2. INITIALISATION (Images, Sons, Timer)

        private void ChargeImageAnimation()
        {
            try
            {
                for (int i = 0; i < Helico1.Length; i++)
                    Helico1[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/Helicoptere/helico{MainWindow.Perso}-{i + 1}.png"));
                for (int i = 0; i < BarreDeVie.Length; i++)
                    BarreDeVie[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/BarreDeVie/Barre{i}.png"));
                for (int i = 0; i < Meteor.Length; i++)
                    Meteor[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/Ennemis/meteor{i}.png"));

                Avion = new BitmapImage(new Uri($"pack://application:,,,/Images/Ennemis/Avion/avion1.png"));
            }
            catch { MessageBox.Show("Attention : Images manquantes dans le dossier !"); }
        }

        private void InitialisationSon()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                SonTirJoueur.Open(new Uri(System.IO.Path.Combine(baseDir, "Son", "SonTir1.wav")));
                SonMeteorDetruit.Open(new Uri(System.IO.Path.Combine(baseDir, "Son", "SonMeteorDetruit.wav")));
            }
            catch (Exception ex) { MessageBox.Show("Erreur chargement son : " + ex.Message); }
        }

        private void LancerMusique()
        {
            try
            {
                string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Son", "SonJeu.wav");
                if (File.Exists(chemin))
                {
                    MusiqueFondJeu.Open(new Uri(chemin));
                    MusiqueFondJeu.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeMusique;
                    MusiqueFondJeu.MediaEnded += (s, e) => { MusiqueFondJeu.Position = TimeSpan.Zero; MusiqueFondJeu.Play(); };
                    MusiqueFondJeu.Play();
                }
            }
            catch { }
        }

        private void InitTimer()
        {
            movementTimer = new DispatcherTimer();
            movementTimer.Interval = TimeSpan.FromMilliseconds(20);
            movementTimer.Tick += MovementTimer_Tick;
            movementTimer.Start();
        }

        #endregion

        #region 3. BOUCLE PRINCIPALE (Logique du Jeu)

        private void MovementTimer_Tick(object? sender, EventArgs e)
        {
            // --- A. UI & DIFFICULTÉ ---
            AffichageNouveauxEnnemis();

            int vitesseActuelle = 4 + (MainWindow.Score / 300);
            limit = Math.Max(20, 40 - (MainWindow.Score / 50));


            // --- B. GESTION TIR ET RECHARGEMENT ---
            if (tempsRechargementArme > 0) tempsRechargementArme--;
            if (tempsRecharge > 0) tempsRecharge--;

            if (Keyboard.IsKeyDown(Key.Space) && tempsRecharge <= 0 && tempsRechargementArme <= 0)
            {
                SonTirJoueur.Stop();
                SonTirJoueur.Position = TimeSpan.Zero;
                SonTirJoueur.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeBruitages;
                SonTirJoueur.Play();

                CreerBalle();
                tempsRecharge = cadenceTir;

                ballesRestantes--;
                if (ballesRestantes <= 0)
                {
                    tempsRechargementArme = dureePauseRechargement;
                    ballesRestantes = tailleChargeur;
                }
            }


            // --- C. DEPLACEMENT JOUEUR ---
            double left = Canvas.GetLeft(imgHelico);
            if (double.IsNaN(left)) left = 0;

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


            // --- D. SPAWN ENNEMIS ---
            enemieCounter--;
            if (enemieCounter < 0)
            {
                MakeEnemies();
                enemieCounter = limit;
            }


            // --- E. COLLISIONS & PHYSIQUE ---
            List<UIElement> magasinItemMouv = new List<UIElement>();

            // 1. Gestion des Ennemis
            foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
            {
                Canvas.SetTop(ennemiImage, Canvas.GetTop(ennemiImage) + vitesseActuelle);

                Rect ennemiRect = new Rect(Canvas.GetLeft(ennemiImage), Canvas.GetTop(ennemiImage), ennemiImage.Width, ennemiImage.Height);
                Rect joueurRect = new Rect(Canvas.GetLeft(imgHelico), Canvas.GetTop(imgHelico), imgHelico.Width, imgHelico.Height);

                if (joueurRect.IntersectsWith(ennemiRect))
                {
                    magasinItemMouv.Add(ennemiImage);
                    pointVie -= 1;
                    if (pointVie >= 0 && pointVie < BarreDeVie.Length) imgPointVie.Source = BarreDeVie[pointVie];
                    if (pointVie <= 0) FinDeJeu();
                }
                else if (Canvas.GetTop(ennemiImage) > canvasJeu.ActualHeight)
                {
                    magasinItemMouv.Add(ennemiImage);
                }
            }

            // 2. Gestion des Balles
            foreach (Rectangle balle in canvasJeu.Children.OfType<Rectangle>())
            {
                if ((string)balle.Tag == "balle")
                {
                    Canvas.SetTop(balle, Canvas.GetTop(balle) - 20);
                    Rect balleRect = new Rect(Canvas.GetLeft(balle), Canvas.GetTop(balle), balle.Width, balle.Height);

                    if (Canvas.GetTop(balle) < -20)
                    {
                        magasinItemMouv.Add(balle);
                    }
                    else
                    {
                        foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
                        {
                            Rect ennemiRect = new Rect(Canvas.GetLeft(ennemiImage), Canvas.GetTop(ennemiImage), ennemiImage.Width, ennemiImage.Height);

                            if (balleRect.IntersectsWith(ennemiRect))
                            {
                                ennemiImage.nbVieEnnemi--;
                                magasinItemMouv.Add(balle);

                                if (ennemiImage.nbVieEnnemi <= 0)
                                {
                                    SonMeteorDetruit.Stop();
                                    SonMeteorDetruit.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeBruitages;
                                    SonMeteorDetruit.Play();

                                    magasinItemMouv.Add(ennemiImage);
                                    MainWindow.Score += ennemiImage.scoreBonus;
                                    AffichageScore();
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // --- F. SCORE TEMPOREL & NETTOYAGE ---
            scoreTemp--;
            if (scoreTemp < 0)
            {
                MainWindow.Score++;
                scoreTemp = scoreLimit;
                AffichageScore();
            }

            foreach (UIElement i in magasinItemMouv)
            {
                canvasJeu.Children.Remove(i);
            }

            // --- G. ANIMATIONS ---
            AnimationHelico();
            AnimationMeteor();
        }

        #endregion

        #region 4. METHODES UTILITAIRES

        private void MakeEnemies()
        {
            // PHASE 1 : Si le score est faible, on ne met que des météorites
            if (MainWindow.Score < scoreMinApparitionAvion)
            {
                Ennemi nouveauMeteor = new Ennemi
                {
                    Height = 128,
                    Width = 64,
                    Stretch = Stretch.UniformToFill,
                    nbVieEnnemi = VieMeteorInitiale,
                    TypeEnnemi = "meteor",
                    scoreBonus = bonusScoreMeteor
                };
                Canvas.SetLeft(nouveauMeteor, rand.Next(100, (int)(canvasJeu.ActualWidth - 100)));
                Canvas.SetTop(nouveauMeteor, -50);
                canvasJeu.Children.Add(nouveauMeteor);
            }
            // PHASE 2 : Si le score est élevé, on mélange (50% chance chacun)
            else
            {
                // Pile ou Face : 0 = Météorite, 1 = Avion
                if (rand.Next(0, 2) == 0)
                {
                    // --- C'est une Météorite (Même code qu'au dessus) ---
                    Ennemi nouveauMeteor = new Ennemi
                    {
                        Height = 128,
                        Width = 64,
                        Stretch = Stretch.UniformToFill,
                        nbVieEnnemi = VieMeteorInitiale,
                        TypeEnnemi = "meteor",
                        scoreBonus = bonusScoreMeteor
                    };
                    Canvas.SetLeft(nouveauMeteor, rand.Next(100, (int)(canvasJeu.ActualWidth - 100)));
                    Canvas.SetTop(nouveauMeteor, -50);
                    canvasJeu.Children.Add(nouveauMeteor);
                }
                else
                {
                    // --- C'est un Avion ---
                    Ennemi nouvelAvion = new Ennemi
                    {
                        Height = 160,
                        Width = 160,
                        Stretch = Stretch.UniformToFill,
                        Source = Avion,
                        nbVieEnnemi = VieAvionInitiale,
                        TypeEnnemi = "avion",
                        scoreBonus = bonusScoreAvion
                    };
                    Canvas.SetLeft(nouvelAvion, rand.Next(100, (int)(canvasJeu.ActualWidth - 100)));
                    Canvas.SetTop(nouvelAvion, -50);
                    canvasJeu.Children.Add(nouvelAvion);
                }
            }
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
            Canvas.SetTop(nouveauTir, Canvas.GetTop(imgHelico) - nouveauTir.Height);
            Canvas.SetLeft(nouveauTir, Canvas.GetLeft(imgHelico) + imgHelico.Width / 2);
            canvasJeu.Children.Add(nouveauTir);
        }

        private void AnimationHelico()
        {
            nb_animation_helico++;
            if (nb_animation_helico >= Helico1.Length * 4) nb_animation_helico = 0;
            if (nb_animation_helico % 4 == 0) imgHelico.Source = Helico1[nb_animation_helico / 4];
        }

        private void AnimationMeteor()
        {
            nb_animation_meteor++;
            if (nb_animation_meteor >= Meteor.Length * 4) nb_animation_meteor = 0;

            if (nb_animation_meteor % 4 == 0)
            {
                BitmapImage frame = Meteor[nb_animation_meteor / 4];
                foreach (Ennemi ennemiImage in canvasJeu.Children.OfType<Ennemi>())
                {
                    if (ennemiImage.TypeEnnemi == "meteor") ennemiImage.Source = frame;
                }
            }
        }

        public void AffichageScore()
        {
            labelScore.Content = MainWindow.Score.ToString();
        }

        public async void AffichageNouveauxEnnemis()
        {
            if (MainWindow.Score >= scoreMinApparitionAvion && !alerteAfficheLabelNouveauEnnemis)
            {
                alerteAfficheLabelNouveauEnnemis = true;
                labelNouveauxEnnemis.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                labelNouveauxEnnemis.Visibility = Visibility.Collapsed;
            }
        }

        private void FinDeJeu()
        {
            movementTimer.Stop();
            MusiqueFondJeu.Stop();
            MusiqueFondJeu.Close();
            MainWindow.FinJeu = true;
            Console.WriteLine("Fin du jeu UCJeu" + MainWindow.FinJeu);
        }

        #endregion

        #region 5. GESTION CLAVIER & EVENEMENTS

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
            MusiqueFondJeu.Stop();
            MusiqueFondJeu.Close();
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

        #endregion
    }
}