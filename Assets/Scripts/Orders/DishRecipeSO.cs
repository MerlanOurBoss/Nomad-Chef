using System.Collections.Generic;
using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.Orders
{
    /// <summary>A dish customers can order: the exact set of ingredients on the plate, its score and patience.</summary>
    [CreateAssetMenu(menuName = "Nomad Chef/Dish Recipe", fileName = "Dish")]
    public class DishRecipeSO : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private KitchenObjectSO[] ingredients;
        [SerializeField, Min(0)] private int points = 20;
        [SerializeField, Min(1f)] private float patience = 60f;

        public string DisplayName => displayName;
        public IReadOnlyList<KitchenObjectSO> Ingredients => ingredients;
        public int Points => points;
        public float Patience => patience;

        /// <summary>True when the plate holds exactly this dish's ingredients, in any order.</summary>
        public bool Matches(IReadOnlyList<KitchenObjectSO> plateIngredients)
        {
            if (plateIngredients.Count != ingredients.Length)
                return false;

            foreach (KitchenObjectSO ingredient in ingredients)
            {
                bool found = false;
                for (int i = 0; i < plateIngredients.Count; i++)
                {
                    if (plateIngredients[i] == ingredient)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            return true;
        }
    }
}
