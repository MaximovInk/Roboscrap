using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MaximovInk
{
    public class Breakable : SavedPrefabBehaviour
    {
        private int hp = 0;
        
       public RandomResource[] Resources;

       public int min = 10,max = 450;

       public int maxHp = 10;
        [Serializable]
       public class RandomResource
       {
           public Item item;
           [Range(0,1)]
           public float chance;
       }

       public void Attack(int amount)
        {
            hp -= amount;
            
            if (hp <= 0)
            {                
                var generate = Random.Range(min, max);


                for (var i = 0; i < generate; i++)
                {
                    var shit = Resources.Where(n => n.chance > Random.Range(0.0f, 1.0f)).ToArray();
                    
                    if(shit.Length== 0)
                        continue;
                    
                    var item = new ItemInstance
                    {
                        item = shit[Random.Range(0,shit.Length)].item, 
                        count = 1
                    };
                    item.condition = item.item.Unbreakable ? item.item.MaxCondition : Random.Range(item.item.MaxCondition/100,item.item.MaxCondition);
                    GameManager.Instance.mainInventory.AddItem(item);

                    GameManager.Instance.TextMesh.gameObject.SetActive(false);
                }

                hp = 0;
                
                gameObject.SetActive(false);
                
                ChunkManager.Instance.SaveLoadedChunks();
            }

            if (GetComponentInChildren<Renderer>().isVisible)
            {
                GameManager.Instance.TextMesh.transform.position = transform.position + Vector3.back;
                GameManager.Instance.SetTextMesh( hp + " hp");
            }
        }

       public void UpdateInfo()
       {
           if (hp <= 0)
           {
               gameObject.SetActive(false);
           }
       }

       public override object[] OnSave()
       {
           return new object[]{ Mathf.Clamp(hp,0,255) };
       }

       public override void OnLoad(object[] data)
       {
           base.OnLoad(data);
           
           
           
           if (data == null || data.Length == 0)
           {
               hp = maxHp;
           }
           else
            hp = Mathf.Clamp((int)data[0],0,255);

           UpdateInfo(); 
       }
    }
}