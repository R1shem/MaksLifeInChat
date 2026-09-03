using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Moderator : Unit
    {
        public double MP { get; set; } = 100;
        public double MaxMP { get; set; } = 100;
        public double RegenMP { get; set; } = 0.05;
        public double MPConsuptionRoll { get; set; } = 25;
        public double MPConsuptionAttackBase { get; set; } = 15;
        public double MPConsuptionAttackSplash { get; set; } = 25;
        public bool IsBanish = false; // режим экономии энергии ланца
    }
}
