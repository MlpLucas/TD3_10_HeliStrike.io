using System;
using System.IO; // Nécessaire pour Path et File
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCFinJeu.xaml
    /// </summary>
    public partial class UCFinJeu : UserControl
    {
        #region 1. VARIABLES

        private readonly MediaPlayer MusiqueFondFinJeu = new();

        #endregion

        #region 2. INITIALISATION ET FERMETURE

        public UCFinJeu()
        {
            InitializeComponent();

            // Lancement des affichages et du son
            AffichageScore();
            AffichageMeilleurScore();
            LancerMusique();

            // Evénement de fermeture pour couper le son
            this.Unloaded += UCFinJeu_Unloaded;
        }

        private void UCFinJeu_Unloaded(object sender, RoutedEventArgs e)
        {
            MusiqueFondFinJeu.Stop();  // On arrête
            MusiqueFondFinJeu.Close(); // On nettoie la mémoire
        }

        #endregion

        #region 3. GESTION DU SCORE

        public void AffichageScore()
        {
            labelScore.Content = MainWindow.Score.ToString();
        }

        private void AffichageMeilleurScore()
        {
            if (MainWindow.Score > MainWindow.MeilleurScore)
            {
                MainWindow.MeilleurScore = MainWindow.Score;
                labelAfficheMeilleurScore.Content = "Nouveau meilleur score !";
            }
            labelMeilleurScore.Content = MainWindow.MeilleurScore.ToString();

            // Réinitialiser le score pour la prochaine partie
            MainWindow.Score = 0;
        }

        #endregion

        #region 4. GESTION AUDIO

        private void LancerMusique()
        {
            try
            {
                // Utilisation de chemin absolu pour la sécurité (comme dans UCJeu)
                string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Son", "SonFinJeu.wav");

                if (System.IO.File.Exists(chemin))
                {
                    MusiqueFondFinJeu.Open(new Uri(chemin));

                    // Formule : Master * Musique
                    MusiqueFondFinJeu.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeMusique;

                    // Boucle de musique
                    MusiqueFondFinJeu.MediaEnded += (s, e) =>
                    {
                        MusiqueFondFinJeu.Position = TimeSpan.Zero;
                        MusiqueFondFinJeu.Play();
                    };

                    MusiqueFondFinJeu.Play();
                }
            }
            catch { /* Ignorer si erreur son */ }
        }

        #endregion
    }
}