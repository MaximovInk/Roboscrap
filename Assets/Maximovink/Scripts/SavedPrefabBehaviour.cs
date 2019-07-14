
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class SavedPrefabBehaviour : MonoBehaviour
    {
        public ChunkManager.Chunk Chunk;
        public int ObjectId;

        protected int loadedData;
        
        private List<Entity> entites = new List<Entity>();

        private void Awake()
        {
            entites = GetComponentsInChildren<Entity>().ToList();
        }

        public void Load()
        {
            OnLoad(Chunk.objects[ObjectId].data);
        }

        public void Save()
        {
            Chunk.objects[ObjectId].data = OnSave();
        }

        protected virtual int OnSave()
        {
            return loadedData;
        }

        protected virtual void OnLoad(int data)
        {
            loadedData = data;

            foreach (var entity in entites)
            {
                entity.Sort();
            }
        }
    }
}