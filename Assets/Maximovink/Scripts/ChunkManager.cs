using System;
using System.Collections;
using System.Linq;
using Boo.Lang;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MaximovInk
{
    public class ChunkManager : MonoBehaviour
    {
        public static ChunkManager instance;
        
        public int TrashPerChunkMax = 10;
        public int TrashPerChunkMin = 0;
        
        public PrefabRandomizeGroup[] RandomGroups;
        
        public int ChunkSize = 32;

        private const int _chunkVisibality = 4;

        public int worldSize = 2000;

        public float iterationsDelay = 0.1f;
        private float timer;

        public float NoiseScale = 1;
        public int TileScale = 1;
        
        public Transform target;
        private Vector3 lastPos;
        private Vector2Int lastChunk;

        private System.Collections.Generic.List<LoadedChunk> _loadedChunks;

        public PrefabForPool[] prefabs;

        private const string path = "saves/null/map.json";
        
        private float offset => _chunkVisibality / 2 * ChunkSize * TileScale;

        private Map map;

        public bool isLoaded = false;
        
        public Chunk GetChunkData(int x, int y)
        {
           return map.chunks[x + map.chunks.GetLength(0) / 2, y + map.chunks.GetLength(1) / 2];
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
                DontDestroyOnLoad(instance);
            }
        }

        private void Start()
        {
            map = new Map {seed = Random.Range(0, 10000), chunks = new Chunk[worldSize, worldSize]};

            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < worldSize; y++)
                {
                    map.chunks[x, y] = new Chunk();

                    for (int i = 0; i < ChunkSize; i++)
                    {
                        for (int j = 0; j < ChunkSize; j++)
                        {
                            
                            float obj = Mathf.PerlinNoise((x * (float) ChunkSize + i) / NoiseScale + map.seed,
                                (y * (float) ChunkSize + j) / NoiseScale + map.seed);

                            for (int k = 0; k < RandomGroups.Length; k++)
                            {
                                if ((obj > RandomGroups[k].thresoult && RandomGroups[k].greatherThan) ||
                                    (obj < RandomGroups[k].thresoult && !RandomGroups[k].greatherThan))
                                {

                                    var prefabIndex =  (byte)RandomGroups[k].index[Random.Range(0, RandomGroups[k].index.Length)];

                                    //if(!TrashNearInChunk(i,j,map.chunks[x,y]))
                                    map.chunks[x, y].objects = map.chunks[x, y].objects.Add(new DataObject
                                        {data = -1, prefab = prefabIndex, position = new Vector2(i+Random.Range(TileScale/-2,TileScale/2), j+Random.Range(TileScale/-2,TileScale/2))});
                                    break;

                                }
                            }
                            
                        }
                    }
                }
            }

            _loadedChunks = new System.Collections.Generic.List<LoadedChunk>(_chunkVisibality ^ 2);

            for (int x = 0; x < _chunkVisibality; x++)
            {
                for (int y = 0; y < _chunkVisibality; y++)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);


                    go.transform.localPosition = new Vector3(x * ChunkSize * TileScale - offset,
                        y * ChunkSize * TileScale - offset);

                    _loadedChunks.Add(new LoadedChunk {target = go.transform, x = -1, y = -1});
                    go.name = "chunk";

                }
            }
            UpdateChunksPos();
            isLoaded = true;
        }

        private void UpdateChunksPos()
        {
            var chunk = WorldToChunk(target.position);
            for (int i = 0; i < _loadedChunks.Count; i++)
            {
                _loadedChunks[i].isFree = IsFree(_loadedChunks[i], chunk);
                
                
                _loadedChunks[i].target.gameObject.name = _loadedChunks[i].isFree ? "chunk" : "chunk [in use]";
                
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
            
        }
        
        private void TargetFreeChunkTo(int x , int y)
        {
            if (!ChunkIsLoaded(x, y))
            {
                var from = _loadedChunks.FirstOrDefault(n => n.isFree);

                if(from == null)
                    return;
                
                from.target.localPosition = new Vector3(x * ChunkSize*TileScale - offset, y * ChunkSize*TileScale - offset);
                from.x = x;
                from.y = y;
                from.isFree = false;
                for (int i = 0; i < from.target.childCount; i++)
                {
                    //Destroy(from.target.GetChild(i).gameObject);
                    from.target.GetChild(i).gameObject.SetActive(false);
                }

                var chunk = GetChunkData(x, y);
                for (int i = 0; i < chunk.objects.Length; i++)
                {

                    var freePrefab = prefabs[chunk.objects[i].prefab].instantiated.FirstOrDefault(n => n.gameObject.activeSelf == false);
                    
                    if (freePrefab != null)
                    {
                        freePrefab.Save();
                        freePrefab.gameObject.SetActive(true);
                        freePrefab.transform.SetParent(from.target);
                        freePrefab.transform.localPosition = chunk.objects[i].position*TileScale;
                        freePrefab.Chunk = chunk;
                        freePrefab.ObjectId = i;
                        freePrefab.Load();
                    }
                    else
                    {
                        var newobj = Instantiate(
                            prefabs[chunk.objects[i].prefab].prefab ,
                            (Vector2)from.target.position + chunk.objects[i].position*TileScale,
                            Quaternion.identity,
                            from.target);

                        prefabs[chunk.objects[i].prefab].instantiated = prefabs[chunk.objects[i].prefab].instantiated.Add(newobj);
                        newobj.Chunk = chunk;
                        newobj.ObjectId = i;
                        newobj.Load();
                    }

                }
            }
        }

        private bool ChunkIsLoaded(int x, int y)
        {
            return _loadedChunks.Exists(n => n.x == x && n.y == y);
        }

        private static bool IsFree(LoadedChunk chunk , Vector2Int center)
        {
            return !(
                (chunk.x == center.x || chunk.x+1 == center.x || chunk.x-1 == center.x) &&
                (chunk.y == center.y || chunk.y+1 == center.y || chunk.y-1 == center.y)
                );
        }

        public Vector2Int WorldToChunk(Vector2 pos) 
        {
            return Vector2Int.FloorToInt(new Vector2(pos.x/ChunkSize/TileScale+_chunkVisibality/2, pos.y/ChunkSize/TileScale+_chunkVisibality/2));
        }
        
        private void Update()
        {
            if(!isLoaded)
                return;
            
            timer += Time.deltaTime;

            if (iterationsDelay < timer)
            {
                timer = 0;
                if (target.transform.position != lastPos)
                {
                    lastPos = target.transform.position;

                    var chunk = WorldToChunk(lastPos);
                    if (lastChunk != chunk)
                    {
                        UpdateChunksPos();
                    }
                }
            }

        
        }

        public class LoadedChunk
        {
            public Transform target;
            public int x, y;
            public bool isFree;
        }

        public class Map
        {
            public float seed;
            public Chunk[,] chunks;
        }

        public class Chunk
        {
            public DataObject[] objects = new DataObject[0];
        }

        public struct DataObject
        {
            public int data;
            public byte prefab;
            public Vector2 position;
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
        }
        
    }
}