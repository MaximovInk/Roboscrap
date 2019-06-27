using UnityEngine;

namespace MaximovInk
{
    public  static  class Extenshions
    {
        public static void Populate<T>(this T[] arr, T value ) {
            for ( int i = 0; i < arr.Length;i++ ) {
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
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return new Color32(a, r, g, b);
        }

       
        public static T[] Add<T>(this T[] target, T item)
        {
            if (target == null)
            {
                //TODO: Return null or throw ArgumentNullException;
            }
            T[] result = new T[target.Length + 1];
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
    }
}