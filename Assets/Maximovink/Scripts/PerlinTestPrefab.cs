using UnityEngine;

namespace MaximovInk

{
    public class PerlinTestPrefab : SavedPrefabBehaviour
    {
        private SpriteRenderer spriteRenderer
        {
            get
            {
                if (sp == null) sp = GetComponent<SpriteRenderer>();
                return sp;
            }
        }

        private SpriteRenderer sp;
        
        protected override void OnLoad(int data)
        {
            base.OnLoad(data);
            var c = Extenshions.ToSingle((uint)data);
            spriteRenderer.color = new Color(c,c,c);
            transform.localScale = Vector3.one*ChunkManager.instance.TileScale;
        }
    }
}