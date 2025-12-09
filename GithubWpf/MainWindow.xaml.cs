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
    }
}