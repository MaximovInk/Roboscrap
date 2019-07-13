using UnityEngine;

namespace MaximovInk
{
    public class SavedPrefabBehaviour : MonoBehaviour
    {
        public ChunkManager.Chunk Chunk;
        public int ObjectId;
        
        public void Load()
        {
            OnLoad(Chunk.objects[ObjectId].data);
        }

        public void Save()
        {
            Chunk.objects[ObjectId].data = OnSave();
        }

        protected virtual uint OnSave()
        {
            return 0;
        }

        protected virtual void OnLoad(uint data)
        {
            
        }
    }
}