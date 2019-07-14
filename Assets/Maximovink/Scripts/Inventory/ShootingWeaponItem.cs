using UnityEngine;

namespace MaximovInk
{
    [CreateAssetMenu(menuName = "item/shooting weapon")]
    public class ShootingWeaponItem : WeaponItem
    {
       
    }

    public enum AmmoType
    {
        Fraction,
        Pistol,
        Automatic
    }
}