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

      public override void OnLoad(object[] data)
      {
          base.OnLoad(data);
          var c = (float) data[0];
          spriteRenderer.color = new Color(c,c,c);
          transform.localScale = Vector3.one
          *ChunkManager.Instance.tileScale;
      }
    }
}