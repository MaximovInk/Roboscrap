using UnityEngine;

namespace MaximovInk.AI

{
    public class NPC : Creature
    {
        public Creature Enemy;
        public Transform NavigationTarget;

        public class NPC_Behaviour
        {
           
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
    
}