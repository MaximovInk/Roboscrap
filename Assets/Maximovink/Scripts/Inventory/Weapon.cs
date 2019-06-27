using Anima2D;
using UnityEngine;

namespace MaximovInk
{
    public class Weapon : MonoBehaviour
    {
        public Ik2D ik;

        public bool twoArms
        {
            get { return ik != null; }
        }

        public virtual void Attack()
        {
            
        }

    }
}