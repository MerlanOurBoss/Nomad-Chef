using System;
using System.Collections.Generic;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// A plate is a kitchen object that also holds a set of ingredients.
    /// Only listed ingredients are accepted, each one at most once.
    /// </summary>
    public class PlateKitchenObject : KitchenObject
    {
        [SerializeField] private KitchenObjectSO[] validIngredients;

        private readonly List<KitchenObjectSO> ingredients = new List<KitchenObjectSO>();

        public event Action<KitchenObjectSO> IngredientAdded;
        public event Action IngredientsCleared;

        public IReadOnlyList<KitchenObjectSO> Ingredients => ingredients;

        public bool CanAdd(KitchenObjectSO ingredient)
        {
            return Array.IndexOf(validIngredients, ingredient) >= 0 && !ingredients.Contains(ingredient);
        }

        public bool TryAddIngredient(KitchenObjectSO ingredient)
        {
            if (!CanAdd(ingredient))
                return false;

            ingredients.Add(ingredient);
            IngredientAdded?.Invoke(ingredient);
            return true;
        }

        public void ClearIngredients()
        {
            if (ingredients.Count == 0)
                return;

            ingredients.Clear();
            IngredientsCleared?.Invoke();
        }
    }
}
