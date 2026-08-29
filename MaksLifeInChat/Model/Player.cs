using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Player : Unit
    {
        public double MP { get; set; } = 50;
        public double MaxMP { get; set; } = 50;
        public double Stamina { get; set; } = 50;
        public double MaxStamina { get; set; } = 50;
        public double RegenHP { get; set; } = 0;
        public double RegenMP { get; set; } = 0.01;
        public double RegenStamina { get; set; } = 0.05;
        public double KaraConsuption { get; set; } = 0.05;
        public double StaminaConsuptionWalk { get; set; } = 0.2;
        public double KaraSpeedModificator { get; set; } = 1.3;
        public double KaraAtackModificator { get; set; } = 1.6;
        public double RunModificator { get; set; } = 2;
        public bool IsRun { get; set; } = false;
        public int EXP { get; set; } = 0;
        public int Level { get; set; } = 0;
        public int EXPForLevelUP { get; set; } = 10;
        public double EXPForLevelUPModificator { get; set; } = 1.3; // при новом уровне EXPForLevelUP на свой модификатор и хр обнуляется
        public int RollSpeed { get; set; } = 13;
        public double StaminaConsuptionRoll { get; set; } = 25;
        public double StaminaConsuptionAttack { get; set; } = 10;
    }
}
