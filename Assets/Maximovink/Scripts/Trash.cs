using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class Trash : SavedPrefabBehaviour
    {
        private int hp = 255;

       public Item[] Resources;

       public TextMesh textCondition;


       private Entity entity;

       private void Awake()
       {
           entity = GetComponent<Entity>();
       }

       /*private void Start()
       {
           var ht = Physics2D.OverlapBoxAll(transform.position, Vector2.one *10, 0);
           var hts = ht.Where(n => n.GetComponentInParent<Trash>() && n.GetComponentInParent<Trash>().gameObject != gameObject).ToList();
           if (hts.Count > 0)
           {
               Debug.Log("Destroy");
              
               Destroy(gameObject);
           }
           
          
       }*/

       public void Attack(int amount)
        {
            hp -= amount;
            
            textCondition.gameObject.SetActive(true);

            textCondition.text = hp + " hp";
            
            if (hp <= 0)
            {
                var generate = Random.Range(10, 450);


                for (int i = 0; i < generate; i++)
                {
                    var id = Random.Range(0, Resources.Length);
                    
                    var item = new ItemInstance { item = Resources[id] };
                    item.count += 1;
                    item.condition = item.item.Unbreakable ? item.item.MaxCondition : Random.Range(item.item.MaxCondition/100,item.item.MaxCondition);
                    GameManager.Instance.mainInventory.AddItem(item);
                }
                
                gameObject.SetActive(false);
            }
        }

       public void UpdateInfo()
       {
           textCondition.gameObject.SetActive(hp < 255 && hp > 0);

           textCondition.text = hp + " hp";

           if (hp <= 0)
           {
               gameObject.SetActive(false);
           }
       }

       protected override uint OnSave()
       {
           return (uint)Mathf.Clamp(hp,0,255);
       }

       protected override void OnLoad(uint data)
       {
           hp = Mathf.Clamp((int)data,0,255);
           UpdateInfo();
           entity.Sort();
           
       }
    }
}