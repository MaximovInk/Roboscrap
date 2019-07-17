using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaximovInk;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class MapViewer : MonoBehaviour
    {
        
        
        public static MapViewer instance;

        public RawImage Renderer;

        public int mapSize = 64;
        private Texture2D texture;
        private float chunkSize;
        private float pixel;
        public GameObject FullMap;
        public Transform chunksParent;
        public RawImage chunkPrefab;
        public float zoom = 1;
        public Color backgroundColor = Color.white;
        public float rectSize = 64;
        
        public class InspectedChunk
        {
            public RawImage target;
            public Texture2D texture;
            public int x,y;
        }

        public Color playerChunk;

        public Color player;

        private Color[] clearColors;
        
        private Vector2Int pl;

        private Vector3 lastPlayerPos;
        
        public List<InspectedChunk> Chunks = new List<InspectedChunk>();
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                clearColors = new Color[mapSize*mapSize];
                for (int i = 0; i < clearColors.Length; i++)
                {
                    clearColors[i] = backgroundColor;
                }
            }
        }

        private Vector2 lastMousePos;

        public void InspectChunk(ChunkManager.Chunk chunk)
        {
            
            if (!Chunks.Any(n => n.x == chunk.x && n.y == chunk.y))
            {
                InspectedChunk inspectedChunk = new InspectedChunk{ x = chunk.x , y = chunk.y };
                inspectedChunk.target = Instantiate(chunkPrefab, chunksParent);
                inspectedChunk.target.transform.localPosition = new Vector3(chunk.x*rectSize, chunk.y*rectSize);
                
                Chunks.Add(inspectedChunk);
                
            }
            var c = Chunks.First(n => n.x == chunk.x && n.y == chunk.y);

 
            c.texture = GetDataFromChunk(chunk,c.texture);


            c.target.texture = c.texture;

        }

        private InspectedChunk lastSelectedChunk;

        public void Init()
        {
            pixel =  mapSize/ ((float)ChunkManager.instance.ChunkSize * ChunkManager.instance.TileScale);
            //TODO : FIX BUG AND MAKE START ZONE FOR PLAYER!
        }

        public void ResetMap()
        {
            zoom = 1;
            chunksParent.transform.position = new Vector3(Screen.width/2,Screen.height/2);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FullMap.SetActive(!FullMap.activeSelf);
                Load();
            }

            if (FullMap.activeSelf)
            {
                if (Input.GetMouseButton(2))
                {
                    chunksParent.transform.position -= (Vector3) lastMousePos - Input.mousePosition;

                }
                
                var pc = ChunkManager.instance.playerChunk;
                for (var i = 0; i < Chunks.Count; i++)
                {

                    if (Chunks[i].x == pc.x && Chunks[i].y == pc.y)
                    {
                        if (lastSelectedChunk != null && (lastSelectedChunk.x != Chunks[i].x || lastSelectedChunk.y != Chunks[i].y))
                        {
                            lastSelectedChunk.target.texture = GetDataFromChunk(ChunkManager.instance.playerChunk,lastSelectedChunk.texture);
                        }

                        Chunks[i].target.color = playerChunk;
                        Chunks[i].target.texture = GetDataFromChunk(ChunkManager.instance.playerChunk,Chunks[i].texture, pl.x,pl.y );
                        lastSelectedChunk = Chunks[i];
                    }
                    else
                    {
                        Chunks[i].target.color = Color.white;
                    }
                }

                zoom += Input.mouseScrollDelta.y * 0.1f;

                chunksParent.localScale = new Vector3(zoom, zoom, 1);

                
            }
            
            if (lastPlayerPos != GameManager.Instance.player.transform.position)
            {
                pl = ChunkManager.instance.WorldToPosInsideChunk(GameManager.Instance.player.transform.position);              

                UpdateRadar(ChunkManager.instance.playerChunk, pl.x, pl.y);
                
                
            }

            lastMousePos = Input.mousePosition;
        }

        private void Start()
        {
            texture = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            Renderer.texture = texture;
        }

        public void Load()
        {
            Debug.Log("load");
            SaveManager.instance.CheckTempFolder();

            var files = Directory.EnumerateFiles(SaveManager.instance.GetTempPath()+ "/chunks", "*.chnk").ToList();
            Debug.Log(files.Count);
            foreach (var file in files)
            {
                var chunk = new ChunkManager.Chunk();
                using (var fs = new FileStream(file,FileMode.Open))
                {
                    chunk.ReadData(MessagePackSerializer.Deserialize<ChunkManager.Chunk>(fs));
                }
                InspectChunk(chunk);
            }
 
        }

        public Texture2D GetDataFromChunk(ChunkManager.Chunk chunk, Texture2D t = null,int x = -1 , int y = -1)
        {
            if(t == null)
                t = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false)
                {
                    filterMode = FilterMode.Point
                };
            
            t.SetPixels(clearColors);
            
            foreach (var obj in chunk.objects)
            {
                t.SetPixel((int)(obj.PositionX*pixel),(int)(obj.PositionY*pixel) , ChunkManager.instance.prefabs[obj.Prefab].mapColor);
            }
            
            if(x != -1 && y != -1)
                t.SetPixel((int)(x*pixel),(int)(y*pixel), player);

            t.Apply();
            return t;
        }

        public void UpdateRadar(ChunkManager.Chunk chunk, int x , int y)
        {
            if (chunk == null)
                return;

            texture.SetPixels(clearColors);
            
            foreach (var obj in chunk.objects)
            {
                texture.SetPixel((int)(obj.PositionX*pixel),(int)(obj.PositionY*pixel) , ChunkManager.instance.prefabs[obj.Prefab].mapColor);
            }
            
            texture.SetPixel((int)(x*pixel),(int)(y*pixel), player);

            texture.Apply();
        }
    }
}