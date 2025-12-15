using System;
using System.Collections.Generic;
using System.Linq;
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

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCDemarrage.xaml
    /// </summary>
    public partial class UCDemarrage : UserControl
    {
        private MediaPlayer MusiqueFondDemarrage = new MediaPlayer();
        public UCDemarrage()
        {
            InitializeComponent();
            this.Unloaded += UCDemarrage_Unloaded;
            LancerMusique();

        }
        //Gestion du son de fond dans le demarrage
        private void LancerMusique()
        {
            MusiqueFondDemarrage.Open(new Uri("Son/SonDemarrage.wav", UriKind.Relative));

            // Formule : Master * Musique
            MusiqueFondDemarrage.Volume = MainWindow.VolumeGeneral * MainWindow.VolumeMusique;

            MusiqueFondDemarrage.MediaEnded += (s, e) => { MusiqueFondDemarrage.Position = TimeSpan.Zero; MusiqueFondDemarrage.Play(); }; // Boucle
            MusiqueFondDemarrage.Play();
        }
        private void UCDemarrage_Unloaded(object sender, RoutedEventArgs e)
        {
            MusiqueFondDemarrage.Stop();
            MusiqueFondDemarrage.Close();
        }
    }

}
