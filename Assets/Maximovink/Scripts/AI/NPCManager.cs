using System;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace MaximovInk.AI
{
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager instance;

        public NPC[] prefabs;

        public FractionData[] fractions = new FractionData[]
        {
            new FractionData{NpcGroup = NPC_Group.Agro},
            new FractionData{NpcGroup = NPC_Group.A},
            new FractionData{NpcGroup = NPC_Group.B},
            new FractionData{NpcGroup = NPC_Group.C},
            new FractionData{NpcGroup = NPC_Group.Cpu}
        };
        
        public void Save()
        {
            
        }

        public void Load()
        {
            
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


    }



    [MessagePackObject]
    public class FractionData
    {
        [Key(0)] public NPC_Group NpcGroup;
        [Key(1)] public NPCData[] Datas;
    }

    [MessagePackObject]
    public class NPCData
    {
        [Key(0)]
        public float positionX;
        [Key(1)]
        public float positionY;
        [Key(2)]
        public NPC_State state;
        [Key(3)]
        public int prefabIndex;
        [Key(4)]
        public object[] data;
    }
}