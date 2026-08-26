using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Constants
    {
        public const string resourceFolder = "Resource/";
        public static int FPS = 5;
        public static int second = 900;
        public static int SpriteUpdateFrameCount = 50;
        public static readonly string[] itemNames = { "shawarmaMP", "shawarmaHP", "nintendoHP", "building" , "saucer" };
        public static readonly string[] unitNames = { "kay", "kara", "halalcart", "wall" };
        public static readonly string[] states = { "stand", "attack", "walk" };
        public static readonly string[] rotations = { "down", "left", "up", "right" };
        public static int SizeHalalcart = 150;
        public static int SizeWall = 50;
    }
}
