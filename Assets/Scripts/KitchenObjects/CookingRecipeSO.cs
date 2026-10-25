using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// What happens to an ingredient in the kazan: input cooks into cooked, and if left too long
    /// cooked turns into burned. Leave burned empty for food that never burns.
    /// </summary>
    [CreateAssetMenu(menuName = "Nomad Chef/Cooking Recipe", fileName = "CookingRecipe")]
    public class CookingRecipeSO : ScriptableObject
    {
        [SerializeField] private KitchenObjectSO input;
        [SerializeField] private KitchenObjectSO cooked;
        [SerializeField, Min(0.1f)] private float cookTime = 4f;
        [SerializeField] private KitchenObjectSO burned;
        [SerializeField, Min(0.1f)] private float burnTime = 6f;

        public KitchenObjectSO Input => input;
        public KitchenObjectSO Cooked => cooked;
        public float CookTime => cookTime;
        public KitchenObjectSO Burned => burned;
        public float BurnTime => burnTime;
        public bool CanBurn => burned != null;
    }
}
