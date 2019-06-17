using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class Tooltip : MonoBehaviour
    {
        public Text Name, Descritption, Rarity,Condition;
        public Button drop,equip;
        public Image icon;

        private void Start()
        {
            SelectItem(null,null);
        }

        public void SelectItem(Inventory.Slot slot, Inventory inv)
        {
            if (slot == null || inv == null || slot.item.item == null)
            {
                Name.text = Descritption.text = Rarity.text = Condition.text = string.Empty;
                drop.gameObject.SetActive(false);
                equip.gameObject.SetActive(false);
                icon.gameObject.SetActive(false);
                return;    
            }

            if (slot.item.item is WeaponItem)
            {
                equip.gameObject.SetActive(true);
                equip.onClick.RemoveAllListeners();
                equip.onClick.AddListener(() =>
                {
                    var s = slot.item.Clone();
                    if (GameManager.Instance.player.EquipWeapon(s))
                    {
                        inv.RemoveItem(slot.item);
                        SelectItem(null,null);
                    }
                });
            }
            
            drop.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
            icon.sprite = slot.item.item.Icon;
            drop.onClick.RemoveAllListeners();
            drop.onClick.AddListener(() => { inv.RemoveItem(slot.item); GameManager.Instance.DropItem(slot); SelectItem(null,null); });
            
            Name.text = slot.item.item.Name + "(" + slot.item.count + ")";
            Descritption.text = slot.item.item.Description;
            Rarity.text = "Rarity: " + slot.item.item.Rarity;
            Condition.text = "Condition: " + slot.item.condition + "/" + slot.item.item.MaxCondition;
        }
    }
}