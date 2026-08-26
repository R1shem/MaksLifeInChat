using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace MaksLifeInChat.Model
{
    public class SpriteCache
    {
        public readonly Dictionary<(string Name, string State, string Rotation), List<BitmapImage>> _unit = new();
        public readonly Dictionary<string, BitmapImage> _item = new();
        private List<BitmapImage> GetUnitSprites(string name, string state, string rotation)
        {
            var key = (name, state, rotation);
            if (_unit.TryGetValue(key, out var list))
                return list;

            list = new List<BitmapImage>();
            int progress = 0;
            while (true)
            {
                string fileName = $"{Constants.ResourceFolder}{name}{state}{rotation}{progress}.png";
                if (!File.Exists(fileName)) break;
                BitmapImage image = LoadImage(fileName);
                list.Add(image);
                progress++;
            }
            _unit[key] = list;
            return list;
        }
        private BitmapImage GetItemSprites(string name)
        {
            string fileName = $"{Constants.ResourceFolder}{name}.png";
            if (!File.Exists(fileName)) return null;
            BitmapImage image = LoadImage(fileName);
            _item[name] = image;
            return image;
        }

        private static BitmapImage LoadImage(string fileName)
        {
            byte[] byteArray = File.ReadAllBytes(fileName);
            BitmapImage image = new();
            using (var stream = new MemoryStream(byteArray))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }

        public void GetUnitSpritesList(string[] unitNames, string[] states, string[] rotations)
        {
            foreach (var name in unitNames)
                foreach (var state in states)
                    foreach (var rot in rotations)
                        GetUnitSprites(name, state, rot);
        }

        public void GetItemSpritesList(string[] itemNames)
        {
            foreach (var name in itemNames)
                GetItemSprites(name);
        }
    }
}
