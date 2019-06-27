using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Anima2D;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MaximovInk
{
    public class Player : Creature
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

        private bool isTaking;

        public SpriteRenderer takingItemSprite;

        public int damage = 5;

        public LayerMask items_mask;

        private DroppedItem item_on_ground;
        private Vector2 old_mouse;

        private ItemInstance EquipedItem;
        private ItemInstance DeequipedItem;
        
        private Weapon EquipedWeapon;
        
        private Vector2 defaultPosLeftIK, defaultPosRightIK;

        private bool rightIkEquipped,twoArmsEquiped;

        public bool workbench;
        
        public Vector3 centerPt = Vector3.zero;
        public float radius = 1.2f;
        
        [Serializable]
        public class RobotM_P
        {
            public RobotPart part;
            public SpriteMeshInstance sprite;
            public ItemInstance item;
        }

        public void DeequipWeapon()
        {
            left_armIk.SetActive(true);

            if (EquipedWeapon is ShootingWeapon)
            {
                var sh_wp = EquipedWeapon as ShootingWeapon;
                
                switch (sh_wp.AmmoType)
                {
                    case AmmoType.Fraction:
                        GameManager.Instance.mainInventory.ShotgunAmmo += sh_wp.ammo;
                        break;
                    case AmmoType.Pistol:
                        GameManager.Instance.mainInventory.PistolAmmo += sh_wp.ammo;
                        break;
                    case AmmoType.Automatic:
                        GameManager.Instance.mainInventory.AutomaticAmmo += sh_wp.ammo;
                        break;
                }
            }

            for (int i = 0; i < weaponsParentLeft.childCount; i++)
            {
                Destroy(weaponsParentLeft.GetChild(i).gameObject);
            }

            for (int i = 0; i < weaponsParentRight.childCount; i++)
            {
                Destroy(weaponsParentRight.GetChild(i).gameObject);
            }

            DeequipedItem = EquipedItem.Clone();
            GameManager.Instance.mainInventory.AddItem(DeequipedItem);
            GameManager.Instance.ammoText.text = string.Empty;
            EquipedItem = null;
            EquipedWeapon = null;
            right_armIk.transform.position = defaultPosRightIK;
            left_armIk.transform.position = defaultPosLeftIK;
            right_armIk.SetActive(true);
        }

        private void SetAmmoText()
        {
            var weapon = (EquipedWeapon as ShootingWeapon);
            var inventory =
                weapon.AmmoType == AmmoType.Pistol ? GameManager.Instance.mainInventory.PistolAmmo :
                weapon.AmmoType == AmmoType.Fraction ? GameManager.Instance.mainInventory.ShotgunAmmo :
                 GameManager.Instance.mainInventory.AutomaticAmmo 
                ;
            
            GameManager.Instance.ammoText.text = "Ammo: " + weapon.ammo+ "/" + inventory;
        }

        public bool EquipWeapon(ItemInstance item)
        {
            if (EquipedItem != null)
                DeequipWeapon();
            
            var player = GameManager.Instance.player;

            var weapon = item.item as WeaponItem;

            if (weapon.prefab.twoArms)
            {
                if ((player.la3.part as Hand_RP).CanKeepItems && (player.ra3.part as Hand_RP).CanKeepItems)
                {
                    /*Взять в две руки*/
                    var instance = Instantiate(weapon.prefab, weaponsParentLeft);
                    instance.transform.localPosition = Vector3.zero;
                    right_armIk.SetActive(false);
                    instance.ik.target = right_armBone;
                    EquipedItem = item.Clone();
                    rightIkEquipped = false;
                    twoArmsEquiped = true;
                    EquipedWeapon = instance;
                    if(EquipedWeapon is ShootingWeapon)
                        SetAmmoText();
                    return true;
                }
            }
            else
            {

                if ((player.la3.part as Hand_RP).CanKeepItems)
                {
                    var instance = Instantiate(weapon.prefab, weaponsParentLeft);
                    instance.transform.localPosition = Vector3.zero;
                    EquipedItem = item.Clone();
                    rightIkEquipped = false;
                    twoArmsEquiped = false;
                    EquipedWeapon = instance;
                    if(EquipedWeapon is ShootingWeapon)
                        SetAmmoText();
                    return true;
                }
                else if ((player.ra3.part as Hand_RP).CanKeepItems)
                {
                    var instance = Instantiate(weapon.prefab, weaponsParentLeft);
                    instance.transform.localPosition = Vector3.zero;
                    EquipedItem = item.Clone();
                    rightIkEquipped = true;
                    twoArmsEquiped = false;
                    EquipedWeapon = instance;
                    if(EquipedWeapon is ShootingWeapon)
                        SetAmmoText();
                    return true;
                }
                else
                {
                    /*Ни в одну из рук не влезает!*/
                }
            }

         
            
            return false;
        }

        private float flipped
        {
            get { return facing_right ? 1 : -1; }
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
            UpdateSprites();
            defaultPosRightIK = right_armIk.transform.position;
            defaultPosLeftIK = left_armIk.transform.position;
            
        }

        private void UpdateRP(RobotM_P rp)
        {
            var cond = rp.item.condition / rp.item.item.MaxCondition;
            rp.part = (rp.item.item as RobotPartItem).part;
            if (rp.part is Hand_RP)
                rp.sprite.spriteMesh = rp.part.meshes[0];
            else
                rp.sprite.spriteMesh =
                    cond > 0.6f ? rp.part.meshes[0] : cond > 0.3f ? rp.part.meshes[1] : rp.part.meshes[2];
        }

        private void DamageMP(Vector2 world_mp)
        {
            RaycastHit2D hit = Physics2D.Raycast(world_mp, Vector2.zero);
            Animator.SetTrigger("attack_right");
            if (hit)
            {
                var trash = hit.collider.GetComponentInParent<Trash>();

                if (trash != null)
                {
                    trash.Attack(5);
                }
                GameManager.Instance.MakeParticleAt(hit.collider.gameObject,hit.point);
            }
        }
        private void DamageMP(Vector2 world_mp, int amount, bool right = true)
        {
            RaycastHit2D hit = Physics2D.Raycast(world_mp, Vector2.zero);
            if(right)
            Animator.SetTrigger("attack_right");
            else
                Animator.SetTrigger("attack_left");
            if (hit)
            {
                var trash = hit.collider.GetComponentInParent<Trash>();

                if (trash != null)
                {
                    trash.Attack(amount);
                }
                GameManager.Instance.MakeParticleAt(hit.collider.gameObject,hit.point);
            }
        }

        private int ReloadAmmo(int need,int canAdd,ShootingWeapon sh_wp)
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

        protected override void Update()
        {
            base.Update();
            run = Input.GetKey(KeyCode.LeftShift);

            move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector2 mouse_pos = Input.mousePosition;
            Vector2 world_mp = Camera.main.ScreenToWorldPoint(mouse_pos);
            if (!isTaking)
            {


                Vector3 delta = mouse_pos - old_mouse;
                camIK.position = world_mp;
                camIK.localPosition = new Vector3(
                    Mathf.Clamp(camIK.localPosition.x,
                        camMin,
                        camMax),
                    Mathf.Clamp(camIK.localPosition.y, camMin, camMax)
                );

                
                if (Input.GetKeyDown(KeyCode.G))
                {
                    if (EquipedWeapon != null)
                    {
                        DeequipWeapon();
                    }
                    else
                    {
                        if (DeequipedItem != null && GameManager.Instance.mainInventory.RemoveItem(DeequipedItem))
                        {
                            EquipWeapon(DeequipedItem);
                        }
                    }
                }

                if (GameManager.Instance.MouseIsFree)
                {
                     if (EquipedItem != null)
                {
                    if (EquipedWeapon is ShootingWeapon)
                    {
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            var sh_wp = EquipedWeapon as ShootingWeapon;
                            if (sh_wp.ammo < sh_wp.maxAmmo)
                            {
                                
                                
                                var need = sh_wp.maxAmmo - sh_wp.ammo;
                                var canAdd = 0;

                                switch (sh_wp.AmmoType)
                                {
                                    case AmmoType.Fraction:
                                        canAdd += GameManager.Instance.mainInventory.ShotgunAmmo;
                                        GameManager.Instance.mainInventory.ShotgunAmmo = ReloadAmmo(need,canAdd,sh_wp);
                                        break;
                                    case AmmoType.Pistol:
                                        canAdd += GameManager.Instance.mainInventory.PistolAmmo;
                                        GameManager.Instance.mainInventory.PistolAmmo = ReloadAmmo(need,canAdd,sh_wp);
                                        break;
                                    case AmmoType.Automatic:
                                        canAdd += GameManager.Instance.mainInventory.AutomaticAmmo;
                                        GameManager.Instance.mainInventory.AutomaticAmmo = ReloadAmmo(need,canAdd,sh_wp);
                                        break;

                                }

                               

                            }
                            SetAmmoText();
                        }
                    }

                    if (twoArmsEquiped)
                        {
                            if (EquipedWeapon is ShootingWeapon)
                            {
                                left_armIk.transform.position = world_mp;

                                left_armIk.transform.localPosition = centerPt + Vector3.ClampMagnitude(left_armIk.transform.localPosition, radius);
                                
                                left_armIk.transform.localPosition = new Vector3(
                                    Mathf.Clamp(left_armIk.transform.localPosition.x,1f,40f),
                                    Mathf.Clamp(left_armIk.transform.localPosition.y,-40f,40f)
                                );
                                
                                if (world_mp.x < transform.position.x && facing_right)
                                {
                                    Flip();
                                }
                                else if (world_mp.x > transform.position.x && !facing_right)
                                {
                                    Flip();
                                }
                            }

                            if (Input.GetMouseButtonDown(0))
                            {
                                if(EquipedWeapon is MeleeWeapon)
                                    DamageMP(world_mp,((MeleeWeapon) EquipedWeapon).Damage);
                            }

                            if (Input.GetMouseButton(0))
                            {
                                if (EquipedWeapon is ShootingWeapon)
                                {
                                    EquipedWeapon.Attack();

                                    SetAmmoText();

                                }
                            }
                        }
                        else
                        {
                            if (rightIkEquipped)
                            {
                                if (EquipedWeapon is ShootingWeapon)
                                {
                                    
                                        right_armIk.transform.position = world_mp;
                                        if (right_armIk.transform.position.x < transform.position.x && facing_right)
                                        {
                                            Flip();
                                        }
                                        else if (right_armIk.transform.position.x > transform.position.x && !facing_right)
                                        {
                                            Flip();
                                        }
                                        if (Input.GetMouseButtonDown(0))
                                        { 
                                            DamageMP(world_mp);
                                        }

                                        if (Input.GetMouseButton(1))
                                        {
                                            EquipedWeapon.Attack();
                                            SetAmmoText();
                                        }
                                }
                                else
                                {
                                    if (Input.GetMouseButtonDown(0))
                                    { 
                                        DamageMP(world_mp);
                                    }

                                    if (Input.GetMouseButtonDown(1))
                                    {
                                        DamageMP(world_mp,damage,false);
                                    }
                                }


                            }
                            else
                            {
                                if (EquipedWeapon is ShootingWeapon)
                                {
                                    left_armIk.transform.position = world_mp;
                                    if (left_armIk.transform.position.x < transform.position.x && facing_right)
                                    {
                                        Flip();
                                    }
                                    else if (left_armIk.transform.position.x > transform.position.x && !facing_right)
                                    {
                                        Flip();
                                    }

                                    if (Input.GetMouseButton(0))
                                    {
                                        EquipedWeapon.Attack();
                                        SetAmmoText();
                                    }
                                    
                                    if (Input.GetMouseButtonDown(1))
                                    {
                                        DamageMP(world_mp);
                                    }
                                    
                                }
                                else
                                {
                                    if (Input.GetMouseButtonDown(0))
                                    {
                                        DamageMP(world_mp);
                                    }
                                    
                                    if (Input.GetMouseButtonDown(1))
                                    {
                                        DamageMP(world_mp,(EquipedWeapon as MeleeWeapon).Damage, false);
                                    }
                                }


                            }
                        }
                    
                    
                    
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && (la3.part as Hand_RP).CanKeepItems)
                    {
                        DamageMP(world_mp);
                    }
                     
                    if (Input.GetMouseButtonDown(1) && (ra3.part as Hand_RP).CanKeepItems)
                    {
                        DamageMP(world_mp,damage,false);
                    }
                }
                }
            }

            if (GameManager.Instance.MouseIsFree)
            {
                if (Input.GetKeyDown(KeyCode.F) && !isTaking)
                {
                    var d = Physics2D.OverlapCircle(transform.position, 2, items_mask);
                    if (d != null)
                        item_on_ground = d.GetComponent<DroppedItem>();
                    if (item_on_ground != null)
                        StartCoroutine(TakeItem());
                } 
            }

          

            old_mouse = mouse_pos;
        }

        private void LateUpdate()
        {
            if (EquipedItem == null || !(EquipedWeapon is ShootingWeapon))
                if ((move.x > 0 && !facing_right) || (move.x < 0 && facing_right))
                    Flip();

        }

        IEnumerator TakeItem()
        {
            ra3.sprite.spriteMesh = ra3.part.meshes[1];
            isTaking = true;
            Animator.SetTrigger("takeItem");
            LockMove = true;
            yield return new WaitForSeconds(0.7f);
            takingItemSprite.sprite = item_on_ground.item.item.GetDropSprite();
            if (GameManager.Instance.mainInventory.AddItem(item_on_ground.item))
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            var selectable = other.GetComponent<SelectableObject>();
            
            
            if (selectable != null)
            {
                if (selectable is WorkbenchStation)
                {
                    workbench = true;
                    GameManager.Instance.WorkbenchChanged(true);
                }
                selectable.OnShow();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var selectable = other.GetComponent<SelectableObject>();

           

            if (selectable != null)
            { 
                if (selectable is WorkbenchStation)
                {
                    workbench = false;
                    GameManager.Instance.WorkbenchChanged(false);
                }
                selectable.OnHide();
            }
        }
    }
}