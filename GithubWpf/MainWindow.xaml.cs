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

        private static int pasFond = 4;
        private void Jeu(object? sender, EventArgs e)
        {
            Deplace(imgFond1, pasFond);
            Deplace(imgFond2, pasFond);
        }

        public void Deplace(Image image, int pas)
        {
            Canvas.SetTop(image, Canvas.GetTop(image) + pas);
            if (Canvas.GetTop(image) >= image.Height)
                Canvas.SetTop(image, -image.ActualHeight + pas);
        }
        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;
            uc.butProfil.Click += AfficheChoixPerso;
            uc.butJouer.Click += AfficheJeu;
            uc.butBoutique.Click += AfficheBoutique;
            uc.butReglage.Click += AfficheReglages;
            uc.butReglesJeu.Click += AfficheReglesJeu;
        }

        private void AfficheDemarrage(object sender, RoutedEventArgs e)
        {
            UCDemarrage uc = new UCDemarrage();
            ZoneJeu.Content = uc;
            uc.butProfil.Click += AfficheChoixPerso;
            uc.butJouer.Click += AfficheJeu;
            uc.butBoutique.Click += AfficheBoutique;
            uc.butReglage.Click += AfficheReglages;
            uc.butReglesJeu.Click += AfficheReglesJeu;
        }

        private void AfficheChoixPerso(object sender, RoutedEventArgs e)
        {
            UCChoixPerso uc = new UCChoixPerso();
            ZoneJeu.Content = uc;
            uc.butRetourChoixPerso.Click += AfficheDemarrage;
        }
        private void AfficheBoutique(object sender, RoutedEventArgs e)
        {
            UCBoutique uc = new UCBoutique();
            ZoneJeu.Content = uc;
            uc.butRetourBoutique.Click += AfficheDemarrage;
        }
        private void AfficheJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
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

    }
}