using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Constants
    {
        public const string ResourceFolder = "Resource/";
        public static int FPS = 5;
        public static int Second = 900;
        public static double ProcentShawarmaRegenHP = 0.25;
        public static double ProcentShawarmaRegenMP = 0.30;
        public static int SpriteUpdateFrameCount = 50;
        public static readonly string[] interfaceNames = { "building" , "saucer" , "inventory", "hand" };
        public static readonly string[] itemNames = { "shawarmaMP", "shawarmaHP", "nintendoHP" , "attack_down", "attack_up" , "attack_left" , "attack_right" , "levelup" , "bossdeath" , "death", "chatters_attack_down", "chatters_attack_up", "chatters_attack_left", "chatters_attack_right" };
        public static readonly string[] unitNames = { "kay", "kara", "halalcart", "wall", "newbie0" , "newbie1" , "chatters0" };
        public static readonly string[] states = { "stand", "attack", "walk" , "roll" , "welcome" };
        public static readonly string[] rotations = { "down", "left", "up", "right" };
        public static int SizeHalalcart = 150;
        public static int SizeWall = 50;
        public static int SpawnNewbieDelaySec = 5;

        public static double NewbieHP  = 10;
        public static int NewbieDropMeat = 1;
        public static int NewbieDropEXP = 2;
        public static int NewbieSize = 80; // px
        public static double NewbieSpeed = 2;
        public static int NewbieOpacityFrameCount = 30;
        public static int NewbieChattersingFrameCount = 50;
        public static int PlayerRollFrameCount = SpriteUpdateFrameCount*7;
        public static int PlayerWelcomeFrameCount = SpriteUpdateFrameCount*5;
        public static int PlayerAttackFrameCount = SpriteUpdateFrameCount*10;
        public static int LevelUpFrameCount = SpriteUpdateFrameCount*5;
        public static int DeathFrameCount = SpriteUpdateFrameCount*5;
        public static int BossDeathFrameCount = SpriteUpdateFrameCount*15;
        public static int FindEnemyFrameCount = SpriteUpdateFrameCount;
        public static double PlayerAttackPiasProcent = 1;
        public static double SizeSpriteDeathPlayer = 150;

        public static int HalalcartCostCable = 13;
        public static int HalalcartCostMeat = 0;
        public static  string HalalcartDescription = $"Шаурмичная ({HalalcartCostMeat}🍕 {HalalcartCostCable}🔌)\n\nСамая настоящая халяльная шаурмичная.";
        public static int WallCostCable = 2;
        public static int WallCostMeat = 0;
        public static string WallDescription = $"Забор ({WallCostMeat}🍕 {WallCostCable}🔌)\n\nНастолько низкий, что через него не составляет труда перешагнуть, \nно чаттерсы из уважения таким промышлять не будут (¬‿¬).";
        public static int ShawarmaHPCostCable = 0;
        public static int ShawarmaHPCostMeat = 5;
        public static string ShawarmaHPDescription = $"Фирменная шаурма ({ShawarmaHPCostMeat}🍕 {ShawarmaHPCostCable}🔌)\n\nСамая обычная шаурма с бодро виляющим хвостиком. \nВосстанавливает человечность.";
        public static int ShawarmaMPCostCable = 2;
        public static int ShawarmaMPCostMeat = 1;
        public static string ShawarmaMPDescription = $"Веганская шаурма ({ShawarmaMPCostMeat}🍕 {ShawarmaMPCostCable}🔌)\n\nВеганская шаурма, состоящая из натуральных \nингредиентов, полностью экологична. \nВосстанавливает жестокость.";

        public static double LevelGiveHP = 2;
        public static double LevelGiveMP = 2;
        public static double LevelGiveStamina = 3;
        public static double LevelGiveAttack = 1;


        public static double ChattersAttack = 5;
        public static int ChattersAttackPauseDelay = 500;
        public static double ChattersHP = 20;
        public static int ChattersDropMeat = 2;
        public static int ChattersDropEXP = 3;
        public static int ChattersSize = 100; // px
        public static double ChattersSpeed = 4;
        public static int ChattersAttackFrameCount = SpriteUpdateFrameCount*15;
        public static double ChatterGainDayModificator = 0.1; // усиление статов чаттерсов в зависимости от дня HP = ChattersHP + ChattersHP * ChatterGainDayModificator * day_count

        public static double HalalcartHP = 45;
        public static double WallHP = 25;
    }
}
