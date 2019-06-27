using UnityEngine;

namespace MaximovInk
{
    public class ShootingWeapon : Weapon
    {
        public Transform startBullets;

        public float Speed = 1000;

        public bool canAttack = true;

        public int ammo = 10;
        
        public int maxAmmo = 10;
        public AmmoType AmmoType;
        public float fireRate = 0.1f;
        public float reloadingDelay = 1;

        private float timer = 0;
        
        
        public override void Attack()
        {
            if(ammo <=0 )
                return;
            
            if (timer > fireRate)
            {
                timer = 0;
                ammo -= 1;
                if (!canAttack)
                    return;
                var b = Instantiate(GameManager.Instance.bullet, startBullets.position, startBullets.rotation);
                b.GetComponent<Rigidbody2D>()
                    .AddForce(transform.right * (GameManager.Instance.player.facing_right ? 1 : -1) * Speed);
            }
        }

        private void Update()
        {
            if(ammo <=0 )
                return;
            
            if (timer <= fireRate)
            {
                timer += Time.deltaTime;
            }
            
           
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Player>())
            {
                canAttack = false;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<Player>())
            {
                canAttack = true;
            }
        }
    }
}