using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class Workbench : MonoBehaviour
    {
        public Transform items_parent;
        public Button button_prefab;
        public List<Slot> slots = new List<Slot>();
        public RecipeDatabase RecipeDatabase;

        public Text Name, Rarity, Description,Components;
       // public Slider Slider;
        public GameObject CraftButton;
        public Image Icon;

        //public int count = 1;
        private Slot selectedSlot;
        
        
        [Serializable]
        public class Slot
        {
            public Item item;
            public Button selectButton;
        }

        private void Start()
        {
            DisplayInfo(null);
        }

        private void OnEnable()
        {
            for (int i = 0; i < items_parent.childCount; i++)
            {
                Destroy(items_parent.GetChild(i).gameObject);
            }

            foreach (var recipe in RecipeDatabase.Recipes)
            {
                var b = Instantiate(button_prefab, items_parent);
            
                var slot = new Slot{item = recipe.result, selectButton = b};
            
                slots.Add(slot);
                b.onClick.AddListener(() => { DisplayInfo(slot); });
                b.GetComponentInChildren<Text>().text = "<color=#" + Extenshions.GetColorFrom(slot.item.Rarity) + ">" + slot.item.Name + "</color>";
            }
        }

        /*public void OnSliderChange(float value)
        {
            count = (int) value;
        }*/

        public void DisplayInfo(Slot slot)
        {
            //count = 1;
            if (slot != null)
            {
                Name.text ="<color=#" + Extenshions.GetColorFrom(slot.item.Rarity) + ">" + slot.item.Name + "</color>";
                Rarity.text = "<color=#" + Extenshions.GetColorFrom(slot.item.Rarity) + ">Rarity: " + slot.item.Rarity + "</color>";
                Description.text = slot.item.Description;
                //Slider.interactable = true;
                CraftButton.SetActive(true);
                Icon.sprite = slot.item.Icon;
                Components.text = string.Empty;
                var comp = RecipeDatabase.Recipes.First(n => n.result == slot.item).Components;
                for (int i = 0; i < comp.Length; i++)
                {
                    var inInv = GameManager.Instance.mainInventory.slots.FirstOrDefault(n =>
                        n.item.item == comp[i].item);
                    int have = inInv != null ? 
                        GameManager.Instance.mainInventory.slots.First(n => n.item.item == comp[i].item).item.count 
                        : 0;

                    bool red = have < comp[i].count;
                    
                    
                    Components.text += "<color="+ (red ? "red" : "green") +">" + comp[i].item.Name + " " + have + "/" + comp[i].count + " </color>\n";
                }
                
                Icon.gameObject.SetActive(true);
                
                selectedSlot = slot;
            }
            else
            {
                Name.text = string.Empty;
                Rarity.text = string.Empty;
                Description.text = string.Empty;
                //Slider.interactable = false;
                CraftButton.SetActive(false);
                Icon.sprite = null;
                Components.text = string.Empty;
                Icon.gameObject.SetActive(false);
                selectedSlot = null;
            }
        }

        public void Craft()
        {
            var comp = RecipeDatabase.Recipes.First(n => n.result == selectedSlot.item).Components;
            
            
            
            for (var i = 0; i < comp.Length; i++)
            {
                var inInv = GameManager.Instance.mainInventory.slots.FirstOrDefault(n =>
                    n.item.item == comp[i].item);
                var have = inInv != null ? 
                    GameManager.Instance.mainInventory.slots.First(n => n.item.item == comp[i].item).item.count 
                    : 0;

                if (have < comp[i].count)
                {
                    return;
                }
            }
            
            
            foreach (var component in comp)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.mainInventory.slots.First(n =>
                        n.item.item == component.item).item.count -= component.count;
            }

            GameManager.Instance.mainInventory.AddItem(new ItemInstance{ item = selectedSlot.item, condition = selectedSlot.item.MaxCondition, count = 1});
            DisplayInfo(null);
        }
    }
}