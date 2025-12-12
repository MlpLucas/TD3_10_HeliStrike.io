using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DispatcherTimer minuterie;
   
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            AfficheDemarrage();
        }

        private void InitializeTimer()
        {
            minuterie = new DispatcherTimer();
            // configure l'intervalle du Timer
            minuterie.Interval = TimeSpan.FromMilliseconds(16);
            // associe l’appel de la méthode Jeu à la fin de la minuterie
            minuterie.Tick += Jeu;
            // lancement du timer
            minuterie.Start();
        }

        //Choix du personnage + vitesse hélico
        public static string Perso { get; set; } = "1";
        public static int PasHelico { get; set; } = 8;

        public static bool FinJeu { get; set; } = false;

        //Animation background
        private static int pasFond = 4;
        private void Jeu(object? sender, EventArgs e)
        {
            Deplace(imgFond1, pasFond);
            Deplace(imgFond2, pasFond);


            if (MainWindow.FinJeu == true)
            {
                AfficheFinJeu();
                Console.WriteLine("Le jeu est terminé. mainWindow");
            }
        }

        public void Deplace(Image image, int pas)
        {
            Canvas.SetTop(image, Canvas.GetTop(image) + pas);
            if (Canvas.GetTop(image) >= image.Height)
                Canvas.SetTop(image, -image.ActualHeight + pas);
        }

        //Boutton Navigation entre les UserControl
        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;

            uc.butJouer.Click += AfficheJeu;
            uc.butProfil.Click += AfficheChoixPerso;
            uc.butReglages.Click += AfficheReglages;
            uc.butReglesJeu.Click += AfficheReglesJeu;
        }

        private void AfficheDemarrage(object sender, RoutedEventArgs e)
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;

            uc.butJouer.Click += AfficheJeu;
            uc.butProfil.Click += AfficheChoixPerso;
            uc.butReglages.Click += AfficheReglages;
            uc.butReglesJeu.Click += AfficheReglesJeu;
        }
        private void AfficheChoixPerso(object sender, RoutedEventArgs e)
        {
            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;
            uc.butRetourChoixPerso.Click += AfficheDemarrage;
        }
        private void AfficheReglages(object sender, RoutedEventArgs e)
        {
            UCReglages uc = new UCReglages();
            ZoneJeu.Content = uc;
            uc.butRetourReglages.Click += AfficheDemarrage;
        }
        private void AfficheReglesJeu(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;
            uc.butRetourReglesJeu.Click += AfficheDemarrage;
        }
        private void AfficheJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
        }

        private void AfficheFinJeu()
        {
            UCFinJeu uc = new UCFinJeu();
            ZoneJeu.Content = uc;
        }
    }
}