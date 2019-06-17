using UnityEngine;

namespace MaximovInk
{
    [CreateAssetMenu(menuName = "item/robot part")]
    public class RobotPartItem : Item
    {
        public float deterioration = 1;
        public RobotPart part;
    }
}