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
        public static int ModersCount = 1;
        public static int ItemCount = 20;
        public static int ItemDropCable = 5;
        public static double ItemModificator = 2;
        public static double ItemStroborezRegenModificator = 0.001;
        public static int ChanseGetItem = 33;
        public static readonly string[] interfaceNames = 
            { "building" , "saucer" , "inventory", "hand" };
        public static readonly string[] itemNames =
            { "shawarmaMP", "shawarmaHP", "nintendoHP" , "attack_down", "attack_up" ,
            "attack_left" , "attack_right" , "levelup" , "bossdeath" , "death", "chatters_attack_down",
            "chatters_attack_up", "chatters_attack_left", "chatters_attack_right", "heal_recovery",
            "mana_recovery", "stamina_recovery", "damage", "lancevshadow0" , "lancevshadow1", "find" ,
            "lancev_attack_down", "lancev_attack_up" , "lancev_attack_left" , "lancev_attack_right" };
        public static readonly string[] unitNames = 
            { "kay", "kara", "halalcart", "wall", "newbie0" , "newbie1" , "chatters0", "chatters1", "chatters2", "chatters3", "chatters4", "lancev0", "lancev1" };
        public static readonly string[] states = { "stand", "attack", "walk" , "roll" , "welcome" , "splash" , "shoot" };
        public static readonly string[] rotations = { "down", "left", "up", "right" };
        public static readonly Dictionary<string,string> ItemDescription = new Dictionary<string, string> {
            { "item0", "Нинтендо\n\n" +
                "Увеличение запаса человечности, жестокости и выносливости на количество пицц."},
            { "item1", "Штроборез\n\n" +
                "Увеличение восстановления человечности, жестокости\n" +
                "и выносливости на колличество кабелей."},
            { "item2", "Очки гитариста\n\n" +
                "Увеличение скорости ценой запаса человечности."},
            { "item3", "Немой листик\n\n" +
                "Увеличение регенерации человечности."},
            { "item4", "Перо волка\n\n" +
                "Увеличение запаса человечности."},
            { "item5", "Склизкая лопата\n\n" +
                "Увеличение области атаки ценой наносимого урона."},
            { "item6", "Мини трость\n\n" +
                "Жертва всей жестокости ради выносливости."},
            { "item7", "Пыльный архив\n\n" +
                "Увеличение запаса жестокости."},
            { "item8", "Разделённые ножницы\n\n" +
                "Увеличение выпадаемого опыта ценой выпадаемой пиццы."},
            { "item9", "Грозная маска\n\n" +
                "Жертва всей жестокости ради человечности."},
            { "item10", "Футболка мотылька\n\n" +
                "Увеличение запаса выносливости."},
            { "item11", "Банан?\n\n" +
                "Увеличение выпадаемой пиццы ценой выпадаемого опыта."},
            { "item12", "Нож для карликов\n\n" +
                "Увеличение наносимого урона ценой области атаки."},
            { "item13", "Статуя ленина\n\n" +
                "Дарует лунную походку"},
            { "item14", "Обожжённая доска\n\n" +
                "Усиление скорости в состоянии кары\nценой урона Кары."},
            { "item15", "Тату арии\n\n" +
                "Увеличение регенерации жестокости."},
            { "item16", "Карманный монстр\n\n" +
                "Усиление урона в состоянии кары\nценой скорости Кары."},
            { "item17", "Рог виверны\n\n" +
                "Уменьшение расхода выносливости."},
            { "item18", "Кровяная колбаса\n\n" +
                "Увеличение регенерации выносливости."},
            { "item19", "dQw4w9WgXcQ\n\n" +
                "Уменьшение расхода состояния кары."},
        };
        public static int SizeHalalcart = 150;
        public static int SizeWall = 50;
        public static int SpawnNewbieDelaySec = 5;
        public static int SpawnBossDelaySec = 300;//300
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
        public static int DamageFrameCount = SpriteUpdateFrameCount;
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
        public static double LevelGiveRegenHP = 0;
        public static double LevelGiveRegenMP = 0.0004;
        public static double LevelGiveRegenStamina = 0.0006;
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
        public static double DuelDistant = 1500;
    }
}
