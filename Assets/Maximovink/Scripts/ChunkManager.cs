using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MessagePack;
using UnityEngine;

namespace MaximovInk
{
    public class ChunkManager : MonoBehaviour
    {
        public static ChunkManager instance;
        
        public PrefabRandomizeGroup[] RandomGroups;
        
        public int ChunkSize = 32;

        private const int _chunkVisibality = 5;

        public int worldSize = 2000;

        public float iterationsDelay = 0.1f;
        private float timer;

        public float NoiseScale = 1;
        public int TileScale = 1;
        
        public Transform target;
        private Vector3 lastPos;
        private Vector2Int lastChunk;

        private readonly List<Chunk> _loadedChunks = new List<Chunk>();

        public PrefabForPool[] prefabs;
        
        private float offset => _chunkVisibality / 2 * ChunkSize * TileScale;

        public Chunk playerChunk;
        private Vector2Int chunk;
        
        public Vector2Int WorldToChunk(Vector2 pos) 
        {
            return Vector2Int.FloorToInt(new Vector2(pos.x/ChunkSize/TileScale+_chunkVisibality/2, pos.y/ChunkSize/TileScale+_chunkVisibality/2));
        }

        public Vector2 ChunkToWorld(Vector2Int chunk)
        {
            return new Vector2(chunk.x*ChunkSize*TileScale-_chunkVisibality/2,chunk.y*ChunkSize*TileScale-_chunkVisibality/2);
        }

        public bool LoadingComplete => sectors/9 > 0.9f;
        private float sectors;
        
        private void Update()
        {
            timer += Time.deltaTime;
            
            chunk = WorldToChunk(target.position);
            var pc = _loadedChunks.FirstOrDefault(n => chunk.x == n.x && chunk.y == n.y);
            if (pc != null)
                playerChunk = pc;
            
            if (iterationsDelay < timer)
            {
                timer = 0;
                if (target.transform.position != lastPos)
                {
                    lastPos = target.transform.position;

                    var c = WorldToChunk(lastPos);
                    if (lastChunk != c)
                    {
                        lastChunk = c;
                        UpdateChunkPos();
                    }
                }
            }

        
        }

        private void Awake()
        {
            
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                timer = iterationsDelay++;
                
            }
        }

        private void Start()
        {
            InitChunks();
        }

        private void InitChunks()
        {
            for (var x = 0; x < _chunkVisibality; x++)
            {
                for (var y = 0; y < _chunkVisibality; y++)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    _loadedChunks.Add(new Chunk {instance = go.transform, x = 0, y = 0,isFree = false, objects = generateRandom(0,0)});
                    var ch = _loadedChunks.Last();
                    for (var i = 0; i < ch.objects.Length; i++)
                    {
                        var freePrefab = prefabs[ch.objects[i].Prefab].instantiated
                            .FirstOrDefault(n => n.gameObject.activeSelf == false && n.Chunk != ch);

                        if (freePrefab != null)
                        {
                            freePrefab.gameObject.SetActive(true);
                            freePrefab.transform.SetParent(ch.instance);
                            freePrefab.transform.localPosition =
                                new Vector2(ch.objects[i].PositionX, ch.objects[i].PositionY);
                            freePrefab.Chunk = ch;
                            freePrefab.ObjectId = i;
                            ch.objects[i].Target = freePrefab;
                        }
                        else
                        {
                            var newobj = Instantiate(
                                prefabs[ch.objects[i].Prefab].prefab,
                                (Vector2) ch.instance.position +
                                new Vector2(ch.objects[i].PositionX, ch.objects[i].PositionY),
                                Quaternion.identity,
                                ch.instance);

                            prefabs[ch.objects[i].Prefab].instantiated =
                                prefabs[ch.objects[i].Prefab].instantiated.Add(newobj);
                            newobj.Chunk = ch;
                            newobj.ObjectId = i;
                            ch.objects[i].Target = newobj;
                        }

                        ch.objects[i].Target.OnLoad(ch.objects[i].Data);

                    }
                    go.name = "chunk";
                }
            }

        }
        
        public void UpdateChunkPos()
        {
            foreach (var t in _loadedChunks)
            {
                t.isFree = IsFree(t, chunk);

                t.instance.gameObject.name = t.isFree ? "chunk" : "chunk [in use]";
            }
            
            TargetFreeChunkTo(chunk.x,chunk.y);
            TargetFreeChunkTo(chunk.x+1,chunk.y);
            TargetFreeChunkTo(chunk.x-1,chunk.y);
            TargetFreeChunkTo(chunk.x,chunk.y+1);
            TargetFreeChunkTo(chunk.x,chunk.y-1);
            TargetFreeChunkTo(chunk.x+1,chunk.y+1);
            TargetFreeChunkTo(chunk.x-1,chunk.y-1);
            TargetFreeChunkTo(chunk.x+1,chunk.y-1);
            TargetFreeChunkTo(chunk.x-1,chunk.y+1);

            
        }

        public void TargetFreeChunkTo(int x, int y)
        {
            if (_loadedChunks.Any(n => n.x == x && n.y == y))
                return;

            var free = _loadedChunks.FirstOrDefault(n => n.isFree);

            Save(free);
            
            for (var i = 0; i < free.instance.childCount; i++)
            {
                free.instance.GetChild(i).gameObject.SetActive(false);
            }
            LoadDataTo(free, x, y);
            free.instance.localPosition =
                new Vector3(x * ChunkSize * TileScale - offset, y * ChunkSize * TileScale - offset);

            for (var i = 0; i < free.objects.Length; i++)
            {
                var freePrefab = prefabs[free.objects[i].Prefab].instantiated
                    .FirstOrDefault(n => n.gameObject.activeSelf == false && n.Chunk != free);

                if (freePrefab != null)
                {
                    freePrefab.gameObject.SetActive(true);
                    freePrefab.transform.SetParent(free.instance);
                    freePrefab.transform.localPosition =
                        new Vector2(free.objects[i].PositionX, free.objects[i].PositionY);
                    freePrefab.Chunk = free;
                    freePrefab.ObjectId = i;
                    free.objects[i].Target = freePrefab;
                }
                else
                {
                    var newobj = Instantiate(
                        prefabs[free.objects[i].Prefab].prefab,
                        (Vector2) free.instance.position +
                        new Vector2(free.objects[i].PositionX, free.objects[i].PositionY),
                        Quaternion.identity,
                        free.instance);

                    prefabs[free.objects[i].Prefab].instantiated =
                        prefabs[free.objects[i].Prefab].instantiated.Add(newobj);
                    newobj.Chunk = free;
                    newobj.ObjectId = i;
                    free.objects[i].Target = newobj;
                }

                free.objects[i].Target.OnLoad(free.objects[i].Data);

            }
            MapViewer.instance.InspectChunk(free);
            free.isFree = false;
            if (!LoadingComplete)
                sectors++;
        }

        public DataObject[] generateRandom(int x, int y)
        {
            
            var objs = new DataObject[0];
            for (var i = 0; i < ChunkSize; i++)
            {
                for (var j = 0; j < ChunkSize; j++)
                {
                    var obj = Mathf.PerlinNoise((x * (float) ChunkSize + i) / NoiseScale + SaveManager.instance.saveData.seed,
                        (y * (float) ChunkSize + j) / NoiseScale + SaveManager.instance.saveData.seed);
                        

                    foreach (var group in RandomGroups)
                    {
                        if ((!(obj > group.thresoult) || !group.greatherThan) &&
                            (!(obj < group.thresoult) || group.greatherThan)) continue;
                           
                        var prefabIndex = (byte) group.index[Extenshions.random.Next(0, group.index.Length)];

                        
                        
                        objs = objs.Add(new DataObject
                        {
                            Data = new object[0],
                            Prefab = prefabIndex,
                            PositionX = 
                                i * TileScale + Extenshions.random.GetRandomFloat(-TileScale / 2.0f, TileScale / 2.0f),
                            PositionY = 
                                j * TileScale + Extenshions.random.GetRandomFloat(-TileScale / 2.0f, TileScale / 2.0f)
                        });
                        break;
                    }
                }
            }

            return objs;
        }

        private void LoadDataTo(Chunk chunk,int x , int y)
        {
            var path = SaveManager.instance.GetTempPath() + "/chunks/";
            SaveManager.instance.CheckTempFolder();
           
            if(File.Exists(path+ x + "_" + y+".chnk"))
            {

                using (var fs = new FileStream(path+ x + "_" + y+".chnk",FileMode.Open))
                {
                    chunk.ReadData(MessagePackSerializer.Deserialize<Chunk>(fs));
                }
                return;
            }

            var c = new Chunk {x = x, y = y, objects = generateRandom(x, y)};


            chunk.ReadData(c);
            Save(chunk);
        }

        public void SaveLoadedChunks()
        {
            foreach (var chunk in _loadedChunks)
            {
                Save(chunk);
            }
        }

        private void Save(Chunk chunk)
        {
            var path = SaveManager.instance.GetTempPath()+"/chunks/" ;
            SaveManager.instance.CheckTempFolder();

            if(chunk.objects != null)
                chunk.objects = chunk.objects.Where(n => n.Target.gameObject.activeSelf).ToArray();
            
            for (var i = 0; i < chunk.objects.Length; i++)
            {
                if(chunk.objects[i] != null &&  chunk.objects[i].Data != null && chunk.objects[i].Target != null)
                    chunk.objects[i].Data = chunk.objects[i].Target.OnSave();
            }
            
            using (var fs = new FileStream(path+ chunk.x + "_" + chunk.y+".chnk",FileMode.OpenOrCreate))
            {
                MessagePackSerializer.Serialize(fs, chunk);
            }
        }

        private static bool IsFree(Chunk chunk , Vector2Int center)
        {
            return !(
                (chunk.x == center.x || chunk.x+1 == center.x || chunk.x-1 == center.x) &&
                (chunk.y == center.y || chunk.y+1 == center.y || chunk.y-1 == center.y)
            );
        }

        public Vector2Int WorldToPosInsideChunk(Vector2 pos)
        {
            var chunk_pos = WorldToChunk(pos);
            var chunk = _loadedChunks.FirstOrDefault(n => n.x == chunk_pos.x && n.y == chunk_pos.y);
            if (chunk != null)
            {
                var inside = pos - (Vector2)chunk.instance.position;
                return Vector2Int.FloorToInt(new Vector2(inside.x, inside.y));
                
            }
            return Vector2Int.zero;
        }

        [MessagePackObject]
        public class Chunk
        {
            [IgnoreMember]
            [NonSerialized]
            public Transform instance;

            [Key(0)] public   DataObject[] objects { get; set; }

            [Key(1)] public  int x { get; set; }

            [Key(2)] public int y { get; set; }

            [IgnoreMember] public bool isFree { get; set; }

            public void ReadData(Chunk chunk)
            {
                objects = chunk.objects;
                x = chunk.x;
                y = chunk.y;
            }
            
            
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
        }
        
        [Serializable]
        public class PrefabForPool
        {
            public SavedPrefabBehaviour prefab;
            [HideInInspector]
            public SavedPrefabBehaviour[] instantiated;

            public Color mapColor = Color.white;
        }
        
    }
    
 
}