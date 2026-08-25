using MaksLifeInChat.Model;
using System.Numerics;
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
        int count_sprite_frame = 0;
        TimeOnly time = new TimeOnly(0, 0);
        SpriteCache cashe;
        Player player;
        Image playerSprite;
        List<Chatters> chatters = [];
        List<Image> chatterSprites = [];
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
            SpawnPlayer();
            await GameTimer();
        }

        private async Task GameTimer()
        {
            while (isPlay)
            {
                await Task.Delay(Constants.FPS);
                count_sprite_frame++;
                count_frame+= Constants.FPS;
                if (count_frame >= Constants.second)
                {
                    count_frame = 0;
                    time = time.Add(TimeSpan.FromSeconds(1));
                }
                if (count_sprite_frame == Constants.SpriteUpdateFrameCount)
                {
                    count_sprite_frame = 0;
                    SpriteUpdate();
                }
                frameCountTB.Text = $"Frame: {count_frame}";
                timeCountTB.Text = $"Time: {time}";
                spriteCountTB.Text = $"Sprite: {count_sprite_frame}";
                coordinatesTB.Text = $"Coordinates: {player.Coordinates}";
                GameUpdate();
            }
        }

        void GameUpdate()
        {
            PlayerGameUpdate();

        }

        void PlayerGameUpdate() // все методы GameUpdate закинуть в класс GameUpdate и испольлзовать статичные методы оттуда
        {
            switch (player.State) // для разных классов сделать отдельные методы
            {
                case "walk":
                    switch (player.Rotation)
                    {
                        case "left":
                            player.Coordinates = new (player.Coordinates.Left - player.Speed, player.Coordinates.Top, 0,0);
                            break;
                        case "right":
                            player.Coordinates = new(player.Coordinates.Left + player.Speed, player.Coordinates.Top, 0, 0);
                            break;
                        case "up":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - player.Speed, 0, 0);
                            break;
                        case "down":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + player.Speed, 0, 0);
                            break;
                    }
                    switch (player.SecondRotation)
                    {
                        case "left":
                            player.Coordinates = new(player.Coordinates.Left - player.Speed, player.Coordinates.Top, 0, 0);
                            break;
                        case "right":
                            player.Coordinates = new(player.Coordinates.Left + player.Speed, player.Coordinates.Top, 0, 0);
                            break;
                        case "up":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - player.Speed, 0, 0);
                            break;
                        case "down":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + player.Speed, 0, 0);
                            break;
                    }
                    playerSprite.Margin = player.Coordinates;
                    break;
            }
        }

        void ChattersGameUpdate() 
        {

        }

        void NewbieGameUpdate() 
        {

        }

        void ModeratorGameUpdate() 
        {

        }

        void SpriteUpdate()
        {
            if (player.ProgressSprite >= cashe._cache[(player.Name, player.State, player.Rotation)].Count)
                player.ProgressSprite = 0;
            playerSprite.Source = cashe._cache[(player.Name, player.State, player.Rotation)][player.ProgressSprite];
            player.ProgressSprite++;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateDirections();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateDirections();
        }

        private void UpdateDirections()
        {
            var pressed = new List<string>();
            if (Keyboard.IsKeyDown(Key.W)) pressed.Add("up");
            if (Keyboard.IsKeyDown(Key.A)) pressed.Add("left");
            if (Keyboard.IsKeyDown(Key.S)) pressed.Add("down");
            if (Keyboard.IsKeyDown(Key.D)) pressed.Add("right");

            if (pressed.Count == 0)
            {
                player.State = "stand";
                player.ProgressSprite = 0;
                player.SecondRotation = null;
            }
            else
            {
                player.State = "walk";
                player.Rotation = pressed[0];
                player.SecondRotation = pressed.Count >= 2 ? pressed[1] : null;
            }
        }

        void SpawnPlayer()
        {
            player = new();
            playerSprite = new()
            {
                Width = player.Size,
                Height = player.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = cashe._cache[("kay", "stand", "down")][0]
            };
            gameMapGrid.Children.Add(playerSprite);
        }
    }
}