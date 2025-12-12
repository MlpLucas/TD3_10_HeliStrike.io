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
using static System.Formats.Asn1.AsnWriter;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCFinJeu.xaml
    /// </summary>
    public partial class UCFinJeu : UserControl
    {
        public UCFinJeu()
        {
            InitializeComponent();
            AffichageScore();
            AffichageMeilleurScore();
        }

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
        }
    }
}
