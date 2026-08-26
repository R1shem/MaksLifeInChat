using System;
using System.Collections.Generic;
using System.Text;

namespace MaksLifeInChat.Model
{
    public class Newbie : Unit
    {
        public int VariationSprite { get; set; } = 1; // index_last_variation (start with 0)
        public int OpacityProgress { get; set; } = 0; // при 100 - удаление
        public int OpacityFrameCountProgress { get; set; } = 0; // при достижении значения из константы - обнуление и опаситипрогресс++
        public bool IsWelcoming { get; set; } = false; // если да - чатится, если нет - исчезает
        public int ChattersingProgress { get; set; } = 0; // при 100 - превращение в случайного чаттерса
        public int ChattersingFrameCountProgress { get; set; } = 0; // при достижении значения из константы - обнуление и чаттерсингпрогресс++
    }
}
