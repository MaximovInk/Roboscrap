using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MaximovInk.AI;
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

        private List<LoadedChunk> _loadedChunks;

        public PrefabForPool[] prefabs;

        public bool generateComplete;
        private bool isLoaded;

        public Chunk[,] GetChunks() => map.chunks;
        
        private const string path = "saves/null/map.json";
        
        private float offset => _chunkVisibality / 2 * ChunkSize * TileScale;

        public Map map { get; private set; }

        public float generationProgress = 0;
        
        public Chunk GetChunkData(int x, int y)
        {
           return map.chunks[x + map.chunks.GetLength(0) / 2, y + map.chunks.GetLength(1) / 2];
        }

        public bool UnitIsEmpty(int x, int y, int i, int j)
        {
            var chunk = map.chunks[x, y];
            return chunk.objects.
                       Cast<DataObject?>().
                       FirstOrDefault(
                           n => 
                               n?.position.x-0.5f > i &&
                                n?.position.x + 0.5f < i &&
                                n?.position.y-0.5f > j && n?.position.y+0.5f < j) != null;

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
            }
        }

        private void Start()
        {
            Thread thread = new Thread(GenertateTerrain);
            thread.Start();
        }

        private void GenertateTerrain()
        {
            var random = new System.Random();
            map = new Map {seed = random.GetRandomFloat(0, 10000), chunks = new Chunk[worldSize, worldSize]};
            
            
            
            for (var x = 0; x < worldSize; x++)
            {
                for (var y = 0; y < worldSize; y++)
                {
                    map.chunks[x, y] = new Chunk();
                    map.chunks[x,y].objects = new DataObject[0];
                    for (var i = 0; i < ChunkSize; i++)
                    {
                        for (var j = 0; j < ChunkSize; j++)
                        {
                            
                            var obj = Mathf.PerlinNoise((x * (float) ChunkSize + i) / NoiseScale + map.seed,
                                (y * (float) ChunkSize + j) / NoiseScale + map.seed);

                           
                            
                            for (var k = 0; k < RandomGroups.Length; k++)
                            {
                                if ((obj > RandomGroups[k].thresoult && RandomGroups[k].greatherThan) ||
                                    (obj < RandomGroups[k].thresoult && !RandomGroups[k].greatherThan))
                                {

                                    var prefabIndex =  (byte)RandomGroups[k].index[random.Next(0, RandomGroups[k].index.Length)];

                                    //if(!TrashNearInChunk(i,j,map.chunks[x,y]))
                                    map.chunks[x, y].objects = map.chunks[x, y].objects.Add(new DataObject
                                    {
                                        data = -1,
                                        prefab = prefabIndex,
                                        position = new Vector2(
                                            i*TileScale+random.GetRandomFloat(-TileScale / 2.0f, TileScale / 2.0f),
                                            j*TileScale+random.GetRandomFloat(-TileScale / 2.0f, TileScale / 2.0f))
                                    });
                                    break;

                                }
                            }

                           
                        }
                    }
                    generationProgress = (float)x/worldSize;
                    //GameManager.Instance.LoadingSlider.value = generationProgress;
                }
             
            }

            generateComplete = true;
            _loadedChunks = new List<LoadedChunk>(_chunkVisibality ^ 2);
            

        }

        public void OnEndGeneration()
        {
            for (var x = 0; x < _chunkVisibality; x++)
            {
                for (var y = 0; y < _chunkVisibality; y++)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);


                    /* go.transform.localPosition = new Vector3(x * ChunkSize * TileScale - offset,
                         y * ChunkSize * TileScale - offset);*/

                    _loadedChunks.Add(new LoadedChunk {target = go.transform, x = -1, y = -1,isFree = true});
                    go.name = "chunk";

                }
            }
            UpdateChunksPos();
            isLoaded = true;
            
        }

        private void UpdateChunksPos()
        {
            var chunk = WorldToChunk(target.position);
            for (var i = 0; i < _loadedChunks.Count; i++)
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
                for (var i = 0; i < from.target.childCount; i++)
                {
                    //from.target.GetChild(i).GetComponent<SavedPrefabBehaviour>().Save();
                    from.target.GetChild(i).gameObject.SetActive(false);
                }

                var chunk = GetChunkData(x, y);
                if (chunk.objects != null)
                {
                    for (var i = 0; i < chunk.objects.Length; i++)
                    {

                        var freePrefab = prefabs[chunk.objects[i].prefab].instantiated
                            .FirstOrDefault(n => n.gameObject.activeSelf == false);

                        if (freePrefab != null)
                        {
                            freePrefab.Save();
                            freePrefab.gameObject.SetActive(true);
                            freePrefab.transform.SetParent(from.target);
                            freePrefab.transform.localPosition = chunk.objects[i].position;
                            freePrefab.Chunk = chunk;
                            freePrefab.ObjectId = i;
                            freePrefab.Load();
                        }
                        else
                        {
                            var newobj = Instantiate(
                                prefabs[chunk.objects[i].prefab].prefab,
                                (Vector2) from.target.position + chunk.objects[i].position,
                                Quaternion.identity,
                                from.target);

                            prefabs[chunk.objects[i].prefab].instantiated =
                                prefabs[chunk.objects[i].prefab].instantiated.Add(newobj);
                            newobj.Chunk = chunk;
                            newobj.ObjectId = i;
                            newobj.Load();
                        }

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
                        lastChunk = chunk;
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

        public struct Map
        {
            public float seed;
            public Chunk[,] chunks;
        }

        public struct Chunk
        {
            public DataObject[] objects;
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
            public Color mapColor;
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