using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class Inventory : MonoBehaviour
    {
        public Transform items_parent;
        public Button button_prefab;
        public List<Slot> slots = new List<Slot>();
        [Serializable]
        public class Slot
        {
            public ItemInstance item;
            public Button selectButton;
        }

        public bool AddItem(ItemInstance item)
        {
            if (item.item == null || item.count == 0 || item.condition == 0)
                return false;
            
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].item.item == item.item && slots[i].item.condition == item.condition)
                {
                    slots[i].item.count += item.count;
                    return true;
                }
                
            }

            var b = Instantiate(button_prefab, items_parent);
            
            var slot = new Slot{item = item, selectButton = b};
            
            slots.Add(slot);
            b.onClick.AddListener(() => { GameManager.Instance.tooltip.SelectItem(slot,this); });
            b.GetComponentInChildren<Text>().text = "<color=#" + Extenshions.GetColorFrom(slot.item.item.Rarity) + ">" + item.item.Name + "(" + item.count + ")</color>";
            return true;
        }

        public bool RemoveItem(ItemInstance item, int count = -1)
        {
            var slot = slots.FirstOrDefault(x =>
                           x.item.item == item.item && x.item.condition == item.condition);
            if (slot == null)
                return false;
            
            if (count == -1)
            {
                Destroy(slot.selectButton.gameObject);
                slots.Remove(slot);
            }
            else
            {
                slot.item.count -= count;
                if (slot.item.count > 0)
                    return true;
                Destroy(slot.selectButton.gameObject);
                slots.Remove(slot);
            }

            return true;

        }
    }


[Serializable]
    public class ItemInstance
    {
        public Item item = null;
        public int count = 0;
        public float condition = 0;

        public ItemInstance Clone()
        {
            return new ItemInstance { item =item,count = count,condition = condition};
        }
    }
}