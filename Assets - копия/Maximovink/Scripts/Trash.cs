using System;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

namespace MaximovInk
{
    public class Trash : SavedPrefabBehaviour
    {
        public byte hp = 100;

       public Resource[] Resources;

       public TextMesh textCondition;


       private void Start()
       {
           var ht = Physics2D.OverlapBoxAll(transform.position, Vector2.one *10, 0);
           var hts = ht.Where(n => n.GetComponentInParent<Trash>() && n.GetComponentInParent<Trash>().gameObject != gameObject).ToList();
           if (hts.Count > 0)
           {
               Destroy(gameObject);
           }
           
           GetComponent<Entity>().Sort();
       }

       public void Attack(float amount)
        {
            hp -= (byte)amount;
            
            textCondition.gameObject.SetActive(true);

            textCondition.text = hp + " hp";
            
            if (hp <= 0)
            {
                /* Добыть мусор */
                for (int i = 0; i < Resources.Length; i++)
                {
                    GameManager.Instance.mainInventory.AddItem(Resources[i].item);
                }
                Destroy(gameObject);
            }
            
        }

       public override uint OnSave()
       {
           return hp;
       }

       public override void OnLoaded(uint data)
       {
           hp = (byte)data;
       }
    }
[Serializable]
    public class Resource
    {
        public ItemInstance item;
        public ResourceType type;
    }

    public enum ResourceType
    {
        Wood,
        Metal,
        Glass,
        Microschemes,
        Wires,
        Rubber,
        RadioElectricity,
        ElectoComponents,
        Components
    }
}