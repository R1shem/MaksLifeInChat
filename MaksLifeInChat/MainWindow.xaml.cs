using MaksLifeInChat.Model;
using System.Numerics;
using System.Security.Policy;
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
        bool isBoss = false;
        int count_frame = 0; // frame count (не отражает время)
        int count_sprite_frame = 0;
        int count_spawn_newbie_delay = 0;
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
        List<Newbie> newbies = [];
        List<Image> newbieSprites = [];
        List<Chatters> chatters = [];
        List<Image> chatterSprites = [];
        string? selectBuilding;
        bool halalcartZone = false;
        string stateBefore;

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
            cashe.GetItemSpritesList(Constants.interfaceNames);
            toolBarStackPanel.Visibility = Visibility.Visible;
            isPlay = true;
            SetInterfaceSprite();
            SpawnPlayer();
            await GameTimer();
        }

        void SetInterfaceSprite()
        {
            SetBuildingSprites();
            inventoryInterfaceImage.Source = cashe._item["inventory"];
            handInterfaceImage.Source = cashe._item["hand"];
        }

        void SetBuildingSprites()
        {
            building1Image.Source = cashe._unit[("wall", "stand", "down")][0];
            building2Image.Source = cashe._unit[("halalcart", "stand", "down")][0];
            buildingInterfaceImage.Source = cashe._item["building"];
            buildingButton1.ToolTip = Constants.WallDescription;
            buildingButton2.ToolTip = Constants.HalalcartDescription;
        }

        void SetShawarmaSprites()
        {
            building1Image.Source = cashe._item["shawarmaHP"];
            building2Image.Source = cashe._item["shawarmaMP"];
            buildingInterfaceImage.Source = cashe._item["saucer"];
            buildingButton1.ToolTip = Constants.ShawarmaHPDescription;
            buildingButton2.ToolTip = Constants.ShawarmaMPDescription;
        }

        private async Task GameTimer()
        {
            while (isPlay)
            {
                await Task.Delay(Constants.FPS);
                count_sprite_frame++;
                count_frame+= Constants.FPS;
                if (count_frame >= Constants.Second)
                {
                    count_frame = 0;
                    time = time.Add(TimeSpan.FromSeconds(1));
                    if (!isBoss)
                    {
                        count_spawn_newbie_delay++;
                        if (count_spawn_newbie_delay >= Constants.SpawnNewbieDelaySec)
                        {
                            count_spawn_newbie_delay = 0;
                            SpawnNewbie();
                        }
                        if (time.Minute != _lastMinute)
                        {
                            dayCountTB.Text = $"{count_day++} д.";
                            _lastMinute = time.Minute;
                            // сюда добавить условие
                        }
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
            NewbieGameUpdate();
            ChattersGameUpdate();

        }

        void PlayerGameUpdate() // все методы GameUpdate закинуть в класс GameUpdate и испольлзовать статичные методы оттуда
        {
            double factSpeed = player.Speed;
            if (player.IsRun && player.State == "walk")
            {
                if (player.Stamina <= 1) // немного больше единицы, чтобы высота показателя не была отрицательной
                    player.IsRun = false;
                else
                {
                    factSpeed *= player.RunModificator;
                    player.Stamina -= player.StaminaConsuptionWalk;
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

            hpTB.Text = player.HP.ToString();
            mpTB.Text = player.MP.ToString();
            staminaTB.Text = player.Stamina.ToString();

            switch (player.State)
            {
                case "walk":
                    PlayerMovement(factSpeed);
                    playerSprite.Margin = player.Coordinates;
                    HalalCartCollision();
                    break;
                case "roll":
                    PlayerMovement(player.RollSpeed);
                    playerSprite.Margin = player.Coordinates;
                    HalalCartCollision();
                    break;
            }
        }

        void PlayerMovement(double distance)
        {
            switch (player.Rotation)
            {
                case "left":
                    player.Coordinates = new(player.Coordinates.Left - distance, player.Coordinates.Top, 0, 0);
                    break;
                case "right":
                    player.Coordinates = new(player.Coordinates.Left + distance, player.Coordinates.Top, 0, 0);
                    break;
                case "up":
                    player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - distance, 0, 0);
                    break;
                case "down":
                    player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + distance, 0, 0);
                    break;
            }
            switch (player.SecondRotation)
            {
                case "left":
                    player.Coordinates = new(player.Coordinates.Left - distance, player.Coordinates.Top, 0, 0);
                    break;
                case "right":
                    player.Coordinates = new(player.Coordinates.Left + distance, player.Coordinates.Top, 0, 0);
                    break;
                case "up":
                    player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top - distance, 0, 0);
                    break;
                case "down":
                    player.Coordinates = new(player.Coordinates.Left, player.Coordinates.Top + distance, 0, 0);
                    break;
            }
            if (player.Coordinates.Left <= -1850)
                player.Coordinates = new(-1850, player.Coordinates.Top, 0, 0);
            else if (player.Coordinates.Left >= 1850)
                player.Coordinates = new(1850, player.Coordinates.Top, 0, 0);
            if (player.Coordinates.Top <= -910)
                player.Coordinates = new(player.Coordinates.Left, -910, 0, 0);
            else if (player.Coordinates.Top >= 910)
                player.Coordinates = new(player.Coordinates.Left, 910, 0, 0);
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

        async void WelcomeNewbie()
        {
            if (player.State == "roll" || player.State == "welcome" || player.State == "attack")
                return;
            for (int i = 0; i < newbies.Count; i++)
            {
                if ((Math.Abs(player.Coordinates.Left - newbies[i].Coordinates.Left) <= player.Size + newbies[i].Size) && (Math.Abs(player.Coordinates.Top - newbies[i].Coordinates.Top) <= player.Size + newbies[i].Size) && !newbies[i].IsWelcoming)
                {
                    newbies[i].OpacityProgress = 0;
                    newbieSprites[i].Opacity = 1;
                    newbies[i].IsWelcoming = true;
                }
            }
            stateBefore = player.State;
            player.ProgressSprite = 0;
            player.State = "welcome";
            await Task.Delay(Constants.PlayerWelcomeFrameCount);
            player.State = stateBefore;
            UpdateDirections();
        }

        void ChattersGameUpdate()
        {
            for (int i = 0; i < newbies.Count; i++)
            {

            }
        }

        void NewbieGameUpdate()
        {
            for (int i = 0; i < newbies.Count; i++)
            {
                if (!newbies[i].IsWelcoming)
                {
                    newbies[i].OpacityFrameCountProgress++;
                    if (newbies[i].OpacityFrameCountProgress >= Constants.NewbieOpacityFrameCount)
                    {
                        newbies[i].OpacityFrameCountProgress = 0;
                        newbies[i].OpacityProgress++;
                        newbieSprites[i].Opacity-=0.01;
                        if (newbies[i].OpacityProgress >= 100)
                        {
                            KillNewbie(i);
                            i--;
                            continue;
                        }
                    }
                }
                else
                {
                    newbies[i].ChattersingFrameCountProgress++;
                    if (newbies[i].ChattersingFrameCountProgress >= Constants.NewbieChattersingFrameCount)
                    {
                        newbies[i].ChattersingFrameCountProgress = 0;
                        newbies[i].ChattersingProgress++;
                        if (newbies[i].ChattersingProgress >= 100)
                        {
                            SpawnChatters(newbies[i]);
                            gameUnitMapGrid.Children.Remove(newbieSprites[i]);
                            newbieSprites.RemoveAt(i);
                            newbies.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
                NewbieMovement(i);
            }
        }

        void NewbieMovement(int index) // логика движения (лево-право-верх-низ без второго направления)
        {
            if (newbies[index].State == "walk")
            {
                switch (newbies[index].Rotation)
                {
                    case "left":
                        newbies[index].Coordinates = new(newbies[index].Coordinates.Left - newbies[index].Speed, newbies[index].Coordinates.Top, 0, 0);
                        break;
                    case "right":
                        newbies[index].Coordinates = new(newbies[index].Coordinates.Left + newbies[index].Speed, newbies[index].Coordinates.Top, 0, 0);
                        break;
                    case "up":
                        newbies[index].Coordinates = new(newbies[index].Coordinates.Left, newbies[index].Coordinates.Top - newbies[index].Speed, 0, 0);
                        break;
                    case "down":
                        newbies[index].Coordinates = new(newbies[index].Coordinates.Left, newbies[index].Coordinates.Top + newbies[index].Speed, 0, 0);
                        break;
                }
                bool changeRotation = false;
                List<string> openRotation = ["left", "right", "up", "down"];
                if (newbies[index].Coordinates.Left <= -1850)
                {
                    newbies[index].Coordinates = new(-1845, newbies[index].Coordinates.Top, 0, 0);
                    openRotation.Remove("left");
                    changeRotation = true;
                }
                else if (newbies[index].Coordinates.Left >= 1850)
                {
                    newbies[index].Coordinates = new(1845, newbies[index].Coordinates.Top, 0, 0);
                    openRotation.Remove("right");
                    changeRotation = true;
                }
                if (newbies[index].Coordinates.Top <= -910)
                {
                    newbies[index].Coordinates = new(newbies[index].Coordinates.Left, -905, 0, 0);
                    openRotation.Remove("up");
                    changeRotation = true;
                }
                else if (newbies[index].Coordinates.Top >= 910)
                {
                    newbies[index].Coordinates = new(newbies[index].Coordinates.Left, 905, 0, 0);
                    openRotation.Remove("down");
                    changeRotation = true;
                }
                if (changeRotation)
                {
                    Random rnd = new();
                    newbies[index].Rotation = openRotation[rnd.Next(0, openRotation.Count)];
                }
                newbieSprites[index].Margin = newbies[index].Coordinates;
            }
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
            for (int i = 0; i < newbieSprites.Count; i++)
            {
                if (newbies[i].ProgressSprite >= cashe._unit[(newbies[i].Name + newbies[i].VariationSprite, newbies[i].State, newbies[i].Rotation)].Count) 
                    newbies[i].ProgressSprite = 0;
                newbieSprites[i].Source = cashe._unit[(newbies[i].Name + newbies[i].VariationSprite, newbies[i].State, newbies[i].Rotation)][newbies[i].ProgressSprite];
                newbies[i].ProgressSprite++;
            }
            for (int i = 0; i < chatterSprites.Count; i++)
            {
                if (chatters[i].ProgressSprite >= cashe._unit[(chatters[i].Name + chatters[i].VariationSprite, chatters[i].State, chatters[i].Rotation)].Count)
                    chatters[i].ProgressSprite = 0;
                chatterSprites[i].Source = cashe._unit[(chatters[i].Name + chatters[i].VariationSprite, chatters[i].State, chatters[i].Rotation)][chatters[i].ProgressSprite];
                chatters[i].ProgressSprite++;
            }
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
                if (e.Key == Key.Space)  // перекат
                    if (player.State != "roll" && player.State != "attack" && player.State != "welcome")
                        PlayerRoll();
            }
        }

        async void PlayerRoll()
        {
            if (player.Stamina < player.StaminaConsuptionRoll) 
                return;
            player.Stamina -= player.StaminaConsuptionRoll;
            stateBefore = player.State;
            player.State = "roll";
            await Task.Delay(Constants.PlayerRollFrameCount);
            player.State = stateBefore;
            UpdateDirections();
        }

        private void UpdateDirections()
        {
            if (player.State == "roll" || player.State == "welcome" || player.State == "attack")
                return;
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
            player.AttackPiasProcent = Constants.PlayerAttackPiasProcent;
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

        void SpawnNewbie()
        {
            List<string> openRotation = ["left", "right", "up", "down"];
            Random rnd = new();
            int positionX = rnd.Next(-1700,1700);
            int positionY = rnd.Next(-850, 850);
            Newbie newbie = new()
            {
                Name = "newbie",
                Size = Constants.NewbieSize,
                Speed = Constants.NewbieSpeed,
                MaxHP = Constants.NewbieHP,
                HP = Constants.NewbieHP,
                State = "walk",
                Coordinates = new Thickness(positionX, positionY,0,0),
                Rotation = openRotation[rnd.Next(0, openRotation.Count)]
            };
            newbie.VariationSprite = rnd.Next(0, newbie.VariationSprite + 1);
            Image newbieSprite = new()
            {
                Width = newbie.Size,
                Height = newbie.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = cashe._unit[($"{newbie.Name}{newbie.VariationSprite}", newbie.State, newbie.Rotation)][0],
                Margin = newbie.Coordinates
            };
            newbies.Add(newbie);
            newbieSprites.Add(newbieSprite);
            gameUnitMapGrid.Children.Add(newbieSprite);
        }

        void SpawnChatters(Unit newbie)
        {
            Random rnd = new();
            Chatters chatter = new()
            {
                Name = "chatters",
                Size = Constants.ChattersSize,
                Speed = Constants.ChattersSpeed,
                MaxHP = Constants.ChattersHP,
                HP = Constants.ChattersHP,
                State = newbie.State,
                Coordinates = newbie.Coordinates,
                Rotation = newbie.Rotation
            };
            chatter.VariationSprite = rnd.Next(0, chatter.VariationSprite + 1);

            Image chatterSprite = new()
            {
                Width = chatter.Size,
                Height = chatter.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = cashe._unit[($"{chatter.Name}{chatter.VariationSprite}", chatter.State, chatter.Rotation)][0],
                Margin = chatter.Coordinates
            };
            chatters.Add(chatter);
            chatterSprites.Add(chatterSprite);
            gameUnitMapGrid.Children.Add(chatterSprite);
        }

        void BuildClick(string name)
        {
            gameBuildingMapGrid.Focus();
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
                if (name == "halalcart")
                {
                    player.MP += player.MaxMP * Constants.ProcentShawarmaRegenMP;
                    if (player.MP > player.MaxMP)
                        player.MP = player.MaxMP;
                }
                else
                {
                    player.HP += player.MaxHP * Constants.ProcentShawarmaRegenHP;
                    if (player.HP > player.MaxHP)
                        player.HP = player.MaxHP;
                }
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
            if (!isPlay)
                return;
            gameBuildingMapGrid.Focus();
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
            else
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    PlayerAttack();
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    WelcomeNewbie();
                }
            }
        }

        void PlayerAddEXP(int point)
        {
            player.EXP += point;
            expTB.Text = player.EXP.ToString();
            if (player.EXP >= player.EXPForLevelUP)
            {
                player.Level++;
                player.EXP = 0;
                player.EXPForLevelUP = Convert.ToInt32(player.EXPForLevelUP * player.EXPForLevelUPModificator);
                player.MaxHP += Constants.LevelGiveHP;
                player.MaxMP += Constants.LevelGiveMP;
                player.MaxStamina += Constants.LevelGiveStamina;
                player.Attack += Constants.LevelGiveAttack;
                SpawnSpeceffectOnUnit(player, cashe._item["levelup"], player.Size, player.Size, Constants.LevelUpFrameCount);
            }
        }

        async void SpawnSpeceffectOnDot(Thickness position, BitmapImage source, double sizeWeigth, double sizeHeight, int delay)
        {
            Image speceffect = new()
            {
                Margin = position,
                Width = sizeWeigth,
                Height = sizeHeight,
                Source = source
            };
            gameUnitMapGrid.Children.Add(speceffect);
            await Task.Delay(delay);
            gameUnitMapGrid.Children.Remove(speceffect);
        }

        async void SpawnSpeceffectOnUnit(Unit unit, BitmapImage source, double sizeWeigth, double sizeHeight, int delay)
        {
            Image speceffect = new()
            {
                Margin = unit.Coordinates,
                Width = sizeWeigth,
                Height = sizeHeight,
                Source = source
            };
            gameUnitMapGrid.Children.Add(speceffect);
            int count = 0;
            while (isPlay)
            {
                await Task.Delay(Constants.FPS);
                count++;
                if (count >= delay)
                {
                    gameUnitMapGrid.Children.Remove(speceffect);
                    return;
                }
                speceffect.Margin = unit.Coordinates;
            }
        }

        async void PlayerAttack()
        {
            if (player.State == "roll" || player.State == "welcome" || player.State == "attack")
                return;
            stateBefore = player.State;
            player.ProgressSprite = 0;
            player.State = "attack";
            Thickness attackCoordinates = player.Coordinates;
            double attackWeight = player.AttackWeight;
            double attackHeight = player.AttackHeight;
            switch (player.Rotation)
            {
                case "left":
                    attackWeight = player.AttackHeight;
                    attackHeight = player.AttackWeight;
                    attackCoordinates = new(player.Coordinates.Left - player.Size * player.AttackPiasProcent, player.Coordinates.Top, 0, 0);
                    break;
                case "right":
                    attackWeight = player.AttackHeight;
                    attackHeight = player.AttackWeight;
                    attackCoordinates = new(player.Coordinates.Left + player.Size * player.AttackPiasProcent, player.Coordinates.Top, 0, 0);
                    break;
                case "down":
                    attackCoordinates = new(player.Coordinates.Left, player.Coordinates.Top + player.Size * player.AttackPiasProcent, 0, 0);
                    break;
                case "up":
                    attackCoordinates = new(player.Coordinates.Left, player.Coordinates.Top - player.Size * player.AttackPiasProcent, 0, 0);
                    break;
            }
            await Task.Delay(Constants.PlayerAttackFrameCount);
            for (int i = 0; i < newbies.Count; i++) // пока атака пока действует только на новичков. для остальных отдельные циклы
            {
                if ((Math.Abs(attackCoordinates.Left - newbies[i].Coordinates.Left) <= attackWeight + newbies[i].Size) && (Math.Abs(attackCoordinates.Top - newbies[i].Coordinates.Top) <= attackHeight + newbies[i].Size) && !newbies[i].IsWelcoming)
                {
                    newbies[i].HP -= player.Attack;
                    if (newbies[i].HP <= 0)
                    {
                        count_meat+=Constants.NewbieDropMeat;
                        PlayerAddEXP(Constants.NewbieDropEXP);
                        meatCountTB.Text = $"{count_meat} 🍕";
                        KillNewbie(i);
                    }
                }
            }
            for (int i = 0; i < chatters.Count; i++) // пока атака пока действует только на новичков. для остальных отдельные циклы
            {
                if ((Math.Abs(attackCoordinates.Left - chatters[i].Coordinates.Left) <= attackWeight + chatters[i].Size) && (Math.Abs(attackCoordinates.Top - chatters[i].Coordinates.Top) <= attackHeight + chatters[i].Size))
                {
                    chatters[i].HP -= player.Attack;
                    if (chatters[i].HP <= 0)
                    {
                        count_meat += Constants.ChattersDropMeat;
                        PlayerAddEXP(Constants.ChattersDropEXP);
                        meatCountTB.Text = $"{count_meat} 🍕";
                        KillChatters(i);
                    }
                }
            }
            player.State = stateBefore;
            UpdateDirections();
            SpawnSpeceffectOnDot(attackCoordinates, cashe._item[$"attack_{player.Rotation}"], attackWeight, attackHeight, player.AttackSpriteDelay);
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

        void KillNewbie(int index)
        {
            gameUnitMapGrid.Children.Remove(newbieSprites[index]);
            newbieSprites.RemoveAt(index);
            newbies.RemoveAt(index);
        }

        void KillChatters(int index)
        {
            gameUnitMapGrid.Children.Remove(chatterSprites[index]);
            chatterSprites.RemoveAt(index);
            chatters.RemoveAt(index);
        }

        void InventoryRightMouseDown(MouseButtonEventArgs e, int num)
        {
            e.Handled = true;
        }

        void InventoryClick(int num)
        {
            gameBuildingMapGrid.Focus();

        }

        void HandRightMouseDown(MouseButtonEventArgs e, int num)
        {
            e.Handled = true;
        }

        void HandClick(int num)
        {
            gameBuildingMapGrid.Focus();

        }

        private void Button_Building1(object sender, RoutedEventArgs e)
        {
            BuildClick("wall");
        }

        private void Button_Building2(object sender, RoutedEventArgs e)
        {
            BuildClick("halalcart");
        }

        private void hand1Button_Click(object sender, RoutedEventArgs e)
        {
            HandClick(0);
        }

        private void hand1Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandRightMouseDown(e, 0);
        }

        private void hand2Button_Click(object sender, RoutedEventArgs e)
        {
            HandClick(0);
        }

        private void hand2Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandRightMouseDown(e, 1);
        }

        private void inventory1Button_Click(object sender, RoutedEventArgs e)
        {
            InventoryClick(0);
        }

        private void inventory2Button_Click(object sender, RoutedEventArgs e)
        {
            InventoryClick(1);
        }

        private void inventory3Button_Click(object sender, RoutedEventArgs e)
        {
            InventoryClick(2);
        }

        private void inventory4Button_Click(object sender, RoutedEventArgs e)
        {
            InventoryClick(3);
        }

        private void inventory1Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            InventoryRightMouseDown(e, 0);
        }

        private void inventory2Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            InventoryRightMouseDown(e, 1);
        }

        private void inventory3Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            InventoryRightMouseDown(e, 2);
        }

        private void inventory4Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            InventoryRightMouseDown(e, 3);
        }
    }
}