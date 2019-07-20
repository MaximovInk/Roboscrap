using System;
using System.IO;
using System.Runtime.InteropServices;
using MaximovInk.AI;
using UnityEngine;

namespace MaximovInk
{
    public  static  class Extenshions
    {        
        public static readonly System.Random random = new System.Random();
        public static void Populate<T>(this T[] arr, T value ) {
            for ( var i = 0; i < arr.Length;i++ ) {
                arr[i] = value;
            }
        }

        public static uint ToUint(this Color32 color)
        {
            return (uint)((color.a << 24) | (color.r << 16) |
                          (color.g << 8)  | (color.b << 0));
        }

        public static Color ToColor(this uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return new Color32(a, r, g, b);
        }

        public static bool FractionsEnemy(NPC_Group a, NPC_Group b)
        {
            if (a == NPC_Group.Agro || b == NPC_Group.Agro)
                return true;

            if (a == NPC_Group.Cpu || b == NPC_Group.Cpu)
                return false;

            return a != b;
        }

        public static float GetRandomFloat(this System.Random random,float minimum, float maximum)
        { 
            return (float)random.NextDouble() * (maximum - minimum) + minimum;
        }
        
        public static Texture2D ToTexture2D(this Texture texture)
        {
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }
        
        public static void MoveFromTo(string from, string to)
        {

            foreach (string dirPath in Directory.GetDirectories(from, "*",
                 SearchOption.AllDirectories))
                 Directory.CreateDirectory(dirPath.Replace(from, to));
 
             foreach (string newPath in Directory.GetFiles(from, "*.*",
                 SearchOption.AllDirectories))
                 File.Copy(newPath, newPath.Replace(from, to), true);


        }

        public static void CleanDirectory(string path)
        {
            var di = new DirectoryInfo(path);

            foreach (var file in di.GetFiles())
            {
                file.Delete(); 
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true); 
            }
        }

        public static T[] Add<T>(this T[] target, T item)
        {
            if (target == null)
            {
                return null;
            }
            var result = new T[target.Length + 1];
            target.CopyTo(result, 0);
            result[target.Length] = item;
            return result;
        }


        public static string GetColorFrom(Rarity rarity)
        {
            switch (rarity)
            {
                case MaximovInk.Rarity.Common:
                    return "FFF";
                case MaximovInk.Rarity.Uncommon:
                    return "49CF27";
                case MaximovInk.Rarity.Rare:
                    return "E6125C";
                case MaximovInk.Rarity.Epic:
                    return "CC00FF";
                case MaximovInk.Rarity.Legendary:
                    return "E0D616";
                default:
                    return "000";
            }
        }
        
        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }
        
        [StructLayout(LayoutKind.Explicit)]
        struct UIntFloat
        {       
            [FieldOffset(0)]
            public float FloatValue;

            [FieldOffset(0)]
            public uint IntValue;        
        }
        
        public static float ToSingle(uint value)
        {
            var uf = new UIntFloat();
            uf.IntValue = value;
            return uf.FloatValue;
        }
    }
    
}