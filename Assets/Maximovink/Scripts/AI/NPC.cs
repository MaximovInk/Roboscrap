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