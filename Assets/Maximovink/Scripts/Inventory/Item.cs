using UnityEngine;

namespace MaximovInk
{
    [CreateAssetMenu(menuName = "item/base")]
    public class Item : ScriptableObject
    {
        public Rarity Rarity;
        public string Name,Description;
        public Sprite Icon, DroppedSprite;
        public bool Unbreakable = false;
        
        public int MaxCondition;

        public Sprite GetDropSprite()
        {
         return DroppedSprite != null ? DroppedSprite : Icon;
        }
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic ,
        Legendary
    }
}