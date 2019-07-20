using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace MaximovInk
{
    public class ChunkManager : MonoBehaviour
    {
        public static ChunkManager Instance;
        
        public PrefabRandomizeGroup[] tilesGroups;

        public PrefabRandomizeGroup[] structuresGroups;
        

        public int chunkSize = 8;

        public int structsInChunksMax = 4;

        private const int ChunkVisibality = 4;

        public float iterationsDelay = 0.1f;
        private float _timer;
        [Header("Structures")]
        public float structuresScale = 5.6789f;
        [Header("Alone objects")]
        public float noiseScale = 2.34567f;
        public int tileScale = 15;

        public List<Chunk> _loadedChunks { get; } = new List<Chunk>();

        public PrefabForPool[] prefabs;
        
        private float Offset => ChunkVisibality / 2 * chunkSize * tileScale;
        
        public bool LoadingComplete => _sectors/9 > 0.9f;

        public Transform target;
        
        private float _sectors;

        private Vector2Int _centerChunk;

        public Chunk playerChunk;

        private Vector2Int WorldToChunk(Vector2 pos)
        {
            return Vector2Int.FloorToInt(new Vector2(pos.x/chunkSize/tileScale, pos.y/chunkSize/tileScale));
        }

        private Vector2Int WorldToChunkForLoadedChunks(Vector2 pos) 
        {
            return Vector2Int.FloorToInt(new Vector2(pos.x/chunkSize/tileScale+ChunkVisibality/2.0f, pos.y/chunkSize/tileScale+ChunkVisibality/2.0f));
        }

        public Vector2 ChunkToWorld(Vector2Int chunk)
        {
            return new Vector2(chunk.x*chunkSize*tileScale-ChunkVisibality/2,chunk.y*chunkSize*tileScale-ChunkVisibality/2);
        }


        
        private void Update()
        {
            _timer += Time.deltaTime;
            _centerChunk = WorldToChunkForLoadedChunks(target.position);
            
            var pc = _loadedChunks.FirstOrDefault(n => n.SaveData != null && _centerChunk.x == n.SaveData.X && _centerChunk.y == n.SaveData.Y);
            if (pc != null)
                playerChunk = pc;
            
            if (iterationsDelay < _timer)
            {
                UpdateChunksPosition();
                _timer = 0;
            }

        
        }

        private void Awake()
        {
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                _timer = iterationsDelay+1;
                foreach (var tileGroup in tilesGroups)
                {
                    foreach (var prefab in tileGroup.index)
                    {
                        if (prefabs[prefab].getColorFromGroup)
                        {
                            prefabs[prefab].mapColor = tileGroup.mapColor;
                        }
                    }
                }
                foreach (var structuresGroup in structuresGroups)
                {
                    foreach (var prefab in structuresGroup.index)
                    {
                        if (prefabs[prefab].getColorFromGroup)
                        {
                            prefabs[prefab].mapColor = structuresGroup.mapColor;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            InitPool();
        }

        public void InitPool()
        {
            
            _centerChunk = WorldToChunkForLoadedChunks(target.position);
            
            for (var x = 0; x < ChunkVisibality; x++)
            {
                for (var y = 0; y < ChunkVisibality; y++)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    _loadedChunks.Add(new Chunk {Instance = go.transform, IsFree = true, SaveData = new ChunkSaveData{X = 0, Y = 0 }});                   
                    go.name = "chunk";
                }
            }
              
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y);
        }

        private void InstanceObjectsForChunk(Chunk ch)
        {
            for (var i = 0; i < ch.SaveData.Objects.Length; i++)
            {
                var freePrefab = prefabs[ch.SaveData.Objects[i].Prefab].instantiated
                    .FirstOrDefault(n => n.gameObject.activeSelf == false && n.Chunk != ch);

                if (freePrefab != null)
                {
                    freePrefab.gameObject.SetActive(true);
                    freePrefab.transform.SetParent(ch.Instance);
                    freePrefab.transform.localPosition =
                        new Vector2(ch.SaveData.Objects[i].PositionX, ch.SaveData.Objects[i].PositionY);
                    freePrefab.Chunk = ch;
                    ch.SaveData.Objects[i].Target = freePrefab;
                }
                else
                {
                    var newobj = Instantiate(
                        prefabs[ch.SaveData.Objects[i].Prefab].prefab,
                        (Vector2) ch.Instance.position +
                        new Vector2(ch.SaveData.Objects[i].PositionX, ch.SaveData.Objects[i].PositionY),
                        Quaternion.identity,
                        ch.Instance);

                    prefabs[ch.SaveData.Objects[i].Prefab].instantiated =
                        prefabs[ch.SaveData.Objects[i].Prefab].instantiated.Add(newobj);
                    newobj.Chunk = ch;
                    ch.SaveData.Objects[i].Target = newobj;
                }

                ch.SaveData.Objects[i].Target.OnLoad(ch.SaveData.Objects[i].Data);

            }
        }

        public ChunkSaveData LoadChunkData(int x , int y)
        {
            var path = SaveManager.instance.GetTempPath() + "/chunks/";
            SaveManager.instance.CheckTempFolder();

            if (!File.Exists(path + x + "_" + y + ".chnk"))
                return NewData(x, y);
            
            using (var fs = new FileStream(path+ x + "_" + y+".chnk",FileMode.Open))
            {
                return MessagePackSerializer.Deserialize<ChunkSaveData>(fs);
            }

        }
        
        private static bool IsFree(Chunk chunk , Vector2Int center)
        {
            return !(
                (chunk.SaveData.X == center.x || chunk.SaveData.X+1 == center.x || chunk.SaveData.X-1 == center.x) &&
                (chunk.SaveData.Y == center.y || chunk.SaveData.Y+1 == center.y || chunk.SaveData.Y-1 == center.y)
            );
        }

        
        public void UpdateChunksPosition()
        {
            foreach (var t in _loadedChunks)
            {
                t.IsFree = IsFree(t, _centerChunk);

                t.Instance.gameObject.name = t.IsFree ? "chunk" : "chunk [in use]";
            }
            
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x,_centerChunk.y);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x+1,_centerChunk.y);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y-1);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y+1);
            MoveFreeChunkTo(_centerChunk.x-1,_centerChunk.y);

            
        }

        public void MoveFreeChunkTo(int x, int y)
        {
            if (_loadedChunks.Any(n => n.SaveData.X == x && n.SaveData.Y == y))
                return;
            
            var free = _loadedChunks.FirstOrDefault(n => n.IsFree);
            if(free.SaveData != null && LoadingComplete)
                SaveChunkData(free.SaveData);
            
            for (var i = 0; i < free.Instance.childCount; i++)
            {
                free.Instance.GetChild(i).gameObject.SetActive(false);
            }

            free.SaveData = LoadChunkData(x, y);
            
            free.Instance.localPosition = new Vector3(x * chunkSize * tileScale - Offset, y * chunkSize * tileScale - Offset);

            InstanceObjectsForChunk(free);
            //MapViewer.instance.InspectChunk(free.SaveData);
            free.IsFree = false;

            if(!LoadingComplete)
                _sectors++;
        }

        public ChunkSaveData NewData(int x, int y)
        {
            var objs = new DataObject[0];

            var structure = Mathf.PerlinNoise(
                (x * (float) chunkSize ) / structuresScale + SaveManager.instance.saveData.seed,
                (y * (float) chunkSize ) / structuresScale + SaveManager.instance.saveData.seed);

            var structs = structuresGroups.Where(n =>
                n.thresoult < structure && n.greatherThan ||
                n.thresoult > structure && !n.greatherThan).ToArray();

            if (structs.Length > 0)
            {
                var group = structs[UnityEngine.Random.Range(0, structs.Length)];
                var prefabIndex = (byte) group.index[UnityEngine.Random.Range(0, group.index.Length)];
                objs = objs.Add(new DataObject
                {
                    Data = new object[0],
                    Prefab = prefabIndex,
                    PositionX =
                        UnityEngine.Random.Range(-chunkSize*tileScale / 2.0f, chunkSize*tileScale / 2.0f),
                    PositionY =
                        UnityEngine.Random.Range(-chunkSize*tileScale / 2.0f, chunkSize*tileScale / 2.0f)
                });
            }



            for (var i = 0; i < chunkSize; i++)
            {
                for (var j = 0; j < chunkSize; j++)
                {
                    var obj = Mathf.PerlinNoise(
                        (x * (float) chunkSize * tileScale + i) / noiseScale + SaveManager.instance.saveData.seed,
                        (y * (float) chunkSize * tileScale + j) / noiseScale + SaveManager.instance.saveData.seed);

                    var prefabs = tilesGroups.Where(n =>
                        n.thresoult < obj && n.greatherThan ||
                        n.thresoult > obj && !n.greatherThan).ToArray();

                    if (prefabs.Length > 0)
                    {
                        var group = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
                        var prefabIndex = (byte) group.index[UnityEngine.Random.Range(0, group.index.Length)];
                        objs = objs.Add(new DataObject
                        {
                            Data = new object[0],
                            Prefab = prefabIndex,
                            PositionX =
                                i * tileScale + Extenshions.random.GetRandomFloat(-tileScale / 2.0f, tileScale / 2.0f),
                            PositionY =
                                j * tileScale + Extenshions.random.GetRandomFloat(-tileScale / 2.0f, tileScale / 2.0f)
                        });

                    }

                }
            }

            return new ChunkSaveData {X = x, Y = y, Objects = objs};

        }

        public void SaveChunkData(ChunkSaveData chunkSaveData)
        {
            
            
            var path = SaveManager.instance.GetTempPath()+"/chunks/" ;
            SaveManager.instance.CheckTempFolder();

            if(chunkSaveData.Objects == null)
                chunkSaveData.Objects = new DataObject[0];
            
            chunkSaveData.Objects = chunkSaveData.Objects.Where(n => n.Target.gameObject.activeSelf).ToArray();
            
            for (var i = 0; i < chunkSaveData.Objects.Length; i++)
            {
                if(chunkSaveData.Objects[i] != null &&  chunkSaveData.Objects[i].Data != null && chunkSaveData.Objects[i].Target != null)
                    chunkSaveData.Objects[i].Data = chunkSaveData.Objects[i].Target.OnSave();
            }
            
            using (var fs = new FileStream(path+ chunkSaveData.X + "_" + chunkSaveData.Y+".chnk",FileMode.OpenOrCreate))
            {
                MessagePackSerializer.Serialize(fs, chunkSaveData);
            }
        }
        
        public void SaveLoadedChunks()
        {
            foreach (var chunk in _loadedChunks)
            {
                SaveChunkData(chunk.SaveData);
            }
        }

        public Vector2Int PlayerInsideChunk()
        {
            var inside = GameManager.Instance.player.transform.position - playerChunk.Instance.position;
            return Vector2Int.FloorToInt(new Vector2(inside.x, inside.y));
        }

        [MessagePackObject]
        public class ChunkSaveData
        {
            [Key(0)] public DataObject[] Objects { get; set; }

            [Key(1)] public  int X { get; set; }

            [Key(2)] public int Y { get; set; }
        }
        
        public class Chunk
        {
            public Transform Instance;

            public ChunkSaveData SaveData;

            public bool IsFree;

        }
        
        [MessagePackObject]      
        public class DataObject
        {
            [Key(0)] public float PositionX { get; set; }

            [Key(1)] public float PositionY { get; set; }

            [Key(2)] public object[] Data { get; set; }

            [Key(3)] public int Prefab { get; set; }

            [IgnoreMember] public SavedPrefabBehaviour Target { get; set; }
        }
        
        [Serializable]
        public class PrefabRandomizeGroup
        {
            public int[] index;
            [Range(0,1)]
            public float thresoult;
            public bool greatherThan = true;
            public Color mapColor = Color.white;
        }
        
        [Serializable]
        public class PrefabForPool
        {
            public SavedPrefabBehaviour prefab;
            [HideInInspector]
            public SavedPrefabBehaviour[] instantiated;

            public bool getColorFromGroup = true;
            public Color mapColor = Color.white;
        }
        
    }
    
 
}