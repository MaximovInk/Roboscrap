using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MaximovInk
{
    public class ChunkManager : MonoBehaviour
    {
        public int TrashPerChunkMax = 10;
        public int TrashPerChunkMin = 0;

        //public Chunk[,] chunks;
        
        public int ChunkSize = 32;

        private const int _chunkVisibality = 4;

        public int worldSize = 2000;

        public float iterationsDelay = 0.1f;
        private float timer = 0;

        public float scale = 1;

        public float threshold = 0.8f;
        
        public Transform target;
        private Vector3 lastPos;
        private Vector2Int lastChunk;

        private List<LoadedChunk> _loadedChunks;

        public PrefabForPool[] prefabs;

        private const string path = "saves/null/map.json";
        
        private float offset => _chunkVisibality / 2 * ChunkSize;

        private Map map;
        
        public Chunk GetChunkData(int x, int y)
        {
           return map.chunks[x + map.chunks.GetLength(0) / 2, y + map.chunks.GetLength(1) / 2];
        }

        //public int seed = 0;

        private bool TrashNearInChunk(int x , int y,Chunk chunk)
        {
            for (int i = 0; i < chunk.objects.Length; i++)
            {
                if (x > chunk.objects[i].position.x - 10 && x < chunk.objects[i].position.x + 10)
                {
                    if (y > chunk.objects[i].position.y - 10 && y < chunk.objects[i].position.y + 10)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Start()
        {
            //chunks = new Transform[worldSize,worldSize];
            
            map = new Map();
            
            
            map.seed = Random.Range(0,10000);
            
            map.chunks = new Chunk[worldSize,worldSize];
            
            
            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < worldSize; y++)
                {
                    map.chunks[x,y] = new Chunk();

                    for (int i = 0; i < ChunkSize; i++)
                    {
                        for (int j = 0; j < ChunkSize; j++)
                        {
                            float obj = Mathf.PerlinNoise((x*(float)ChunkSize+i)/scale+map.seed,(y*(float)ChunkSize+j)/scale+map.seed);
                            if (obj > threshold)
                            {
                                
                                if(!TrashNearInChunk(i,j,map.chunks[x,y]))
                                    map.chunks[x,y].objects = map.chunks[x,y].objects.Add(new DataObject{data = 255,prefab = 0, position = new Vector2(i,j)});
                                
                            }
                        }
                    }
                }
            }

            _loadedChunks = new List<LoadedChunk>(_chunkVisibality^2);
            
            for (int x = 0; x < _chunkVisibality; x++)
            {
                for (int y = 0; y < _chunkVisibality; y++)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);


                    go.transform.localPosition = new Vector3(x * ChunkSize - offset, y * ChunkSize - offset);

                    _loadedChunks.Add(new LoadedChunk{target = go.transform, x = -1, y = -1});
                    go.name = "chunk";

                }
            }
            
            
            
            UpdateChunksPos();
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
                
                from.target.localPosition = new Vector3(x * ChunkSize - offset, y * ChunkSize - offset);
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
                        freePrefab.transform.localPosition = chunk.objects[i].position;
                        freePrefab.Chunk = chunk;
                        freePrefab.ObjectId = i;
                        freePrefab.Load();
                    }
                    else
                    {
                        var newobj = Instantiate(
                            prefabs[chunk.objects[i].prefab].prefab ,
                            (Vector2)from.target.position + chunk.objects[i].position,
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
            return Vector2Int.FloorToInt(new Vector2(pos.x/ChunkSize+_chunkVisibality/2, pos.y/ChunkSize+_chunkVisibality/2));
        }
        
        private void Update()
        {
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
            public uint data;
            public byte prefab;
            public Vector2 position;
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