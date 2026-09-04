using MaksLifeInChat.Model;
using System.IO;
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
using System.Xml.Linq;

namespace MaksLifeInChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<int> chattersFindCounter = new List<int>();
        int id_unit_count = 0;
        bool isPlay = false;
        bool isBoss = false;
        int count_frame = 0; // frame count (не отражает время)
        int count_sprite_frame = 0;
        int count_spawn_newbie_delay = 0;
        int count_spawn_boss_delay = 0;
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
        public MediaPlayer _mediaPlayer = new MediaPlayer();
        List<string> playList = [];
        int current_track = 0;
        List<string> inventoryItems = [];
        List<string> handsItems = [];
        double beforeItem6MaxMP = 0;
        double beforeItem6RegenMP = 0;
        double beforeItemMaxStamina = 0;
        double beforeItemRegenStamina = 0;
        double beforeItem9MaxHP = 0;
        double beforeItem9RegenHP = 0;
        double beforeItem9MaxMP = 0;
        double beforeItem9RegenMP = 0;
        int kill_bosses_count = 0;
        Moderator currentModer;
        Image moderSprite;
        public MainWindow()
        {
            App.mainWindow = this;
            InitializeComponent();
            Zastavka();
            pauseTB.Text = "ПАУЗА\n(ПЕРЕРЫВ НА КОШКУ)\n(ПЕРЕРЫВ НА КОШКУ)";
            MenuFrame menuFrame = new();
            menuGrid.Children.Add(menuFrame);
            cashe = new SpriteCache();
            cashe.GetUnitSpritesList(Constants.unitNames, Constants.states, Constants.rotations);
            cashe.GetItemSpritesList(Constants.itemNames);
            cashe.GetItemSpritesList(Constants.interfaceNames);
            string[] items = new string[Constants.ItemCount];
            for (int i = 0; i < Constants.ItemCount; i++)
                items[i] = $"item{i}";
            cashe.GetItemSpritesList(items);
        }

        async void Zastavka()
        {
            await Task.Delay(Constants.Second * Constants.FPS / 2);
            while (zastavkaImage.Opacity >= 0)
            {
                await Task.Delay(Constants.FPS);
                zastavkaImage.Opacity -= 0.01;
            }
            zastavkaImage.Visibility = Visibility.Collapsed;
        }

        async void PlayMusic()
        {
            _mediaPlayer.Volume = App.volumeSettings;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            int count = 0;
            while (true)
            {
                string fileName = $"{Constants.ResourceFolder}music{count}.mp3";
                if (!File.Exists(fileName)) break;
                playList.Add(fileName);
                count++;
            }
            _mediaPlayer.Open(new Uri(playList[current_track], UriKind.Relative));
            _mediaPlayer.Play();
        }

        public async void StartGame()
        {
            menuGrid.Children.Clear();
            gamePlayerMapGrid.Children.Clear();
            gameBuildingMapGrid.Children.Clear();
            gameUnitMapGrid.Children.Clear();
            PlayMusic();
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
                count_frame += Constants.FPS;
                if (count_frame >= Constants.Second)
                {
                    count_frame = 0;
                    time = time.Add(TimeSpan.FromSeconds(1));
                    if (!isBoss)
                    {
                        count_spawn_newbie_delay++;
                        count_spawn_boss_delay++;
                        if (count_spawn_newbie_delay >= Constants.SpawnNewbieDelaySec)
                        {
                            count_spawn_newbie_delay = 0;
                            SpawnNewbie();
                        }
                        if (count_spawn_boss_delay >= Constants.SpawnBossDelaySec)
                        {
                            count_spawn_boss_delay = 0;
                            SpawnModer();
                        }
                    }
                    if (time.Minute != _lastMinute)
                    {
                        dayCountTB.Text = $"{count_day++} д.";
                        _lastMinute = time.Minute;
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
            ModerGameUpdate();

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
            if (player.Name == "kara")
            {
                if (player.MP <= 1)
                    player.Name = "kay";
                else
                {
                    factSpeed *= player.KaraSpeedModificator;
                    player.MP -= player.KaraConsuption;
                }
            }
            else
                if (player.MP < player.MaxMP)
                    player.MP += player.RegenMP;

            if (player.MP <= 0)// чтобы не вызывало ошибку с высотой прямоугольников с отрицательным или нулевым значением
                player.MP = 1;
            if (player.Stamina <= 0)
                player.Stamina = 1;
            if (player.HP < player.MaxHP)
                player.HP += player.RegenHP;
            if (player.HP <= 0)
            {
                SpawnSpeceffectOnDot(player.Coordinates, cashe._item["bossdeath"], Constants.SizeSpriteDeathPlayer, Constants.SizeSpriteDeathPlayer, Constants.BossDeathFrameCount*2);
                playerSprite.Source = null;
                GameOver();
                return;
            }

            mpRectangle.Height = player.MP / player.MaxMP * 120;
            hpRectangle.Height = player.HP / player.MaxHP * 120;
            staminaRectangle.Height = player.Stamina / player.MaxStamina * 120;

            hpTB.Text = $"{player.HP} человечность";
            mpTB.Text = $"{player.MP} жестокость";
            staminaTB.Text = $"{player.Stamina} выносливость";

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
            if (buildings.Count != 0)
            {
                bool isExist = false;
                List<Building> halals = buildings.FindAll(x => x.Name == "halalcart");
                for (int i = 0; i < halals.Count; i++)
                {
                    if ((Math.Abs(player.Coordinates.Left - halals[i].Coordinates.Left) <= player.Size + halals[i].Size) && (Math.Abs(player.Coordinates.Top - halals[i].Coordinates.Top) <= player.Size + halals[i].Size)) {
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
            if (isBoss) return;
            for (int i = 0; i < chatters.Count; i++)
            {
                if (chatters[i].AttackPause)
                    continue;
                chattersFindCounter[i]++;
                if (chattersFindCounter[i] >= Constants.FindEnemyFrameCount)
                {
                    chattersFindCounter[i] = 0;
                    Unit? target = FindNearestTarget(chatters[i]);
                    if (target != null)
                    {
                        SetDirectionToTarget(chatters[i], target);
                        if (IsInAttackRange(chatters[i], target))
                            ChattersAttack(chatters[i], target);
                        else
                            chatters[i].State = "walk";
                    }
                    else
                    {
                        chatters[i].State = "stand";
                    }
                }

                ChattersMovement(i);
            }
        }

        private void SetDirectionToTarget(Unit self, Unit target)
        {
            double dx = target.Coordinates.Left - self.Coordinates.Left;
            double dy = target.Coordinates.Top - self.Coordinates.Top;

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                self.Rotation = dx >= 0 ? "right" : "left";
                self.SecondRotation = Math.Abs(dy) > self.Size * 2 ? (dy > 0 ? "down" : "up") : null;
            }
            else
            {
                self.Rotation = dy >= 0 ? "down" : "up";
                self.SecondRotation = Math.Abs(dy) > self.Size * 2 ? (dx > 0 ? "right" : "left") : null;
            }
        }
        private bool IsInAttackRange(Unit self, Unit target)
        {
            double range = self.Size + self.Size * self.AttackPiasProcent + self.AttackHeight;
            double dx = Math.Abs(self.Coordinates.Left - target.Coordinates.Left);
            double dy = Math.Abs(self.Coordinates.Top - target.Coordinates.Top);
            return dx < range && dy < range;
        }

        private async void ChattersAttack(Unit self, Unit target)
        {
            if (self.State == "attack" || self.AttackPause) return;

            string stateBefore = self.State;
            self.ProgressSprite = 0;
            self.State = "attack";

            Thickness attackCoordinates = self.Coordinates;
            double attackWeight = self.AttackWeight;
            double attackHeight = self.AttackHeight;
            switch (self.Rotation)
            {
                case "left":
                    attackWeight = self.AttackHeight;
                    attackHeight = self.AttackWeight;
                    attackCoordinates = new(self.Coordinates.Left - self.Size * self.AttackPiasProcent, self.Coordinates.Top, 0, 0);
                    break;
                case "right":
                    attackWeight = self.AttackHeight;
                    attackHeight = self.AttackWeight;
                    attackCoordinates = new(self.Coordinates.Left + self.Size * self.AttackPiasProcent, self.Coordinates.Top, 0, 0);
                    break;
                case "down":
                    attackCoordinates = new(self.Coordinates.Left, self.Coordinates.Top + self.Size * self.AttackPiasProcent, 0, 0);
                    break;
                case "up":
                    attackCoordinates = new(self.Coordinates.Left, self.Coordinates.Top - self.Size * self.AttackPiasProcent, 0, 0);
                    break;
            }

            await Task.Delay(Constants.ChattersAttackFrameCount);
            if (!chatters.Exists(x => x == self))
                return;
            if ((Math.Abs(attackCoordinates.Left - target.Coordinates.Left) <= attackWeight + target.Size) &&
                (Math.Abs(attackCoordinates.Top - target.Coordinates.Top) <= attackHeight + target.Size)) // здесь перебирать всех в области и им уменьшать, и таргет[i] пусть будет
            {
                target.HP -= self.Attack;
                if (target is Player)
                {
                    PlayerDamage(self.Attack);  // используем существующий метод
                }
                else if (target is Building)
                {
                    if (target.HP <= 0)
                        KillBuilding(buildings.IndexOf((Building)target));
                }
            }
            SpawnSpeceffectOnDot(attackCoordinates, cashe._item[$"chatters_attack_{self.Rotation}"], attackWeight, attackHeight, self.AttackSpriteDelay);
            self.State = "stand";
            self.AttackPause = true;
            await Task.Delay(Constants.ChattersAttackPauseDelay);
            self.AttackPause = false;
        }

        private async void KillBuilding(int index)
        {
            if (index == -1)
                return; // я хз как оно появляется
            SpawnSpeceffectOnDot(buildings[index].Coordinates, cashe._item["death"], buildings[index].Size, buildings[index].Size, Constants.DeathFrameCount);
            gameBuildingMapGrid.Children.Remove(buildingSprites[index]);
            buildingSprites.RemoveAt(index);
            buildings.RemoveAt(index);
            if (buildings.Count == 0)
            {
                halalcartZone = false;
                SetBuildingSprites();
            }
            HalalCartCollision();
        }

        void ChattersMovement(int index)
        {
            var chatter = chatters[index];
            if (chatter.State != "walk") return;

            double distance = chatter.Speed;

            switch (chatter.Rotation)
            {
                case "left": chatter.Coordinates = new(chatter.Coordinates.Left - distance, chatter.Coordinates.Top, 0, 0); break;
                case "right": chatter.Coordinates = new(chatter.Coordinates.Left + distance, chatter.Coordinates.Top, 0, 0); break;
                case "up": chatter.Coordinates = new(chatter.Coordinates.Left, chatter.Coordinates.Top - distance, 0, 0); break;
                case "down": chatter.Coordinates = new(chatter.Coordinates.Left, chatter.Coordinates.Top + distance, 0, 0); break;
            }

            if (chatter.SecondRotation != null)
            {
                switch (chatter.SecondRotation)
                {
                    case "left": chatter.Coordinates = new(chatter.Coordinates.Left - distance, chatter.Coordinates.Top, 0, 0); break;
                    case "right": chatter.Coordinates = new(chatter.Coordinates.Left + distance, chatter.Coordinates.Top, 0, 0); break;
                    case "up": chatter.Coordinates = new(chatter.Coordinates.Left, chatter.Coordinates.Top - distance, 0, 0); break;
                    case "down": chatter.Coordinates = new(chatter.Coordinates.Left, chatter.Coordinates.Top + distance, 0, 0); break;
                }
            }

            chatterSprites[index].Margin = chatter.Coordinates;
        }

        private Unit? FindNearestTarget(Unit self)
        {
            if (player.Name == "kara")
                return player;
            var targets = new List<Unit>();

            targets.Add(player);

            foreach (var b in buildings)
                targets.Add(b);

            return GetClosestTarget(self, targets);
        }

        private Unit? GetClosestTarget(Unit self, List<Unit> candidates)
        {
            if (candidates.Count == 0) return null;
            Unit closest = candidates[0];
            double minDist = GetDistance(self, closest);
            for (int i = 1; i < candidates.Count; i++)
            {
                double dist = GetDistance(self, candidates[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = candidates[i];
                }
            }
            return closest;
        }

        private double GetDistance(Unit a, Unit b)
        {
            double dx = a.Coordinates.Left - b.Coordinates.Left;
            double dy = a.Coordinates.Top - b.Coordinates.Top;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        void NewbieGameUpdate()
        {
            if (isBoss) return;
            for (int i = 0; i < newbies.Count; i++)
            {
                if (!newbies[i].IsWelcoming)
                {
                    newbies[i].OpacityFrameCountProgress++;
                    if (newbies[i].OpacityFrameCountProgress >= Constants.NewbieOpacityFrameCount)
                    {
                        newbies[i].OpacityFrameCountProgress = 0;
                        newbies[i].OpacityProgress++;
                        newbieSprites[i].Opacity -= 0.01;
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

        async void ModerGameUpdate()
        {
            if (currentModer != null)
            {
                ModerMovement();
                if (currentModer.Name == "lancev0" || currentModer.Name == "lancev1")
                {
                        currentModer.MP += currentModer.RegenMP;
                        if (currentModer.MP < 10)
                            currentModer.IsBanish = true;
                        else if (currentModer.MP >= 50)
                            currentModer.IsBanish = false;
                        if (currentModer.State == "roll")
                            return;
                        SetDirectionToTarget(currentModer, player);
                        if (currentModer.AttackPause || currentModer.State == "attack")
                            return;
                        if (!currentModer.IsBanish)
                        {
                            double range = currentModer.Size + currentModer.Size * currentModer.AttackPiasProcent + currentModer.AttackHeight / 4;
                            double dx = Math.Abs(currentModer.Coordinates.Left - player.Coordinates.Left);
                            double dy = Math.Abs(currentModer.Coordinates.Top - player.Coordinates.Top);
                            bool isInAttackRange = dx < range && dy < range;
                            if (isInAttackRange && currentModer.MP >= currentModer.MPConsuptionAttackBase)
                            {
                                currentModer.MP -= currentModer.MPConsuptionAttackBase;
                                string stateBefore = currentModer.State;
                                currentModer.ProgressSprite = 0;
                                currentModer.State = "attack";
                                Thickness attackCoordinates = currentModer.Coordinates;
                                double attackWeight = currentModer.AttackWeight;
                                double attackHeight = currentModer.AttackHeight;
                                switch (currentModer.Rotation)
                                {
                                    case "left":
                                        attackWeight = currentModer.AttackHeight;
                                        attackHeight = currentModer.AttackWeight;
                                        attackCoordinates = new(currentModer.Coordinates.Left - currentModer.Size * currentModer.AttackPiasProcent, currentModer.Coordinates.Top, 0, 0);
                                        break;
                                    case "right":
                                        attackWeight = currentModer.AttackHeight;
                                        attackHeight = currentModer.AttackWeight;
                                        attackCoordinates = new(currentModer.Coordinates.Left + currentModer.Size * currentModer.AttackPiasProcent, currentModer.Coordinates.Top, 0, 0);
                                        break;
                                    case "down":
                                        attackCoordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top + currentModer.Size * currentModer.AttackPiasProcent, 0, 0);
                                        break;
                                    case "up":
                                        attackCoordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top - currentModer.Size * currentModer.AttackPiasProcent, 0, 0);
                                        break;
                                }
                                await Task.Delay(Constants.ChattersAttackFrameCount);
                                if ((Math.Abs(attackCoordinates.Left - player.Coordinates.Left) <= attackWeight + player.Size) &&
                                    (Math.Abs(attackCoordinates.Top - player.Coordinates.Top) <= attackHeight + player.Size))
                                {
                                    PlayerDamage(currentModer.Attack);
                                }
                                SpawnSpeceffectOnDot(attackCoordinates, cashe._item[$"lancev_attack_{currentModer.Rotation}"], attackWeight, attackHeight, currentModer.AttackSpriteDelay);
                                currentModer.State = "walk";
                                currentModer.AttackPause = true;
                                await Task.Delay(Constants.ChattersAttackPauseDelay * 3);
                                currentModer.AttackPause = false;
                            }
                            else
                                currentModer.State = "walk";
                        }
                        else
                        {
                            double dist = GetDistance(currentModer, player);

                            if (dist > Constants.DuelDistant + currentModer.Speed)
                            {
                                currentModer.State = "walk";
                                currentModer.ProgressSprite = 0;
                                SetDirectionToTarget(currentModer, player);
                            }
                            else if (dist < Constants.DuelDistant - currentModer.Speed)
                            {
                                currentModer.State = "walk";
                                currentModer.ProgressSprite = 0;

                                double dx = currentModer.Coordinates.Left - player.Coordinates.Left;
                                double dy = currentModer.Coordinates.Top - player.Coordinates.Top;

                                if (Math.Abs(dx) >= Math.Abs(dy))
                                {
                                    currentModer.Rotation = dx >= 0 ? "right" : "left";
                                    currentModer.SecondRotation = Math.Abs(dy) > currentModer.Size * 2
                                        ? (dy > 0 ? "down" : "up")
                                        : null;
                                }
                                else
                                {
                                    currentModer.Rotation = dy >= 0 ? "down" : "up";
                                    currentModer.SecondRotation = Math.Abs(dx) > currentModer.Size * 2
                                        ? (dx > 0 ? "right" : "left")
                                        : null;
                                }
                            }
                            else
                            {
                                currentModer.State = "stand";
                                currentModer.SecondRotation = null;
                            }
                        }
                }
            }
        }

        async void LancevRoll()
        {
            if (currentModer.Coordinates.Left < 0)
                currentModer.Rotation = "right";
            else
                currentModer.Rotation = "left";
            if (currentModer.Coordinates.Top < 0)
                currentModer.SecondRotation = "down";
            else
                currentModer.SecondRotation = "up";

            currentModer.MP -= currentModer.MPConsuptionRoll;
            currentModer.State = "roll";
            await Task.Delay(Constants.PlayerRollFrameCount);
            currentModer.State = "stand";
        }

        void ModerMovement() // мне очень больно за этот метод, я бы чаттерский обновил, но у меня время капец поджимает, каждая минута на счету (хотя я мог потратить её на оптимизацию, а не на этот текст хд)
        {
            if (currentModer.State != "walk" && currentModer.State != "roll") return;

            double distance = currentModer.Speed;
            if (currentModer.State == "roll")
                distance *= 2;

            switch (currentModer.Rotation)
            {
                case "left": currentModer.Coordinates = new(currentModer.Coordinates.Left - distance, currentModer.Coordinates.Top, 0, 0); break;
                case "right": currentModer.Coordinates = new(currentModer.Coordinates.Left + distance, currentModer.Coordinates.Top, 0, 0); break;
                case "up": currentModer.Coordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top - distance, 0, 0); break;
                case "down": currentModer.Coordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top + distance, 0, 0); break;
            }

            if (currentModer.SecondRotation != null)
            {
                switch (currentModer.SecondRotation)
                {
                    case "left": currentModer.Coordinates = new(currentModer.Coordinates.Left - distance, currentModer.Coordinates.Top, 0, 0); break;
                    case "right": currentModer.Coordinates = new(currentModer.Coordinates.Left + distance, currentModer.Coordinates.Top, 0, 0); break;
                    case "up": currentModer.Coordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top - distance, 0, 0); break;
                    case "down": currentModer.Coordinates = new(currentModer.Coordinates.Left, currentModer.Coordinates.Top + distance, 0, 0); break;
                }
            }

            if (currentModer.Coordinates.Left <= -1850)
                currentModer.Coordinates = new(-1850, currentModer.Coordinates.Top, 0, 0);
            else if (currentModer.Coordinates.Left >= 1850)
                currentModer.Coordinates = new(1850, currentModer.Coordinates.Top, 0, 0);
            if (currentModer.Coordinates.Top <= -910)
                currentModer.Coordinates = new(currentModer.Coordinates.Left, -910, 0, 0);
            else if (currentModer.Coordinates.Top >= 910)
                currentModer.Coordinates = new(currentModer.Coordinates.Left, 910, 0, 0);

            moderSprite.Margin = currentModer.Coordinates;
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
            if (currentModer != null)
            {
                if (currentModer.ProgressSprite >= cashe._unit[(currentModer.Name, currentModer.State, currentModer.Rotation)].Count)
                    currentModer.ProgressSprite = 0;
                moderSprite.Source = cashe._unit[(currentModer.Name, currentModer.State, currentModer.Rotation)][currentModer.ProgressSprite];
                currentModer.ProgressSprite++;
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
                    if (player.Name == "kara")
                        player.Name = "kay";
                    else
                        player.Name = "kara";
                }
                if (e.Key == Key.Space)  // перекат
                    if (player.State == "stand" || player.State == "walk")
                        PlayerRoll();
                if (e.Key == Key.Escape)
                    PauseGame();
                if (e.Key == Key.Tab)
                    developInfo.Visibility = (developInfo.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (e.Key == Key.Escape)
                PlayGame();
        }

        void PauseGame()
        {
            isPlay = false;
            pauseTB.Visibility = Visibility.Visible;
        }

        async void PlayGame()
        {
            if (player != null)
            {
                isPlay = true;
                pauseTB.Visibility = Visibility.Collapsed; 
                await GameTimer();
            }
        }

        void ClearGame()
        {
            gameBuildingMapGrid.Children.Clear();
            gameUnitMapGrid.Children.Clear();
            gamePlayerMapGrid.Children.Clear();
            chattersFindCounter = new List<int>();
            id_unit_count = 0;
            isPlay = false;
            isBoss = false;
            count_frame = 0;
            count_sprite_frame = 0;
            count_spawn_newbie_delay = 0;
            time = new TimeOnly(0, 0);
            _lastMinute = -1;
            count_day = 0;
            count_cable = 0;
            count_meat = 0;
            player = null;
            playerSprite = null;
            buildings = [];
            buildingSprites = [];
            newbies = [];
            newbieSprites = [];
            chatters = [];
            chatterSprites = [];
            selectBuilding = null;
            halalcartZone = false;
            stateBefore = null;
            playList = [];
            current_track = 0;
            inventoryItems = [];
            handsItems = [];
            beforeItem6MaxMP = 0;
            beforeItem6RegenMP = 0;
            beforeItemMaxStamina = 0;
            beforeItemRegenStamina = 0;
            beforeItem9MaxHP = 0;
            beforeItem9RegenHP = 0;
            beforeItem9MaxMP = 0;
            beforeItem9RegenMP = 0;
            Constants.NewbieDropEXP=2;
            Constants.ChattersDropEXP = 3;
            Constants.NewbieDropMeat = 1;
            Constants.ChattersDropMeat = 2;
            currentModer = null;
            moderSprite = null;
        }

        void GameOver()
        {
            isPlay = false;
            SpawnMenu();
        }

        async void SpawnMenu()
        {
            await Task.Delay(Constants.BossDeathFrameCount * Constants.FPS);
            if (menuGrid.Children.Count == 0)
            {
                toolBarStackPanel.Visibility = Visibility.Collapsed;
                MenuFrame menuFrame = new MenuFrame();
                menuGrid.Children.Add(menuFrame);
                ClearGame();
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
            player = new() {
                AttackPiasProcent = Constants.PlayerAttackPiasProcent,
                ID = id_unit_count++
            };
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
            int positionX = rnd.Next(-1700, 1700);
            int positionY = rnd.Next(-850, 850);
            Newbie newbie = new()
            {
                ID = id_unit_count++,
                Name = "newbie",
                Size = Constants.NewbieSize,
                Speed = Constants.NewbieSpeed,
                MaxHP = Constants.NewbieHP,
                HP = Constants.NewbieHP,
                State = "walk",
                Coordinates = new Thickness(positionX, positionY, 0, 0),
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
                ID = id_unit_count++,
                Name = "chatters",
                Size = Constants.ChattersSize,
                Speed = Constants.ChattersSpeed,
                MaxHP = Constants.ChattersHP + Constants.ChattersHP * Constants.ChatterGainDayModificator * count_day,
                HP = Constants.ChattersHP + Constants.ChattersHP * Constants.ChatterGainDayModificator * count_day,
                Attack = Constants.ChattersAttack + Constants.ChattersAttack * Constants.ChatterGainDayModificator * count_day,
                State = newbie.State,
                Coordinates = newbie.Coordinates,
                Rotation = newbie.Rotation
            };
            chatter.VariationSprite = rnd.Next(0, chatter.VariationSprite + 1);

            switch (chatter.VariationSprite)
            {
                case 1:
                    chatter.Speed *= 0.8;
                    chatter.AttackHeight *= 1.3;
                    chatter.AttackWeight *= 1.3;
                    break;
                case 2:
                    chatter.AttackHeight *= 1.6;
                    chatter.AttackWeight /= 1.6;
                    break;
                case 4:
                    chatter.Size /= 2;
                    chatter.Speed *= 1.3;
                    break;
            }

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
            chattersFindCounter.Add(0);
        }

        async void SpawnModer()
        {
            isBoss = true;
            switch (kill_bosses_count) // шира -> аргунта -> бонус -> ланцев -> шерше -> дабль -> ян (но пока только ланц)
            {
                case 0:
                    Thickness lancevCoords = new Thickness(0, -600, 0, 0);
                    int lancevSize = 150;
                    SpawnSpeceffectOnDot(lancevCoords, cashe._item["bossdeath"], lancevSize, lancevSize, Constants.DeathFrameCount);
                    await Task.Delay(Constants.DeathFrameCount);
                    SpawnSpeceffectOnDot(lancevCoords, cashe._unit[("lancev0", "splash", "down")][0], lancevSize, lancevSize, Constants.BossDeathFrameCount * 2);
                    await Task.Delay(Constants.BossDeathFrameCount * 2);
                    SpawnSpeceffectOnDot(lancevCoords, cashe._unit[("lancev0", "splash", "down")][1], lancevSize, lancevSize, Constants.BossDeathFrameCount * 4);
                    for (int i = chatters.Count-1; i >=0 ; i--)
                        KillChatters(i);
                    for (int i = newbies.Count - 1; i >= 0; i--)
                        KillNewbie(i);
                    await Task.Delay(Constants.BossDeathFrameCount * 4);
                    currentModer = new()
                    {
                        ID = id_unit_count++,
                        Name = "lancev0",
                        RegenMP = 0.06, // 0.07
                        MaxHP = 150,
                        HP = 150,
                        AttackHeight = 440,
                        AttackWeight = 440,
                        Attack = 25, // 35
                        Speed = 6, // 7
                        Size = lancevSize, //170
                        Coordinates = lancevCoords
                    };
                    break;
                default:
                    return;
            }
            moderSprite = new()
            {
                Width = currentModer.Size,
                Height = currentModer.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = cashe._unit[(currentModer.Name, currentModer.State, currentModer.Rotation)][0],
                Margin = currentModer.Coordinates
            };
            gameUnitMapGrid.Children.Add(moderSprite);
        }
        void PlayerDamage(double damage)
        {
            if (player.State == "roll")
                return;
            SpawnSpeceffectOnUnit(player, cashe._item["damage"], player.Size, player.Size, Constants.DamageFrameCount);
            player.HP -= damage;
            if (player.HP < 0)
            {
                SpawnSpeceffectOnDot(player.Coordinates, cashe._item["bossdeath"], Constants.SizeSpriteDeathPlayer, Constants.SizeSpriteDeathPlayer, Constants.BossDeathFrameCount);
                playerSprite.Source = null;
                GameOver();
                return;
            }
            hpRectangle.Height = player.HP / player.MaxHP * 120;
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
                    int size;
                    if (name == "halalcart")
                    {
                        if (count_meat < Constants.HalalcartCostMeat || count_cable < Constants.HalalcartCostCable)
                            return;
                        size = Constants.SizeHalalcart;
                    }
                    else
                    {
                        if (count_meat < Constants.WallCostMeat || count_cable < Constants.WallCostCable)
                            return;
                        size = Constants.SizeWall;
                    }
                    selectBuilding = name;
                    mouseDynamicImage.Visibility = Visibility.Visible;
                    mouseDynamicImage.Width = size;
                    mouseDynamicImage.Height = size;
                    mouseDynamicImage.Source = cashe._unit[(name, "stand", "down")][0];
                }
            }
            else
            {
                if (name == "halalcart")
                {
                    if (count_meat < Constants.ShawarmaMPCostMeat || count_cable < Constants.ShawarmaMPCostCable)
                        return;
                    player.MP += player.MaxMP * Constants.ProcentShawarmaRegenMP;
                    if (player.MP > player.MaxMP)
                        player.MP = player.MaxMP;
                    AddMeatCable(meat:-Constants.ShawarmaMPCostMeat, cable: -Constants.ShawarmaMPCostCable);
                    SpawnSpeceffectOnUnit(player, cashe._item["mana_recovery"], player.Size, player.Size, Constants.LevelUpFrameCount);
                }
                else
                {
                    if (count_meat < Constants.ShawarmaHPCostMeat || count_cable < Constants.ShawarmaHPCostCable)
                        return;
                    player.HP += player.MaxHP * Constants.ProcentShawarmaRegenHP;
                    if (player.HP > player.MaxHP)
                        player.HP = player.MaxHP;
                    AddMeatCable(meat: -Constants.ShawarmaHPCostMeat, cable: -Constants.ShawarmaHPCostCable);
                    SpawnSpeceffectOnUnit(player, cashe._item["heal_recovery"], player.Size, player.Size, Constants.LevelUpFrameCount);
                }
            }
        }

        void AddMeatCable(int meat = 0, int cable = 0)
        {
            count_meat += meat;
            meatCountTB.Text = $"{count_meat} 🍕";
            count_cable += cable;
            cableCountTB.Text = $"{count_cable} 🔌";
            PizzaPower(meat); 
            CablePower(cable);
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
            if (player.EXP >= player.EXPForLevelUP)
            {
                player.Level++;
                player.EXP = 0;
                player.EXPForLevelUP = Convert.ToInt32(player.EXPForLevelUP * player.EXPForLevelUPModificator);
                player.MaxHP += Constants.LevelGiveHP;
                player.HP += Constants.LevelGiveHP;
                player.RegenHP += Constants.LevelGiveRegenHP;
                player.MaxMP += Constants.LevelGiveMP;
                player.RegenMP += Constants.LevelGiveRegenMP;
                player.MP += Constants.LevelGiveMP;
                player.MaxStamina += Constants.LevelGiveStamina;
                player.RegenStamina += Constants.LevelGiveRegenStamina;
                player.Stamina += Constants.LevelGiveStamina;
                player.Attack += Constants.LevelGiveAttack;
                SpawnSpeceffectOnUnit(player, cashe._item["levelup"], player.Size, player.Size, Constants.LevelUpFrameCount);
            }
            expTB.Text = $"{player.EXP} очки пыток";
            attackTB.Text = $"{player.Attack} радость";
            levelCountTB.Text = $"{player.Level} ур.";
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
            while (true)
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
            if (player.State == "roll" || player.State == "welcome" || player.State == "attack" || player.Stamina < player.StaminaConsuptionAttack)
                return;
            player.Stamina -= player.StaminaConsuptionAttack;
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
            for (int i = 0; i < newbies.Count; i++) 
            {
                if ((Math.Abs(attackCoordinates.Left - newbies[i].Coordinates.Left) <= attackWeight + newbies[i].Size) && (Math.Abs(attackCoordinates.Top - newbies[i].Coordinates.Top) <= attackHeight + newbies[i].Size) && !newbies[i].IsWelcoming)
                {
                    SpawnSpeceffectOnUnit(newbies[i], cashe._item["damage"], newbies[i].Size, newbies[i].Size, Constants.DamageFrameCount);
                    newbies[i].HP -= GivePlayerAttack();
                    if (newbies[i].HP <= 0)
                    {
                        AddMeatCable(meat:Constants.NewbieDropMeat);
                        PlayerAddEXP(Constants.NewbieDropEXP);
                        KillNewbie(i);
                    }
                }
            }
            for (int i = 0; i < chatters.Count; i++)
            {
                if ((Math.Abs(attackCoordinates.Left - chatters[i].Coordinates.Left) <= attackWeight + chatters[i].Size) && (Math.Abs(attackCoordinates.Top - chatters[i].Coordinates.Top) <= attackHeight + chatters[i].Size))
                {
                    SpawnSpeceffectOnUnit(chatters[i], cashe._item["damage"], chatters[i].Size, chatters[i].Size, Constants.DamageFrameCount);
                    chatters[i].HP -= GivePlayerAttack();
                    if (chatters[i].HP <= 0)
                    {
                        AddMeatCable(meat: Constants.ChattersDropMeat);
                        PlayerAddEXP(Constants.ChattersDropEXP);
                        KillChatters(i);
                        GetPlayerItem();
                    }
                }
            }
            if (currentModer != null)
            {
                if ((Math.Abs(attackCoordinates.Left - currentModer.Coordinates.Left) <= attackWeight + currentModer.Size) && (Math.Abs(attackCoordinates.Top - currentModer.Coordinates.Top) <= attackHeight + currentModer.Size))
                {
                    if ((currentModer.Name == "lancev0" || currentModer.Name == "lancev1") && (currentModer.State == "walk" || currentModer.State == "stand" || currentModer.State == "attack") && currentModer.MP >= currentModer.MPConsuptionRoll)
                        LancevRoll();
                    else
                    {
                        SpawnSpeceffectOnUnit(currentModer, cashe._item["damage"], currentModer.Size, currentModer.Size, Constants.DamageFrameCount);
                        currentModer.HP -= GivePlayerAttack();
                        if (currentModer.HP <= 0)
                            KillModer();
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
                    AddMeatCable(meat: -Constants.HalalcartCostMeat, cable: -Constants.HalalcartCostCable);
                    building.Size = Constants.SizeHalalcart;
                    building.HP = Constants.HalalcartHP;
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
                    AddMeatCable(meat: -Constants.WallCostMeat, cable: -Constants.WallCostCable);
                    building.Size = Constants.SizeWall;
                    building.HP = Constants.WallHP;
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

        double GivePlayerAttack()
        {
            double finalAttack = player.Attack;
            if (player.Name == "kara")
                finalAttack *= player.KaraAttackModificator;
            return finalAttack;
        }

        void KillNewbie(int index)
        {
            SpawnSpeceffectOnDot(newbies[index].Coordinates, cashe._item["death"], newbies[index].Size, newbies[index].Size, Constants.DeathFrameCount);
            gameUnitMapGrid.Children.Remove(newbieSprites[index]);
            newbieSprites.RemoveAt(index);
            newbies.RemoveAt(index);
        }

        void KillChatters(int index)
        {
            SpawnSpeceffectOnDot(chatters[index].Coordinates, cashe._item["death"], chatters[index].Size, chatters[index].Size, Constants.DeathFrameCount);
            gameUnitMapGrid.Children.Remove(chatterSprites[index]);
            chatterSprites.RemoveAt(index);
            chatters.RemoveAt(index);
            chattersFindCounter.RemoveAt(index);
        }

        async void KillModer()
        {
            if (currentModer.Name == "lancev0")
            {
                isPlay = false;
                moderSprite.Source = null;
                SpawnSpeceffectOnDot(currentModer.Coordinates, cashe._item["bossdeath"], currentModer.Size, currentModer.Size, Constants.DeathFrameCount);
                await Task.Delay(Constants.DeathFrameCount);
                SpawnSpeceffectOnDot(currentModer.Coordinates, cashe._item["lancevshadow0"], currentModer.Size, currentModer.Size, Constants.BossDeathFrameCount);
                await Task.Delay(Constants.BossDeathFrameCount);
                SpawnSpeceffectOnDot(currentModer.Coordinates, cashe._item["lancevshadow1"], currentModer.Size, currentModer.Size, Constants.BossDeathFrameCount*3);
                await Task.Delay(Constants.BossDeathFrameCount*3);
                SpawnSpeceffectOnDot(currentModer.Coordinates, cashe._unit[("lancev1", "splash", "down")][1], currentModer.Size+20, currentModer.Size, Constants.BossDeathFrameCount * 4);
                await Task.Delay(Constants.BossDeathFrameCount*4);
                currentModer.Name = "lancev1";
                currentModer.RegenMP = 0.07;
                currentModer.MP = currentModer.MaxMP;
                currentModer.HP = currentModer.MaxHP;
                currentModer.Attack = 35;
                currentModer.Speed = 7;
                currentModer.Size = 170;
                moderSprite.Width = 170;
                moderSprite.Height = 170;
                isPlay = true;
                GameTimer();
                return;
            }
            isBoss = false;
            kill_bosses_count++;
            SpawnSpeceffectOnDot(currentModer.Coordinates, cashe._item["bossdeath"], currentModer.Size*2, currentModer.Size*2, Constants.DeathFrameCount);
            currentModer = null;
            gameUnitMapGrid.Children.Remove(moderSprite);
            if (kill_bosses_count == Constants.ModersCount)
            {
                MessageBox.Show("Умнички, вы победили!!!");
            }
        }

        void GetPlayerItem()
        {
            Random rnd = new Random();
            if (rnd.Next(0, 101) > Constants.ChanseGetItem)
                return;
            SpawnSpeceffectOnUnit(player, cashe._item["find"], player.Size, player.Size, Constants.LevelUpFrameCount);
            int num = rnd.Next(Constants.ItemCount);
            string name = $"item{num}";
            if (inventoryItems.Count < 4)
            {
                inventoryItems.Add(name);
            }
            else if (handsItems.Count < 2)
            {
                handsItems.Add(name);
                AddStat(name);
            }
            else return;
            SortInventory();
        }

        void InventoryRightMouseDown(MouseButtonEventArgs e, int num) // расщепление предмета
        {
            e.Handled = true;
            if (inventoryItems.Count > num)
            {
                AddMeatCable(cable: Constants.ItemDropCable);
                inventoryItems.RemoveAt(num);
                SortInventory();
            }
        }

        void InventoryClick(int num) // перемещение из инвентаря
        {
            gameBuildingMapGrid.Focus();
            if (inventoryItems.Count > num && handsItems.Count < 2)
            {
                handsItems.Add(inventoryItems[num]);
                AddStat(inventoryItems[num]);
                inventoryItems.RemoveAt(num);
                SortInventory();
            }
        }

        void SortInventory() 
        {
            var images = new[] { inventory1Image, inventory2Image, inventory3Image, inventory4Image };
            var buttons = new[] { inventory1Button, inventory2Button, inventory3Button, inventory4Button };
            var count = inventoryItems.Count;

            for (int i = 0; i < images.Length; i++)
            {
                if (i < count)
                {
                    var item = inventoryItems[i];
                    images[i].Source = cashe._item[item];          
                    buttons[i].ToolTip = Constants.ItemDescription[item];
                }
                else
                {
                    images[i].Source = null;
                    buttons[i].ToolTip = null;
                }
            }

            images = new[] { hand1Image, hand2Image };
            buttons = new[] { hand1Button, hand2Button };
            count = handsItems.Count;

            for (int i = 0; i < images.Length; i++)
            {
                if (i < count)
                {
                    var item = handsItems[i];
                    images[i].Source = cashe._item[item];          
                    buttons[i].ToolTip = Constants.ItemDescription[item];
                }
                else
                {
                    images[i].Source = null;
                    buttons[i].ToolTip = null;
                }
            }

        }

        void HandRightMouseDown(MouseButtonEventArgs e, int num)
        {
            e.Handled = true;
            if (handsItems.Count > num)
            {
                AddMeatCable(cable: Constants.ItemDropCable);
                AddStat(handsItems[num], -1);
                handsItems.RemoveAt(num);
                SortInventory();
            }
        }

        void HandClick(int num)
        {
            gameBuildingMapGrid.Focus();
            if (handsItems.Count > num && inventoryItems.Count < 4)
            {
                AddStat(handsItems[num], -1);
                inventoryItems.Add(handsItems[num]);
                handsItems.RemoveAt(num);
                SortInventory();
            }
        }

        void AddStat(string item, int factor = 1) // factor для того, чтобы при снятии эффекты уменьшались
        {
            double newProcent;
            switch (item)
            {
                case "item0":
                    player.MaxHP += count_meat * factor;
                    player.MaxMP += count_meat * factor;
                    player.MaxStamina += count_meat * factor;
                    break;
                case "item1":
                    player.RegenHP += Constants.ItemStroborezRegenModificator * count_cable * factor;
                    player.RegenMP += Constants.ItemStroborezRegenModificator * count_cable * factor;
                    player.RegenStamina += Constants.ItemStroborezRegenModificator * count_cable * factor;
                    break;
                case "item2":
                    newProcent = player.HP / player.MaxHP;
                    if (factor > 0)
                    {
                        player.Speed += player.Speed * Constants.ItemModificator / 2 * factor;
                        player.MaxHP += player.MaxHP / Constants.ItemModificator * -factor;
                    }
                    else
                    {
                        player.Speed += player.Speed * Constants.ItemModificator/ 2 * factor / 2;
                        player.MaxHP += player.MaxHP / Constants.ItemModificator * -factor * 2;
                    }
                    player.HP = newProcent * player.MaxHP;
                    break;
                case "item3":
                    if (factor > 0)
                        player.RegenHP += 0.01 * Constants.ItemModificator * factor / 2;
                    else
                        player.RegenHP += 0.01 * Constants.ItemModificator * factor / 2;
                    break;
                case "item4":
                    newProcent = player.HP / player.MaxHP;
                    if (factor > 0)
                        player.MaxHP += player.MaxHP / Constants.ItemModificator * factor * 2;
                    else
                        player.MaxHP += player.MaxHP / Constants.ItemModificator * factor;
                    player.HP = newProcent * player.MaxHP;
                    break;
                case "item5":
                    if (factor > 0)
                    {
                        player.AttackWeight += player.AttackWeight * Constants.ItemModificator / 2 * factor;
                        player.AttackHeight += player.AttackHeight * Constants.ItemModificator / 2 * factor;
                        player.Attack += player.Attack / Constants.ItemModificator * -factor;
                    }
                    else
                    {
                        player.AttackWeight += player.AttackWeight * Constants.ItemModificator / 2 * factor / 2;
                        player.AttackHeight += player.AttackHeight * Constants.ItemModificator / 2 * factor / 2;
                        player.Attack += player.Attack / Constants.ItemModificator * -factor * 2;
                    }
                    break;
                case "item6":
                    if (factor > 0) 
                    {
                        beforeItem6MaxMP = -(player.MaxMP - 1); 
                        beforeItem6RegenMP = -player.RegenMP; 
                        beforeItemMaxStamina = player.MaxMP - 1;   
                        beforeItemRegenStamina = player.RegenMP; 
                    }
                    else
                    {
                        beforeItem6MaxMP = -beforeItem6MaxMP;
                        beforeItem6RegenMP = -beforeItem6RegenMP;
                        beforeItemMaxStamina = -beforeItemMaxStamina;
                        beforeItemRegenStamina = -beforeItemRegenStamina;
                    }
                    player.MaxMP += beforeItem6MaxMP;
                    player.RegenMP += beforeItem6RegenMP;
                    player.MaxStamina += beforeItemMaxStamina;
                    player.RegenStamina += beforeItemRegenStamina;
                    player.MP = 0;
                    break;
                case "item7":
                    newProcent = player.MP / player.MaxMP;
                    if (factor > 0)
                        player.MaxMP += player.MaxMP / Constants.ItemModificator * factor * 2;
                    else
                        player.MaxMP += player.MaxMP / Constants.ItemModificator * factor;
                    player.MP = newProcent * player.MaxMP;
                    break;
                case "item8":
                    if (factor > 0){
                        Constants.NewbieDropEXP += Constants.NewbieDropEXP * factor;
                        Constants.ChattersDropEXP += Constants.ChattersDropEXP * factor;
                        Constants.NewbieDropMeat = 0;
                        Constants.ChattersDropMeat = 0;
                    }
                    else{
                        Constants.NewbieDropEXP += Constants.NewbieDropEXP * factor / 2;
                        Constants.ChattersDropEXP += Constants.ChattersDropEXP * factor / 2;
                        Constants.NewbieDropMeat = 1;
                        Constants.ChattersDropMeat = 2;
                    }
                    break;
                case "item9":
                    if (factor > 0) 
                    {
                        beforeItem9MaxMP = -(player.MaxMP - 1);
                        beforeItem9RegenMP = -player.RegenMP;  
                        beforeItem9MaxHP = player.MaxMP - 1; 
                        beforeItem9RegenHP = player.RegenMP;   
                    }
                    else 
                    {
                        beforeItem9MaxMP = -beforeItem9MaxMP;
                        beforeItem9RegenMP = -beforeItem9RegenMP;
                        beforeItem9MaxHP = -beforeItem9MaxHP;
                        beforeItem9RegenHP = -beforeItem9RegenHP;
                    }

                    player.MaxMP += beforeItem9MaxMP;
                    player.RegenMP += beforeItem9RegenMP;
                    player.MaxHP += beforeItem9MaxHP;
                    player.RegenHP += beforeItem9RegenHP;
                    break;
                case "item10":
                    newProcent = player.Stamina / player.MaxStamina;
                    if (factor > 0)
                        player.MaxStamina += player.MaxStamina / Constants.ItemModificator * factor * 2;
                    else
                        player.MaxStamina += player.MaxStamina / Constants.ItemModificator * factor;
                    player.Stamina = newProcent * player.MaxStamina;
                    break;
                case "item11":
                    if (factor > 0)
                    {
                        Constants.NewbieDropMeat += Constants.NewbieDropMeat * factor;
                        Constants.ChattersDropMeat += Constants.ChattersDropMeat * factor;
                        Constants.NewbieDropEXP = 0;
                        Constants.ChattersDropEXP = 0;
                    }
                    else
                    {
                        Constants.NewbieDropMeat += Constants.NewbieDropEXP * factor / 2;
                        Constants.ChattersDropMeat += Constants.ChattersDropMeat * factor / 2;
                        Constants.NewbieDropEXP = 2;
                        Constants.ChattersDropEXP = 3;
                    }
                    break;
                case "item12":
                    if (factor < 0)
                    {
                        player.AttackWeight += player.AttackWeight * Constants.ItemModificator / 2 * -factor;
                        player.AttackHeight += player.AttackHeight * Constants.ItemModificator / 2 * -factor;
                        player.Attack += player.Attack / Constants.ItemModificator * factor;
                    }
                    else
                    {
                        player.AttackWeight += player.AttackWeight * Constants.ItemModificator / 2 * -factor / 2;
                        player.AttackHeight += player.AttackHeight * Constants.ItemModificator / 2 * -factor / 2;
                        player.Attack += player.Attack / Constants.ItemModificator * factor * 2;
                    }
                    break;
                case "item13":
                    if (factor > 0)
                        player.Speed *= -factor;
                    else
                        player.Speed *= factor;
                    break;
                case "item14":
                    if (factor > 0)
                    {
                        player.KaraSpeedModificator += player.KaraSpeedModificator * Constants.ItemModificator / 2 * factor;
                        player.KaraAttackModificator += player.KaraAttackModificator * Constants.ItemModificator / 2 * -factor / 2;
                    }
                    else
                    {
                        player.KaraSpeedModificator += player.KaraSpeedModificator * Constants.ItemModificator / 2 * factor / 2;
                        player.KaraAttackModificator += player.KaraAttackModificator * Constants.ItemModificator / 2 * -factor;
                    }
                    break;
                case "item15":
                    if (factor > 0)
                        player.RegenMP += player.RegenMP * Constants.ItemModificator * factor / 2;
                    else
                        player.RegenMP += player.RegenMP * Constants.ItemModificator * factor / 4;
                    break;
                case "item16":
                    if (factor > 0)
                    {
                        player.KaraAttackModificator += player.KaraAttackModificator * Constants.ItemModificator / 2 * factor;
                        player.KaraSpeedModificator += player.KaraSpeedModificator * Constants.ItemModificator / 2 * -factor / 2;
                    }
                    else
                    {
                        player.KaraAttackModificator += player.KaraAttackModificator * Constants.ItemModificator / 2 * factor / 2;
                        player.KaraSpeedModificator += player.KaraSpeedModificator * Constants.ItemModificator / 2 * -factor;
                    }
                    break;
                case "item17":
                    if (factor > 0)
                    {
                        player.StaminaConsuptionAttack += player.StaminaConsuptionAttack * Constants.ItemModificator / 2 * -factor / 2;
                        player.StaminaConsuptionRoll += player.StaminaConsuptionRoll * Constants.ItemModificator / 2 * -factor / 2;
                        player.StaminaConsuptionWalk += player.StaminaConsuptionWalk * Constants.ItemModificator / 2 * -factor / 2;
                    }
                    else
                    {
                        player.StaminaConsuptionAttack += player.StaminaConsuptionAttack * Constants.ItemModificator / 2 * -factor;
                        player.StaminaConsuptionRoll += player.StaminaConsuptionRoll * Constants.ItemModificator / 2 * -factor;
                        player.StaminaConsuptionWalk += player.StaminaConsuptionWalk * Constants.ItemModificator / 2 * -factor;
                    }
                    break;
                case "item18":
                    if (factor > 0)
                        player.RegenStamina += player.RegenStamina * Constants.ItemModificator * factor / 2;
                    else
                        player.RegenStamina += player.RegenStamina * Constants.ItemModificator * factor / 4;
                    break;
                case "item19":
                    if (factor > 0)
                        player.KaraConsuption += player.KaraConsuption * Constants.ItemModificator / 2 * -factor / 2;
                    else
                        player.KaraConsuption += player.KaraConsuption * Constants.ItemModificator / 2 * -factor;
                    break;
            }
            if (player.HP > player.MaxHP)
                player.HP = player.MaxHP;
            if (player.MP > player.MaxMP)
                player.MP = player.MaxMP;
            if (player.Stamina > player.MaxStamina)
                player.Stamina = player.MaxStamina;
            if (player.MaxHP <= 0)
                player.MaxHP = 1;
            if (player.MaxMP <= 0)
                player.MaxMP = 1;
        }

        void PizzaPower(int quantityDropPizza)
        {
            if (handsItems.Exists(x => x == "item0"))
            {
                player.MaxHP += quantityDropPizza;
                player.HP += quantityDropPizza;
                player.MaxMP += quantityDropPizza;
                player.MP += quantityDropPizza;
                player.MaxStamina += quantityDropPizza;
                player.Stamina += quantityDropPizza;
            }
        }

        void CablePower(int quantityDropCable)
        {
            if (handsItems.Exists(x => x == "item1"))
            {
                player.RegenHP += Constants.ItemStroborezRegenModificator * quantityDropCable;
                player.RegenMP += Constants.ItemStroborezRegenModificator * quantityDropCable;
                player.RegenStamina += Constants.ItemStroborezRegenModificator * quantityDropCable;
            }
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
            HandClick(1);
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

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            current_track++;
            if (current_track == playList.Count)
                current_track = 0;

            _mediaPlayer.Open(new Uri(playList[current_track], UriKind.Relative));
            _mediaPlayer.Play();
        }
    }
}