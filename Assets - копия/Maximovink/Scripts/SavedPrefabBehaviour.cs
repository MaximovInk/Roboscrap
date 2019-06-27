using UnityEngine;

namespace DefaultNamespace
{
    public class SavedPrefabBehaviour : MonoBehaviour
    {
        public virtual void OnLoaded(uint data)
        {
            
        }

        public virtual uint OnSave()
        {
            return 0;
        }
    }
}