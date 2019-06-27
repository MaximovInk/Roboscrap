using UnityEngine;

namespace MaximovInk
{
    [CreateAssetMenu(menuName = "RobotPart/Wrist")]
    public class Hand_RP : RobotPart
    {
        public float Damage = 1;
        public float StoneMining = 0.05f;
        public float WoodMining = 0.5f;
        public float MetalBreaking = 0.1f;
        public float TrashBreaking = 1;
        public bool CanKeepItems = true;
    }
}