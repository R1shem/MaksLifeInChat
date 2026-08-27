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
        public static readonly string[] itemNames = { "shawarmaMP", "shawarmaHP", "nintendoHP" };
        public static readonly string[] unitNames = { "kay", "kara", "halalcart", "wall", "newbie0" , "newbie1" };
        public static readonly string[] states = { "stand", "attack", "walk" , "roll" };
        public static readonly string[] rotations = { "down", "left", "up", "right" };
        public static int SizeHalalcart = 150;
        public static int SizeWall = 50;
        public static int SpawnNewbieDelaySec = 5;

        public static double NewbieHP  = 10;
        public static int NewbieSize = 80; // px
        public static double NewbieSpeed = 2;
        public static int NewbieOpacityFrameCount = 30;
        public static int NewbieChattersingFrameCount = 50;
        public static int PlayerRollFrameCount = SpriteUpdateFrameCount*7;
    }
}
