using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class Breakable : SavedPrefabBehaviour
    {
        private int hp = 0;
        
       public Item[] Resources;

       //public TextMesh textCondition;

       public int min = 10,max = 450;

       public int maxHp = 255;
       
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
            
           // textCondition.gameObject.SetActive(true);

           // textCondition.text = hp + " hp";
            
            
            if (hp <= 0)
            {
                var generate = Random.Range(min, max);


                for (var i = 0; i < generate; i++)
                {
                    var id = Random.Range(0, Resources.Length);
                    
                    var item = new ItemInstance { item = Resources[id] };
                    item.count += 1;
                    item.condition = item.item.Unbreakable ? item.item.MaxCondition : Random.Range(item.item.MaxCondition/100,item.item.MaxCondition);
                    GameManager.Instance.mainInventory.AddItem(item);
                    GameManager.Instance.TextMesh.gameObject.SetActive(false);
                }
                
                gameObject.SetActive(false);
            }

            if (GetComponentInChildren<Renderer>().isVisible)
            {
                GameManager.Instance.TextMesh.transform.position = transform.position + Vector3.back;
                GameManager.Instance.SetTextMesh( hp + " hp");
            }
        }

       public void UpdateInfo()
       {
           //textCondition.gameObject.SetActive(hp < 255 && hp > 0);

           //textCondition.text = hp + " hp";

           if (hp <= 0)
           {
               gameObject.SetActive(false);
           }
       }

       protected override int OnSave()
       {
           return Mathf.Clamp(hp,0,255);
       }

       protected override void OnLoad(int data)
       {
           base.OnLoad(data);
           
           hp = Mathf.Clamp((int)data,0,255);
           if (data == -1)
           {
               hp = maxHp;
           }

           UpdateInfo(); 
       }
    }
}