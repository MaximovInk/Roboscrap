using UnityEngine;

namespace MaximovInk
{
    public class Bullet : MonoBehaviour
    {
        public int value = 20;

        private void Start()
        {
            Destroy(gameObject,10);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var creature = other.GetComponent<Creature>();
            var breakable = other.GetComponentInParent<Breakable>();
            
            if (creature != null)
            {
                creature.Health -= value;
            }

            if (breakable != null)
            {
                breakable.Attack(value/4);
                
            }
            GameManager.Instance.MakeParticleAt(other.gameObject, transform.position);
            Destroy(gameObject);
        }
    }
}