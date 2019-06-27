using UnityEngine;

namespace MaximovInk
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