using UnityEngine;

namespace MaximovInk
{
    public class SavedPrefabBehaviour : MonoBehaviour
    {
        public ChunkManager.Chunk Chunk;
        public int ObjectId;

        protected int loadedData;
        
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
        }
    }
}