using UnityEngine;

namespace MaximovInk
{
    public class TrashGenerator : MonoBehaviour
    {
        //public int Min = 5,Max = 10;
        public float umnoj = 15;

        public float Range = 2;

        public Sprite[] sprites;
        
        
        private void Start()
        {
            
            Generate();
        }

        public void Generate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            int generate = (int) Random.Range(Range*umnoj*0.99f, Range*15);

            for (int i = 0; i < generate; i++)
            {
                var sprite = new GameObject();
                sprite.AddComponent<SpriteRenderer>().sprite = sprites[Random.Range(0,sprites.Length)];
                sprite.transform.SetParent(transform);
                float y = Random.Range(0, Range);
                sprite.transform.localPosition = new Vector3(Mathf.Clamp(Random.Range(-Range,Range), -(Range-y),Range-y), y);
                sprite.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.C))
            {
                Generate();
            }
        }
        
       
    }
}