using MaximovInk;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class MiniMap : MonoBehaviour
    {
        public static MiniMap instance;

        public RawImage Renderer;

        public int mapSize = 64;
        private Texture2D texture;
        private float chunkSize;
        private float pixel;
        
        public Color player;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                
            }
        }

        public void Init()
        {
            pixel =  mapSize/ ((float)ChunkManager.instance.ChunkSize * ChunkManager.instance.TileScale);
        }

        private void Update()
        {
            var pl = ChunkManager.instance.WorldToPosInsideChunk(GameManager.Instance.player.transform.position);
            UpdateRadar(ChunkManager.instance.playerChunk,pl.x,pl.y);
        }

        private void Start()
        {
            texture = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            Renderer.texture = texture;
        }
        
        public void UpdateRadar(ChunkManager.Chunk chunk, int x , int y)
        {
            if (chunk == null)
                return;
            for (var i = 0; i < texture.width; i++)
            {
                for (var j = 0; j < texture.height; j++)
                {
                    texture.SetPixel(i,j,Color.black);
                }
            }
            
            foreach (var obj in chunk.objects)
            {
                texture.SetPixel((int)(obj.positionX*pixel),(int)(obj.positionY*pixel) , ChunkManager.instance.prefabs[obj.prefab].mapColor);
            }
            
            texture.SetPixel((int)(x*pixel),(int)(y*pixel), player);

            texture.Apply();
        }
    }
}