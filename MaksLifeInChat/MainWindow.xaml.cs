using MaksLifeInChat.Model;
using System.Text;
using System.Timers;
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
        bool isPlay = false;
        int count_frame = 0; // frame count (не отражает время)
        TimeOnly time = new TimeOnly(0, 0);
        SpriteCache cashe;
        public MainWindow()
        {
            App.mainWindow = this;
            InitializeComponent();
            MenuFrame menuFrame = new();
            gameMapGrid.Children.Add(menuFrame);
        }

        public async void StartGame()
        {
            cashe = new SpriteCache();
            cashe.GetSpritesList(Constants.unitNames, Constants.states, Constants.rotations);
            isPlay = true;
            await GameTimer();
        }

        private async Task GameTimer()
        {
            while (isPlay)
            {
                await Task.Delay(Constants.FPS);
                count_frame+= Constants.FPS;
                if (count_frame >= Constants.second)
                {
                    count_frame = 0;
                    time = time.Add(TimeSpan.FromSeconds(1));
                }
                GameUpdate();
            }
        }

        void GameUpdate()
        {

        }
    }
}