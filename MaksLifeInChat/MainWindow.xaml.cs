using MaksLifeInChat.Model;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MaksLifeInChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpriteCache cashe;
        public MainWindow()
        {
            App.mainWindow = this;
            InitializeComponent();
            MenuFrame menuFrame = new();
            gameMapGrid.Children.Add(menuFrame);
        }

        public void StartGame()
        {
            cashe = new SpriteCache();
            cashe.GetSpritesList(Constants.unitNames, Constants.states, Constants.rotations);
        }
    }
}