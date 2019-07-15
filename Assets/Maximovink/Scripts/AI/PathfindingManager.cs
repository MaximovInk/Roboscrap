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

        public float generationProgress => (float)threadsComplete / threadsCount;
        public bool generateComplete;

        private int threadsComplete;

        private int threadsCount = 6;
        
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
            chunks = ChunkManager.instance.GetChunks();
            worldSize = chunks.GetLength(0);
            size = ChunkManager.instance.ChunkSize * chunks.GetLength(0) * ChunkManager.instance.TileScale;
            tiles = new bool[size, size];
            chunkUnits = ChunkManager.instance.ChunkSize * ChunkManager.instance.TileScale / unit;
            threadsCount = worldSize / unit;
            // GenerateGrid();
            Thread thread = new Thread(GenerateGrid);
            thread.Start();
        }

        
        
        private void GenerateGrid()
        {   
           /* for (int x = 0; x < chunks.GetLength(0); x++)
            {
                Parallel.For(0, chunks.GetLength(1), y =>
                {
                    for (int i = 0; i < chunkUnits; i++)
                    {
                        for (int j = 0; j < chunkUnits; j++)
                        {
                            tiles[x + i, y + j] = !ChunkManager.instance.UnitIsEmpty(x,y,i,j);
                        }
                    }
                });
                generationProgress = (float)x / chunks.GetLength(0);
                       
            }*/
           var threads = new Thread[threadsCount];

           var partSize = worldSize / threads.Length;

           for (int iThread = 0; iThread < threads.Length; iThread++)
           {
               var g = iThread;
               threads[iThread] = new Thread(() => { Generate(partSize * g, partSize * g + partSize);});
               threads[iThread].Start();
               
           }
           
            
             /*for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    for (int i = 0; i < chunkUnits; i++)
                    {
                        for (int j = 0; j < chunkUnits; j++)
                        {
                            tiles[x + i, y + j] = !ChunkManager.instance.UnitIsEmpty(x,y,i,j);
                        }
                    }
                    
                }

                generationProgress = (float)x / chunks.GetLength(0);
            }*/

            //generateComplete = true;
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
                            tiles[x + i*unit, y + j*unit] = !ChunkManager.instance.UnitIsEmpty(x,y,i,j);
                        }
                    }
                    
                }

                            
            }

            threadsComplete++;

        }
        
        
    }
}