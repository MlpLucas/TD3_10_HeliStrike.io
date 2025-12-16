using System;
using System.IO; // Nécessaire pour Path et File
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCDemarrage.xaml
    /// </summary>
    public partial class UCDemarrage : UserControl
    {
        #region 1. VARIABLES

        private MediaPlayer MusiqueFondDemarrage = new MediaPlayer();

        #endregion

        #region 2. INITIALISATION ET FERMETURE

        public UCDemarrage()
        {
            InitializeComponent();

            // Gestion de la fermeture (pour couper le son)
            this.Unloaded += UCDemarrage_Unloaded;

            // Lancement de la musique
            LancerMusique();
        }

        private void UCDemarrage_Unloaded(object sender, RoutedEventArgs e)
        {
            MusiqueFondDemarrage.Stop();
            MusiqueFondDemarrage.Close();
        }

        #endregion

        #region 3. GESTION AUDIO

        private void LancerMusique()
        {
            try
            {
                // Utilisation de chemin absolu pour la sécurité (comme dans les autres fichiers)
                string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Son", "SonDemarrage.wav");

                if (File.Exists(chemin))
                {
                    MusiqueFondDemarrage.Open(new Uri(chemin));

                    // Formule : Master * Musique
                    MusiqueFondDemarrage.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeMusique;

                    // Boucle de musique
                    MusiqueFondDemarrage.MediaEnded += (s, e) =>
                    {
                        MusiqueFondDemarrage.Position = TimeSpan.Zero;
                        MusiqueFondDemarrage.Play();
                    };

                    MusiqueFondDemarrage.Play();
                }
            }
            catch { /* Ignorer les erreurs de son */ }
        }

        #endregion
    }
}