using System.Windows;
using System.Windows.Controls;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCChoixPerso.xaml
    /// </summary>
    public partial class UCChoixPerso : UserControl
    {
        #region INITIALISATION

        public UCChoixPerso()
        {
            InitializeComponent();
        }

        #endregion

        #region GESTION DES CLICS

        private void rbHelico1_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Perso = "1";
        }

        private void rbHelico2_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Perso = "2";
        }

        #endregion
    }
}