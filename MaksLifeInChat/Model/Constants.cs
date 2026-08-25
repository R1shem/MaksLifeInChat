using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Constants
    {
        public const string resourceFolder = "Resource/";
        public static int FPS = 5;
        public static int second = 15;
        public static int SpriteUpdateFrameCount = 50;
        public static readonly string[] unitNames = { "kay" };
        public static readonly string[] states = { "stand", "attack", "walk" };
        public static readonly string[] rotations = { "down", "left", "up", "right" };
    }
}
