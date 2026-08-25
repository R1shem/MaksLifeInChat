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
        public readonly Dictionary<(string Name, string State, string Rotation), List<BitmapImage>> _cache = new();
        private List<BitmapImage> GetSprites(string name, string state, string rotation)
        {
            var key = (name, state, rotation);
            if (_cache.TryGetValue(key, out var list))
                return list;

            list = new List<BitmapImage>();
            int progress = 0;
            while (true)
            {
                string fileName = $"{Constants.resourceFolder}{name}{state}{rotation}{progress}.png";
                if (!File.Exists(fileName)) break;
                BitmapImage image = LoadImage(fileName);
                list.Add(image);
                progress++;
            }
            _cache[key] = list;
            return list;
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

        public void GetSpritesList(string[] unitNames, string[] states, string[] rotations)
        {
            foreach (var name in unitNames)
                foreach (var state in states)
                    foreach (var rot in rotations)
                        GetSprites(name, state, rot);
        }
    }
}
