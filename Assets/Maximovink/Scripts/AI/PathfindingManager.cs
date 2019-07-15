using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MaximovInk.AI
{
    public class PathfindingManager : MonoBehaviour
    {
        public bool[,] tiles;
        private ChunkManager.Chunk[,] chunks;

        private int worldSize;
        private int unit = 2;
        private int size;
        private int chunkUnits;

        public static PathfindingManager instance;

        public float generationProgress => (float)sectorsComplete / sectorsCount;
        public bool generateComplete;

        private int sectorsCount , sectorsComplete;

        private int threadsCount = 6;
        private int threadLimit;
        
        private Task[] threads;
        private Task generationThread;
        
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

        public void GenerateMap()
        {
            chunks = ChunkManager.instance.map.chunks;
            worldSize = chunks.GetLength(0);
            size = ChunkManager.instance.ChunkSize * chunks.GetLength(0) * ChunkManager.instance.TileScale;
            tiles = new bool[size, size];
            chunkUnits = ChunkManager.instance.ChunkSize * ChunkManager.instance.TileScale / unit;
            

            sectorsCount = worldSize/unit;
            threadsCount = 4;
            generationThread = new Task(GenerateGrid);
            generationThread.Start();
        }

        
        
        private void GenerateGrid()
        {   
          
           threads = new Task[threadsCount];

           var partSize = worldSize/unit / threads.Length;

           for (int iThread = 0; iThread < threads.Length; iThread++)
           {
               var g = iThread;
               threads[iThread] = new Task(() => { Generate(partSize * g, partSize * g + partSize);});
               threads[iThread].Start();
               
           }
         

        }

        public void Clear()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Dispose();
            }

            threads = null;
            generationThread.Dispose();
            generationThread = null;
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            System.GC.Collect();
        }

        private void Generate(int startIndex , int endIndex)
        {
            for (int x = startIndex; x < endIndex; x++)
            {
                for (int y = startIndex; y < endIndex; y++)
                {
                    for (int i = 0; i < chunkUnits; i++)
                    {
                        for (int j = 0; j < chunkUnits; j++)
                        {
                            tiles[x + i, y + j] = !ChunkManager.instance.UnitIsEmpty(x,y,i*unit,j*unit);
                        }
                    }
                    
                }

                sectorsComplete++;       
            }

            
           
            

        }
        
        
    }
}