using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MaksLifeInChat.Model
{
    public class Unit
    {
        public int ID { get; set; }
        public int Size { get; set; } = 110; // px
        public double Speed { get; set; } = 5;
        public string State { get; set; } = "stand";
        public int ProgressSprite { get; set; } = 0;
        public string Rotation { get; set; } = "down";
        public string? SecondRotation { get; set; }
        public string Name { get; set; } = "kay";
        public double HP { get; set; } = 50;
        public double MaxHP { get; set; } = 50;
        public double Atack { get; set; } = 5;
        public Thickness Coordinates { get; set; } = new Thickness(0,0,0,0);
    }
}
