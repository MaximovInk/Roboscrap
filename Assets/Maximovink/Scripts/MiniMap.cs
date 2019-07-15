using MaximovInk;
using UnityEngine;

namespace MaximovInk
{
    public class MiniMap : MonoBehaviour
    {
        public static MiniMap instance;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
        }

        public void GenerateMap()
        {
            
        }
    }
}