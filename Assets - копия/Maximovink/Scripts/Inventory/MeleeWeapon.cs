using UnityEngine;

namespace MaximovInk
{
    public class MeleeWeapon : Weapon
    {
        public int Damage = 10;
        
        /*public float TimeAttack = 2;
        private float timer = 0;
        private bool attacking;
        
        public override void Attack()
        {
            attacking = true;
        }

        private void Update()
        {
            if (attacking)
            {
                timer += Time.deltaTime;

                if (timer > TimeAttack)
                {
                    timer = 0;
                    attacking = false;
                }
            }
        }*/
    }
}