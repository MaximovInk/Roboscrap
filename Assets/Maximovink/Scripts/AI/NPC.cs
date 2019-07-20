using UnityEngine;

namespace MaximovInk.AI

{
    public class NPC : Creature
    {
        public Creature Enemy;
        public Transform NavigationTarget;

        public NPC_Group group;

        public int chunkResolution = 8;

        private float pixel;

        public Transform testedPrefab;

        public void Init()
        {
            pixel =  chunkResolution/ ((float)ChunkManager.Instance.chunkSize * ChunkManager.Instance.tileScale);
        }

        public class NPC_Behaviour
        {
           
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                
            }
        }
        
        
        
        
        public bool[,] ReadDataFromChunk(ChunkManager.ChunkSaveData data)
        {
            bool[,] generated = new bool[chunkResolution,chunkResolution];
            
            if (data.Objects != null)
                foreach (var obj in data.Objects)
                {
                    generated[(int) (obj.PositionX * pixel), (int) (obj.PositionY * pixel)] = true;
                }

            return generated;
        }

     
    }

    
    
    public enum NPC_State
    {
        IDLE,
        MOVE_TO,
        ATTACK,
        FINDING,
        HELPING
    }

    public enum NPC_Group
    {
        Cpu,
        Agro,
        A,
        B,
        C
    }

}