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
    /// Logique d'interaction pour UCReglages.xaml
    /// </summary>
    public partial class UCReglages : UserControl
    {
        public UCReglages()
        {
            InitializeComponent();
            //Positionne les sliders sur les valeurs actuelles
            sliderMaster.Value = MainWindow.VolumeGeneral * 10;
            sliderMusic.Value = MainWindow.VolumeMusique * 10;
            sliderSFX.Value = MainWindow.VolumeBruitages * 10;

            //Regarde les changements
            sliderMaster.ValueChanged += (s, e) => MainWindow.VolumeGeneral = sliderMaster.Value / 10;
            sliderMusic.ValueChanged += (s, e) => MainWindow.VolumeMusique = sliderMusic.Value / 10;
            sliderSFX.ValueChanged += (s, e) => MainWindow.VolumeBruitages = sliderSFX.Value / 10;
        }
    }
    
}
