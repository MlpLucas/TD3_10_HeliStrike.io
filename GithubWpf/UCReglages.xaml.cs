using System.Windows.Controls;

namespace GithubWpf
{
    /// <summary>
    /// Logique d'interaction pour UCReglages.xaml
    /// </summary>
    public partial class UCReglages : UserControl
    {
        #region INITIALISATION

        public UCReglages()
        {
            InitializeComponent();

            // 1. Positionne les sliders sur les valeurs actuelles (Sauvegardées dans MainWindow)
            // On multiplie par 10 car les sliders vont de 0 à 10, mais le volume va de 0.0 à 1.0
            sliderMaster.Value = MainWindow.VolumeGeneral * 10;
            sliderMusic.Value = MainWindow.VolumeMusique * 10;
            sliderSFX.Value = MainWindow.VolumeBruitages * 10;

            // 2. Détecte les changements
            // On utilise des "Lambda expressions" (=>) pour faire court (évite de créer une méthode utilisée qu'une fois)
            sliderMaster.ValueChanged += (s, e) => MainWindow.VolumeGeneral = sliderMaster.Value / 10;
            sliderMusic.ValueChanged += (s, e) => MainWindow.VolumeMusique = sliderMusic.Value / 10;
            sliderSFX.ValueChanged += (s, e) => MainWindow.VolumeBruitages = sliderSFX.Value / 10;
        }

        #endregion
    }
}