using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class MapViewer : MonoBehaviour
    {


        public static MapViewer instance;

        public Color backgroundColor,playerColor,selectedColor;
        
        public int chunkSize = 120;
        public int rectSize = 64;
        public float zoom = 1;
        public float radarZoom = 1;
        public float minimapMaskSize = 200;
        
        public GameObject FullMap;
        public Transform Minimap;
        public RawImage SectorPrefab;
        public Transform SectorsParent;
        
        
        private Color[] clearColors;
        private Color[] selectColors;
        private float pixel;

        private List<InspectedChunk> Chunks = new List<InspectedChunk>();

        private Vector2 lastMousePos;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                clearColors = new Color[chunkSize*chunkSize];
                selectColors = new Color[chunkSize*chunkSize];
                for (int i = 0; i < clearColors.Length; i++)
                {
                    clearColors[i] = backgroundColor;
                    selectColors[i] = selectedColor;
                }
            }
        }

        public class InspectedChunk
        {
            public RawImage target;
            public int x, y;
            public Texture2D texture;
        }

        public Vector2Int playerPosInside { get; private set; }

        private float rectUnit;
        
        public void Init()
        {
            pixel =  chunkSize/ ((float)ChunkManager.Instance.chunkSize * ChunkManager.Instance.tileScale);
            rectUnit = (float)ChunkManager.Instance.chunkSize*ChunkManager.Instance.tileScale / rectSize;
        }

        public void InspectChunk(ChunkManager.ChunkSaveData data, int playerX = -1, int playerY = -1)
        {
            if(data == null)
                return;
            
            if (!Chunks.Any(n => n.x == data.X && n.y == data.Y))
            {
                InspectedChunk inspectedChunk = new InspectedChunk { x = data.X , y = data.Y };
                inspectedChunk.target = Instantiate(SectorPrefab, SectorsParent);
                inspectedChunk.target.transform.localPosition = new Vector3(data.X*rectSize, data.Y*rectSize);
                
                
                Chunks.Add(inspectedChunk);
                
            }
            var c = Chunks.First(n => n.x == data.X && n.y == data.Y);
            
            c.texture = ReadDataFromChunk(data,c.texture,playerX,playerY);

            c.target.texture = c.texture;  
            
        }

        private void Update()
        {
            playerPosInside = ChunkManager.Instance.PlayerInsideChunk();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FullMap.SetActive(!FullMap.activeSelf);

                if (FullMap.activeSelf)
                {
                    LoadMap();
                    ResetMap();
                }
            }

            if (!FullMap.activeSelf)
            {
                zoom = radarZoom;
                SectorsParent.SetParent(Minimap);
                InspectedChunk plCh = Chunks.FirstOrDefault(n =>
                    n.x == ChunkManager.Instance.playerChunk.SaveData.X && n.y == ChunkManager.Instance.playerChunk.SaveData.Y);
                if (plCh != null)
                {
                    SectorsParent.localPosition = 
                        -(plCh.target.transform.localPosition
                          
                          +(Vector3)(Vector2)playerPosInside/rectUnit
                          -new Vector3(ChunkManager.Instance.chunkSize,ChunkManager.Instance.chunkSize)* (minimapMaskSize/rectSize)
                          )
                        *zoom;
                    /*SectorsParent.localPosition = 
                        -(plCh.target.transform.localPosition
                          +(Vector3)(Vector2)playerPosInside/5
                          -new Vector3(ChunkManager.Instance.chunkSize,ChunkManager.Instance.chunkSize))
                        *zoom;*/
                }
            }
            else
            {
                SectorsParent.SetParent(FullMap.transform);
                if (Input.GetMouseButton(2))
                {
                    SectorsParent.transform.position -= (Vector3) lastMousePos - Input.mousePosition;
                }
            }
            
            zoom += Input.mouseScrollDelta.y * 0.1f;

            SectorsParent.localScale = new Vector3(zoom, zoom, 1);
            
            lastMousePos = Input.mousePosition;
        }

        public void ResetMap()
        {
            zoom = 1;
            SectorsParent.transform.position = new Vector3(Screen.width/2.0f,Screen.height/2.0f);
        }

        public void LateUpdate()
        {
            
            
            
            var chunks = ChunkManager.Instance._loadedChunks.Where(n => !n.IsFree);
            foreach (var chunk in chunks)
            {
                //InspectChunk(chunk.SaveData);
                if (chunk.SaveData.X == ChunkManager.Instance.playerChunk.SaveData.X &&
                    chunk.SaveData.Y == ChunkManager.Instance.playerChunk.SaveData.Y)
                {
                    InspectChunk(chunk.SaveData,playerPosInside.x,playerPosInside.y);
                }
                else
                {
                    InspectChunk(chunk.SaveData);
                }
            }
            
            //InspectChunk(ChunkManager.Instance.playerChunk,playerPosInside.x,playerPosInside.y);
            
          
           
        }

        public void LoadMap()
        {
            SaveManager.instance.CheckTempFolder();
            var files = Directory.EnumerateFiles(SaveManager.instance.GetTempPath()+ "/chunks", "*.chnk").ToList();
            Debug.Log(files.Count);
            foreach (var file in files)
            {
                using (var fs = new FileStream(file,FileMode.Open))
                {
                    InspectChunk(MessagePackSerializer.Deserialize<ChunkManager.ChunkSaveData>(fs));
                }
            }

        }



        public Texture2D ReadDataFromChunk(ChunkManager.ChunkSaveData data, Texture2D tex = null, int playerX = -1,
            int playerY = -1)
        {
            if (tex == null)
                tex = new Texture2D(chunkSize, chunkSize, TextureFormat.ARGB32, false) {filterMode = FilterMode.Point};

            //if(playerX != -1 && playerY != -1)
               // tex.SetPixels(selectColors);
           // else
                tex.SetPixels(clearColors);

            if (data.Objects != null)
                foreach (var obj in data.Objects)
                {
                    tex.SetPixel((int) (obj.PositionX * pixel), (int) (obj.PositionY * pixel),
                        ChunkManager.Instance.prefabs[obj.Prefab].mapColor);
                }

            if (playerX != -1 && playerY != -1)
                tex.SetPixel((int) (playerX * pixel), (int) (playerY * pixel), playerColor);
            tex.Apply();
            return tex;
        }


    }
}