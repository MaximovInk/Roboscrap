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
        public bool workbench;
        
        public Vector3 centerPt = Vector3.zero;
        public float radius = 1.2f;
        
        private void SetAmmoText()
        {
            var weapon = (EquipedWeapon as ShootingWeapon);
            var inventory =
                weapon != null && weapon.AmmoType == AmmoType.Pistol ? GameManager.Instance.mainInventory.PistolAmmo :
                weapon != null && weapon.AmmoType == AmmoType.Fraction ? GameManager.Instance.mainInventory.ShotgunAmmo :
                 GameManager.Instance.mainInventory.AutomaticAmmo 
                ;

            if (weapon != null)
                GameManager.Instance.ammoText.text = "Ammo: " + weapon.ammo + "/" + inventory;
        }

        public bool EquipWeapon(ItemInstance item)
        {
            if (EquipedItem != null)
                DeequipWeapon();
            
            var player = GameManager.Instance.player;

            var weapon = item.item as WeaponItem;
            if (weapon == null)
                return false;
            if (weapon.prefab.twoArms)
            {
                if (((Hand_RP) player.la3.part).CanKeepItems && ((Hand_RP) player.ra3.part).CanKeepItems)
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

                if (((Hand_RP) player.la3.part).CanKeepItems)
                {

                    var instance = Instantiate(weapon.prefab, weaponsParentLeft);
                    instance.transform.localPosition = Vector3.zero;
                    EquipedItem = item.Clone();
                    rightIkEquipped = false;
                    twoArmsEquiped = false;
                    EquipedWeapon = instance;

                    if (EquipedWeapon is ShootingWeapon)
                        SetAmmoText();
                    return true;
                }

                if (((Hand_RP) player.ra3.part).CanKeepItems)
                {
                    var instance = Instantiate(weapon.prefab, weaponsParentLeft);
                    instance.transform.localPosition = Vector3.zero;
                    EquipedItem = item.Clone();
                    rightIkEquipped = true;
                    twoArmsEquiped = false;
                    EquipedWeapon = instance;
                    if (EquipedWeapon is ShootingWeapon)
                        SetAmmoText();
                    return true;
                }

                /*Ни в одну из рук не влезает!*/
            }



            return false;
        }

        protected override void DeequipWeapon()
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

            for (var i = 0; i < weaponsParentLeft.childCount; i++)
            {
                Destroy(weaponsParentLeft.GetChild(i).gameObject);
            }

            for (var i = 0; i < weaponsParentRight.childCount; i++)
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
       
        protected override void Update()
        {
            base.Update();
            run = Input.GetKey(KeyCode.LeftShift);

            move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector2 mouse_pos = Input.mousePosition;
            Vector2 world_mp = Camera.main.ScreenToWorldPoint(mouse_pos);
            if (!isTaking)
            {
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
                                    DamageMP(world_mp,(MeleeWeapon) EquipedWeapon);
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
                                        DamageMP(world_mp,false);
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
                                        DamageMP(world_mp,EquipedWeapon as MeleeWeapon, false);
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
                        DamageMP(world_mp,false);
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
                        TakeItem();
                } 
            }

        }

        protected override bool OnTakedItem(ItemInstance  item)
        {
            return GameManager.Instance.mainInventory.AddItem(item);
            
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var selectable = other.GetComponent<SelectableObject>();
            
            if (other.GetComponentInParent<WorkbenchStation>() != null)
            {
                workbench = true;
                GameManager.Instance.WorkbenchChanged(true);
            }
            
            if (selectable != null)
            {
                
                selectable.OnShow();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var selectable = other.GetComponent<SelectableObject>();

            if (other.GetComponentInParent<WorkbenchStation>() != null)
            {
                workbench = false;
                GameManager.Instance.WorkbenchChanged(false);
            }

            if (selectable != null)
            { 
               
                selectable.OnHide();
            }
        }
    }
}