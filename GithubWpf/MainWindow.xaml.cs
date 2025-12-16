using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GithubWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 1. VARIABLES GLOBALES (STATIC)

        // --- SON ---
        public static double VolumeGeneral { get; set; } = 0.5;
        public static double VolumeMusique { get; set; } = 0.5;
        public static double VolumeBruitages { get; set; } = 0.5;

        // --- PARAMETRES JOUEUR ---
        public static string Perso { get; set; } = "1";
        public static int PasHelico { get; set; } = 8;

        // --- ETAT DU JEU ---
        public static bool FinJeu { get; set; } = false;
        public static int Score { get; set; } = 0;
        public static int MeilleurScore { get; set; } = 0;

        #endregion

        #region 2. INITIALISATION ET TIMER

        private static DispatcherTimer minuterie;
        private static int pasFond = 4;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            AfficheDemarrage();
        }

        private void InitializeTimer()
        {
            minuterie = new DispatcherTimer();
            // Environ 60 FPS (1000ms / 60 = 16.6ms)
            minuterie.Interval = TimeSpan.FromMilliseconds(16);
            minuterie.Tick += BouclePrincipale_Tick;
            minuterie.Start();
        }

        private void BouclePrincipale_Tick(object? sender, EventArgs e)
        {
            // Animation du fond
            Deplace(imgFond1, pasFond);
            Deplace(imgFond2, pasFond);

            // Vérification Fin de Jeu
            if (MainWindow.FinJeu == true)
            {
                AfficheFinJeu();
            }
        }

        public void Deplace(Image image, int pas)
        {
            Canvas.SetTop(image, Canvas.GetTop(image) + pas);
            if (Canvas.GetTop(image) >= image.Height)
                Canvas.SetTop(image, -image.ActualHeight + pas);
        }

        #endregion

        #region 3. NAVIGATION (GESTION DES ECRANS)

        // --- ECRAN DEMARRAGE ---
        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;

            // Abonnement aux boutons du menu
            uc.butJouer.Click += AfficheJeu;
            uc.butProfil.Click += AfficheChoixPerso;
            uc.butReglages.Click += AfficheReglages;
            uc.butReglesJeu.Click += AfficheReglesJeu;
        }

        // Surcharge pour le bouton retour (Redirige vers la méthode principale)
        private void AfficheDemarrage(object sender, RoutedEventArgs e)
        {
            AfficheDemarrage();
        }

        // --- ECRAN JEU ---
        private void AfficheJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
            uc.Focus(); // Important pour capter le clavier
        }

        // --- ECRAN FIN DE JEU ---
        private void AfficheFinJeu()
        {
            MainWindow.FinJeu = false; // On reset l'état

            UCFinJeu uc = new UCFinJeu();
            ZoneJeu.Content = uc;

            uc.butRejouer.Click += AfficheJeu;
            uc.butQuitter.Click += AfficheDemarrage;
        }

        // --- ECRAN CHOIX PERSO ---
        private void AfficheChoixPerso(object sender, RoutedEventArgs e)
        {
            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;
            uc.butRetourChoixPerso.Click += AfficheDemarrage;
        }

        // --- ECRAN REGLAGES ---
        private void AfficheReglages(object sender, RoutedEventArgs e)
        {
            UCReglages uc = new UCReglages();
            ZoneJeu.Content = uc;
            uc.butRetourReglages.Click += AfficheDemarrage;
        }

        // --- ECRAN REGLES ---
        private void AfficheReglesJeu(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;
            uc.butRetourReglesJeu.Click += AfficheDemarrage;
        }

        #endregion
    }
}