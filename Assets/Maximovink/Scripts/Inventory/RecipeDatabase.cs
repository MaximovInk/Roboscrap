using System;
using UnityEngine;

namespace MaximovInk
{
    [CreateAssetMenu(menuName = "Recipe database")]
    public class RecipeDatabase : ScriptableObject
    {
        public Recipe[] Recipes;

    }
[Serializable]
    public class Recipe
    {
        public Item result;

        public Component[] Components;
        
        [Serializable]
        public class Component
        {
            public Item item;
            public int count;
        }
    }
}