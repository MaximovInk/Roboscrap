using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MaximovInk
{
    public class TrashGenerator : MonoBehaviour
    {
        public float umnoj = 15;

        public Material material;
        
        public float Range = 2;

        public Resource[] sprites;

        [Serializable]
        public class Resource
        {
            public Sprite sprite;
            public ResourceType type;
            public Item item;
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            var generate = (int) Random.Range(Range*umnoj*0.99f, Range*15);
            var trash = GetComponent<Trash>();


            for (int i = 0; i < generate; i++)
            {
                var sprite = new GameObject();
                var id = Random.Range(0, sprites.Length);
                sprite.AddComponent<SpriteRenderer>().sprite = sprites[id].sprite;
                sprite.GetComponent<SpriteRenderer>().material = material;
                var t = trash.Resources.FirstOrDefault(n => n.type == sprites[id].type);
                t.item = new ItemInstance { item = sprites[id].item };
                t.item.count += 1;
                t.item.condition = Random.Range(t.item.item.MaxCondition/100,t.item.item.MaxCondition);
                sprite.transform.SetParent(transform);
                float y = Random.Range(0, Range);
                sprite.transform.localPosition = new Vector3(Mathf.Clamp(Random.Range(-Range,Range), -(Range-y),Range-y), y);
                sprite.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
            }
        }
       
    }
}