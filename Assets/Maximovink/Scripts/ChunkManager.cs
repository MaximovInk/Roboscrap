using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace MaximovInk
{
    public class ChunkManager : MonoBehaviour
    {
        public static ChunkManager Instance;

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
            public Color mapColor = Color.white;
        }
        
        [Serializable]
        public class PrefabForPool
        {
            public SavedPrefabBehaviour prefab;
            [HideInInspector]
            public SavedPrefabBehaviour[] instantiated;

            public bool getColorFromGroup = true;
            public Color mapColor = Color.white;
        }
        
    }