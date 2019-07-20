using UnityEngine;

namespace MaximovInk
{
    public class Bullet : MonoBehaviour
    {
        public int Damage = 20;
        public int TrashDamage = 1;
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
                creature.Health -= Damage;
            }

            if (breakable != null)
            {
                breakable.Attack(TrashDamage);
                
            }
            GameManager.Instance.MakeParticleAt(other.gameObject, transform.position);
            Destroy(gameObject);
        }
    }
}