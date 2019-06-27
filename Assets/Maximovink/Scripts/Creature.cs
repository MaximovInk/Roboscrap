using UnityEngine;

namespace MaximovInk
{
    public class Creature : MonoBehaviour
    {

        public Rigidbody2D Rigidbody;
        public Animator Animator;

        public int Health
        {
            get { return health; }
            set
            {
                health = value;
                OnHealthChange();
            }
        }

        private int health = 100;

        public int Damage = 1;

        public float Speed = 10;
        public float SpeedFactor = 2;
        public float Attack_KD = 0.5f;
        protected float lastKD;
        protected Vector2 move;
        protected bool run;
        public bool LockMove { get; set; }

        public bool facing_right
        {
            get { return fac_right; }
            set
            {
                if (fac_right != value) Flip();
            }
        }

        private bool fac_right = true;

        public float AttackDistance;


        protected virtual void OnHealthChange()
        {

        }

        protected void Flip()
        {
            fac_right = !fac_right;

            transform.localScale =
                new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        protected virtual void Update()
        {
            if (health <= 0)
            {
                Destroy(gameObject);
            }

            Animator.SetBool("moving", Rigidbody.velocity != Vector2.zero);
            Animator.SetBool("run", run);
        }


        protected virtual void FixedUpdate()
        {
            if (LockMove)
                move = Vector2.zero;



            if (run)
                Rigidbody.velocity = new Vector2(move.x * Speed * SpeedFactor, move.y * Speed * SpeedFactor);
            else
                Rigidbody.velocity = new Vector2(move.x * Speed, move.y * Speed);
        }
    }
}