
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class SavedPrefabBehaviour : MonoBehaviour
    {
        //public ChunkManager.Chunk Chunk;
        protected object[] loadedData;
        
        private List<Entity> entites = new List<Entity>();
        
        private void Awake()
        {
            entites = GetComponentsInChildren<Entity>().ToList();
        }

        public virtual object[] OnSave()
        {
            return loadedData;
        }

        public virtual void OnLoad(object[] data)
        {
            loadedData = data;

            foreach (var entity in entites)
            {
                entity.Sort();
            }
        }
    }
}