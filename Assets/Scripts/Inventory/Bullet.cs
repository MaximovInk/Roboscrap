using UnityEngine;

namespace MaximovInk
{
    public class Bullet : MonoBehaviour
    {
        public float value = 20;

        private void Start()
        {
            Destroy(gameObject,10);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var creature = other.GetComponent<Creature>();
            var trash = other.GetComponentInParent<Trash>();
            
            if (creature != null)
            {
                creature.Health -= 20;
            }

            if (trash != null)
            {
                trash.Attack(value);
                
            }
            GameManager.Instance.MakeParticleAt(other.gameObject, transform.position);
            Destroy(gameObject);
        }
    }
}