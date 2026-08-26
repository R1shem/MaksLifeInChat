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
        int _lastMinute = -1;
        int count_day = 0;
        int count_cable = 0;
        int count_meat = 0;
        SpriteCache cashe;
        Player player;
        Image playerSprite;
        List<Building> buildings = [];
        List<Image> buildingSprites = [];
        List<Chatters> chatters = [];
        List<Image> chatterSprites = [];
        List<Item> equipItems = [];
        List<Item> inventoryItems = [];
        string? selectBuilding;
        bool halalcartZone = false;

        public MainWindow()
        {
            App.mainWindow = this;
            InitializeComponent();
            MenuFrame menuFrame = new();
            gamePlayerMapGrid.Children.Add(menuFrame);
        }

        public async void StartGame()
        {
            cashe = new SpriteCache();
            cashe.GetUnitSpritesList(Constants.unitNames, Constants.states, Constants.rotations);
            cashe.GetItemSpritesList(Constants.itemNames);
            toolBarStackPanel.Visibility = Visibility.Visible;
            isPlay = true;
            SetInterfaceSprite();
            SpawnPlayer();
            await GameTimer();
        }

        void SetInterfaceSprite()
        {
            SetBuildingSprites();

        }

        void SetBuildingSprites()
        {
            building1Image.Source = cashe._unit[("wall", "stand", "down")][0];
            building2Image.Source = cashe._unit[("halalcart", "stand", "down")][0];
            buildingInterfaceImage.Source = cashe._item["building"];
        }

        void SetShawarmaSprites()
        {
            building1Image.Source = cashe._item["shawarmaHP"];
            building2Image.Source = cashe._item["shawarmaMP"];
            buildingInterfaceImage.Source = cashe._item["saucer"];
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
                    if (time.Minute != _lastMinute)
                    {
                        dayCountTB.Text = $"{count_day++} д.";
                        _lastMinute = time.Minute;
                        // сюда добавить условие
                    }
                }
                if (count_sprite_frame == Constants.SpriteUpdateFrameCount)
                {
                    count_sprite_frame = 0;
                    SpriteUpdate();
                }
                frameCountTB.Text = $"Frame: {count_frame}";
                timeCountTB.Text = $"Time: {time}:{time.Second}";
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
            double factSpeed = player.Speed;
            if (player.IsRun)
            {
                if (player.Stamina <= 1) // немного больше единицы, чтобы высота показателя не была отрицательной
                    player.IsRun = false;
                else
                {
                    factSpeed *= player.RunModificator;
                    player.Stamina -= player.StaminaConsuption;
                }
            }
            else
                if (player.Stamina < player.MaxStamina)
                    player.Stamina += player.RegenStamina;
            if (player.IsKara)
            {
                if (player.MP <= 1)
                {
                    player.IsKara = false;
                    player.Name = "kay";
                }
                else
                {
                    factSpeed *= player.KaraSpeedModificator;
                    player.MP -= player.KaraConsuption;
                }
            }
            else
                if (player.MP < player.MaxMP)
                    player.MP += player.RegenMP;
            if (player.HP < player.MaxHP)
                player.HP += player.RegenHP;

            mpRectangle.Height = player.MP / player.MaxMP * 120;
            hpRectangle.Height = player.HP / player.MaxHP * 120;
            staminaRectangle.Height = player.Stamina / player.MaxStamina * 120;

            switch (player.State) // для разных классов сделать отдельные методы
            {
                case "walk":
                    switch (player.Rotation)
                    {
                        case "left":
                            player.Coordinates = new (player.Coordinates.Left - factSpeed, player.Coordinates.Top, 0,0);
                            break;
                        case "right":
                            player.Coordinates = new(player.Coordinates.Left + factSpeed, player.Coordinates.Top, 0, 0);
                            break;
                        case "up":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - factSpeed, 0, 0);
                            break;
                        case "down":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + factSpeed, 0, 0);
                            break;
                    }
                    switch (player.SecondRotation)
                    {
                        case "left":
                            player.Coordinates = new(player.Coordinates.Left - factSpeed, player.Coordinates.Top, 0, 0);
                            break;
                        case "right":
                            player.Coordinates = new(player.Coordinates.Left + factSpeed, player.Coordinates.Top, 0, 0);
                            break;
                        case "up":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - factSpeed, 0, 0);
                            break;
                        case "down":
                            player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + factSpeed, 0, 0);
                            break;
                    }
                    if (player.Coordinates.Left <= -1850)
                        player.Coordinates = new(player.Coordinates.Left + factSpeed, player.Coordinates.Top, 0, 0);
                    else if (player.Coordinates.Left >= 1850)
                        player.Coordinates = new(player.Coordinates.Left - factSpeed, player.Coordinates.Top, 0, 0);
                    if (player.Coordinates.Top <= -910)
                        player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + factSpeed, 0, 0);
                    else if (player.Coordinates.Top >= 910)
                        player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - factSpeed, 0, 0);

                    playerSprite.Margin = player.Coordinates;
                    HalalCartCollision();
                    break;
            }
        }

        void HalalCartCollision() // блин, а ведь обычную коллизию стен тоже довольно легко сделать, но не хочу тратить время на переусложнение ии
        {
            if (buildings.Count!=0)
            {
                bool isExist = false;
                List<Building> halals = buildings.FindAll(x => x.Name == "halalcart");
                for (int i = 0; i < halals.Count; i++)
                {
                    if ((Math.Abs(player.Coordinates.Left - halals[i].Coordinates.Left) <= player.Size + halals[i].Size) && (Math.Abs(player.Coordinates.Top - halals[i].Coordinates.Top) <= player.Size + halals[i].Size)){
                        isExist = true;
                    }
                }
                if (isExist)
                {
                    if (!halalcartZone)
                    {
                        halalcartZone = true;
                        SetShawarmaSprites();
                    }
                }
                else
                {
                    if (halalcartZone)
                    {
                        halalcartZone = false;
                        SetBuildingSprites();
                    }
                }
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
            if (player.ProgressSprite >= cashe._unit[(player.Name, player.State, player.Rotation)].Count)
                player.ProgressSprite = 0;
            playerSprite.Source = cashe._unit[(player.Name, player.State, player.Rotation)][player.ProgressSprite];
            player.ProgressSprite++;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPlay)
            {
                UpdateDirections();
                if (Keyboard.IsKeyDown(Key.LeftShift) && !player.IsRun)
                    player.IsRun = true;
            }
            if (Keyboard.IsKeyDown(Key.F11))
            {
                if (this.WindowStyle == WindowStyle.None)
                {
                    this.WindowState = WindowState.Normal;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    this.WindowStyle = WindowStyle.None;
                }
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (isPlay)
            {
                UpdateDirections();
                if (!Keyboard.IsKeyDown(Key.LeftShift) && player.IsRun)
                    player.IsRun = false;
                if (e.Key == Key.LeftCtrl)  // отказательство от удержания в пользу нажатия
                {
                    if (player.IsKara)
                    {
                        player.IsKara = false;
                        player.Name = "kay";
                    }
                    else
                    {
                        player.IsKara = true;
                        player.Name = "kara";
                    }
                }
            }
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
                Source = cashe._unit[("kay", "stand", "down")][0]
            };
            gamePlayerMapGrid.Children.Add(playerSprite);
        }

        private void Button_Building1(object sender, RoutedEventArgs e)
        {
            BuildClick("wall");
        }

        private void Button_Building2(object sender, RoutedEventArgs e)
        {
            BuildClick("halalcart");
        }

        void BuildClick(string name)
        {
            if (!halalcartZone)
            {
                if (selectBuilding == name)
                {
                    selectBuilding = null;
                    mouseDynamicImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mouseDynamicImage.Visibility = Visibility.Visible;
                    selectBuilding = name;
                    int size;
                    if (selectBuilding == "halalcart")
                        size = Constants.SizeHalalcart;
                    else
                        size = Constants.SizeWall;
                    mouseDynamicImage.Width = size;
                    mouseDynamicImage.Height = size;
                    mouseDynamicImage.Source = cashe._unit[(name, "stand", "down")][0];
                }
            }
            else
            {
                // buy shawarma logic
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectBuilding != null)
            {
                Point p = e.GetPosition(mouseDynamicImageCanvas);
                Canvas.SetLeft(mouseDynamicImage, p.X - mouseDynamicImage.Width / 2);
                Canvas.SetTop(mouseDynamicImage, p.Y - mouseDynamicImage.Height / 2);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectBuilding != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point p = e.GetPosition(mouseDynamicImageCanvas);
                    SpawnBuilding(p, selectBuilding);
                    selectBuilding = null;
                    mouseDynamicImage.Visibility = Visibility.Collapsed;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    selectBuilding = null;
                    mouseDynamicImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        void SpawnBuilding(Point p, string name)
        {
            Thickness coordinates = new Thickness((p.X - 953) * 2, (p.Y - 527) * 2, 0, 0);

            Building building = new();
            building.Name = name;
            switch (selectBuilding)
            {
                case "halalcart":
                    building.Size = Constants.SizeHalalcart;
                    if (coordinates.Left <= -1850)
                        coordinates = new(-1700, coordinates.Top, 0, 0);
                    else if (coordinates.Left >= 1850)
                        coordinates = new(1700, coordinates.Top, 0, 0);
                    if (coordinates.Top <= -910)
                        coordinates = new(coordinates.Left, -850, 0, 0);
                    else if (coordinates.Top >= 910)
                        coordinates = new(coordinates.Left, 850, 0, 0);
                    break;
                case "wall":
                    building.Size = Constants.SizeWall;
                    if (coordinates.Left <= -1850)
                        coordinates = new(-1850, coordinates.Top, 0, 0);
                    else if (coordinates.Left >= 1850)
                        coordinates = new(1850, coordinates.Top, 0, 0);
                    if (coordinates.Top <= -960)
                        coordinates = new(coordinates.Left, -960, 0, 0);
                    else if (coordinates.Top >= 960)
                        coordinates = new(coordinates.Left, 960, 0, 0);
                    break;
            }
            building.Coordinates = coordinates;

            buildings.Add(building);
            Image buildingImage = new()
            {
                Margin = building.Coordinates,
                Width = building.Size,
                Height = building.Size,
                Source = cashe._unit[(building.Name, "stand", "down")][0]
            };
            buildingSprites.Add(buildingImage);
            gameBuildingMapGrid.Children.Add(buildingImage);
        }
    }
}