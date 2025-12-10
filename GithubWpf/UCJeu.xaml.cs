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
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        private static BitmapImage Helico1;
        public UCJeu()
        {
            InitializeComponent();
            Helico1 = new BitmapImage(new Uri($"pack://application:,,,/Images/Helicoptere/helico{MainWindow.Perso}-1.png"));
            imgHelico.Source = Helico1;

        }

        //Abonné keyDown et keyUp de la mainWindow via Loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.KeyDown += canvasJeu_KeyDown;
            Application.Current.MainWindow.KeyUp += canvasJeu_KeyUp;
        }

        private void canvasJeu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (Canvas.GetLeft(imgHelico) < (canvasJeu.ActualWidth - imgHelico.Width))
                {
                    imgHelico.Source = Helico1;
                    Canvas.SetLeft(imgHelico, Canvas.GetLeft(imgHelico) + MainWindow.PasHelico);
                }
            }

            if (e.Key == Key.Left)
            {
                if (Canvas.GetLeft(imgHelico) > 0)
                {
                    imgHelico.Source = Helico1;
                    Canvas.SetLeft(imgHelico, Canvas.GetLeft(imgHelico) - MainWindow.PasHelico);
                }
            }
#if DEBUG
            Console.WriteLine("Position Left pere noel :" + Canvas.GetLeft(imgHelico));
#endif

        }

        private void canvasJeu_KeyUp(object sender, KeyEventArgs e)
        {

        }

    }
}
