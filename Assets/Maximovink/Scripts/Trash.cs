using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class Trash : SavedPrefabBehaviour
    {
        public byte hp = 100;

       public Item[] Resources;

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
                
                
                var generate = (int) Random.Range(10, 450);
                var trash = GetComponent<Trash>();


                for (int i = 0; i < generate; i++)
                {
                    var id = Random.Range(0, Resources.Length);
                    
                    var item = new ItemInstance { item = Resources[id] };
                    item.count += 1;
                    item.condition = Random.Range(item.item.MaxCondition/100,item.item.MaxCondition);
                    GameManager.Instance.mainInventory.AddItem(item);
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
}