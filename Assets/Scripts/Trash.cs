using System;
using UnityEngine;

namespace MaximovInk
{
    public class Trash : MonoBehaviour
    {
        public float hp = 100;

       public Resource[] Resources;

        public void Attack(float amount)
        {
            hp -= amount;
            if (hp <= 0)
            {
                /* Добыть мусор */
                Destroy(gameObject);
            }
        }
    }
[Serializable]
    public class Resource
    {
        public ResourceType type;
        public int min = 0 , max = 100;
    }

    public enum ResourceType
    {
        Wood,
        Metal,
        Glass,
        Microschemes,
        Wires,
        Ceramic,
        Rubber,
        RadioElectricity,
        ElectoComponents,
        Components
    }
}