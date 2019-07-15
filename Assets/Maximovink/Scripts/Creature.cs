using System;
using System.Collections;
using Anima2D;
using UnityEngine;

namespace MaximovInk
{
    public class Creature : MonoBehaviour
    {
        public RobotM_P Head;
        public RobotM_P Body;
        public RobotM_P ra1;
        public RobotM_P ra2;
        public RobotM_P ra3;
        public RobotM_P la1;
        public RobotM_P la2;
        public RobotM_P la3;
        public RobotM_P rl1;
        public RobotM_P rl2;
        public RobotM_P rl3;
        public RobotM_P ll1;
        public RobotM_P ll2;
        public RobotM_P ll3;
        public RobotM_P Cam;


        public GameObject left_armIk;
        public GameObject right_armIk;
        public Bone2D right_armBone;

        public float speedCamX = 0.2f, speedCamY = 0.5f;
        public float camMin = 5, camMax = 10;

        public Transform camIK;

        public Transform weaponsParentLeft, weaponsParentRight;

        protected bool isTaking;

        public SpriteRenderer takingItemSprite;

        public int damage = 5;

        public LayerMask items_mask;

        protected DroppedItem item_on_ground;

        protected ItemInstance  EquipedItem;
        protected ItemInstance  DeequipedItem;

        protected Weapon EquipedWeapon;

        protected Vector2 defaultPosLeftIK, defaultPosRightIK;

        protected bool rightIkEquipped, twoArmsEquiped;

        public Rigidbody2D Rigidbody;
        public Animator Animator;

        protected virtual void DeequipWeapon()
        {

        }

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

        internal void Flip()
        {
            fac_right = !fac_right;

            transform.localScale =
                new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        internal void UpdateRP(RobotM_P rp)
        {
            if (rp.item.count == -1)
            {
                rp.item.condition = 100;
                rp.item.count = 1;
            }

            var cond = rp.item.condition / rp.item.item.MaxCondition;
            rp.part = (rp.item.item as RobotPartItem).part;
            if (rp.part is Hand_RP)
                rp.sprite.spriteMesh = rp.part.meshes[0];
            else
                rp.sprite.spriteMesh =
                    cond > 0.6f ? rp.part.meshes[0] : cond > 0.3f ? rp.part.meshes[1] : rp.part.meshes[2];
        }

        internal void DamageMP(Vector2 world_mp, bool right = true)
        {
            var hit = Physics2D.Raycast(world_mp, Vector2.zero);
            if (right)
                Animator.SetTrigger("attack_right");
            else
                Animator.SetTrigger("attack_left");
            if (hit)
            {
                var breakable = hit.collider.GetComponentInParent<Breakable>();

                if (breakable != null)
                {
                    breakable.Attack(5);
                }

                GameManager.Instance.MakeParticleAt(hit.collider.gameObject, hit.point);
            }
        }

        internal void DamageMP(Vector2 world_mp, MeleeWeapon weapon, bool right = true)
        {
            var hit = Physics2D.Raycast(world_mp, Vector2.zero);
            if (right)
                Animator.SetTrigger("attack_right");
            else
                Animator.SetTrigger("attack_left");
            if (hit)
            {
                var breakable = hit.collider.GetComponentInParent<Breakable>();

                if (breakable != null)
                {
                    breakable.Attack(weapon.TrashDamange);
                }

                GameManager.Instance.MakeParticleAt(hit.collider.gameObject, hit.point);
            }
        }

        internal int ReloadAmmo(int need, int canAdd, ShootingWeapon sh_wp)
        {
            if (need <= canAdd)
            {
                var ost = canAdd - need;
                sh_wp.ammo = sh_wp.maxAmmo;
                return ost;

            }

            sh_wp.ammo += canAdd;

            return 0;
        }

        public void UpdateSprites()
        {
            UpdateRP(Head);
            UpdateRP(la1);
            UpdateRP(la2);
            UpdateRP(la3);
            UpdateRP(ra1);
            UpdateRP(ra2);
            UpdateRP(ra3);
            UpdateRP(ll1);
            UpdateRP(ll2);
            UpdateRP(ll3);
            UpdateRP(rl1);
            UpdateRP(rl2);
            UpdateRP(rl3);
            UpdateRP(Body);
            /*ПОТОМ*/
            //UpdateRP(Cam);
        }
        
        private void Start()
        {
            BeforeStart();
            UpdateSprites();
            defaultPosRightIK = right_armIk.transform.position;
            defaultPosLeftIK = left_armIk.transform.position;

        }

        protected virtual void BeforeStart()
        {
            
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

        private void LateUpdate()
        {
            if (EquipedItem == null || !(EquipedWeapon is ShootingWeapon))
                if ((move.x > 0 && !facing_right) || (move.x < 0 && facing_right))
                    Flip();

        }

        internal void TakeItem()
        {
            StartCoroutine(_TakeItem());
        }

        private IEnumerator _TakeItem()
        {
            ra3.sprite.spriteMesh = ra3.part.meshes[1];
            isTaking = true;
            Animator.SetTrigger("takeItem");
            LockMove = true;
            yield return new WaitForSeconds(0.7f);
            takingItemSprite.sprite = item_on_ground.item.item.GetDropSprite();
            if (OnTakedItem(item_on_ground.item))
            {
                Destroy(item_on_ground.gameObject);
                item_on_ground = null;
            }

            yield return new WaitForSeconds(0.4f);
            takingItemSprite.sprite = null;
            yield return new WaitForSeconds(0.4f);
            ra3.sprite.spriteMesh = ra3.part.meshes[0];
            isTaking = false;
            LockMove = false;

        }

        protected virtual bool OnTakedItem(ItemInstance  item)
        {
            return false;
        }


        protected void FixedUpdate()
        {
            if (LockMove)
                move = Vector2.zero;



            if (run)
                Rigidbody.velocity = new Vector2(move.x * Speed * SpeedFactor, move.y * Speed * SpeedFactor);
            else
                Rigidbody.velocity = new Vector2(move.x * Speed, move.y * Speed);
        }

        [Serializable]
        public class RobotM_P
        {
            public RobotPart part;
            public SpriteMeshInstance sprite;
            public ItemInstance item = new ItemInstance { count = -1};
        }

    }
}