using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Unit
    {
        public int ID { get; set; }
        public int Size { get; set; } = 110;
        public double Speed { get; set; } = 5;
        public string State { get; set; } = "stand";
        public int ProgressSprite { get; set; } = 0;
        public string Rotation { get; set; } = "down";
        public string Name { get; set; } = "kay";
        public int HP { get; set; } = 100;
        public int MP { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public double RegenHP { get; set; } = 0;
        public double RegenMP { get; set; } = 1;
        public double RegenStamina { get; set; } = 5;

    }
}
