using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>Turns one ingredient into another on a cutting board (e.g. Meat -> Chopped Meat).</summary>
    [CreateAssetMenu(menuName = "Nomad Chef/Cutting Recipe", fileName = "CuttingRecipe")]
    public class CuttingRecipeSO : ScriptableObject
    {
        [SerializeField] private KitchenObjectSO input;
        [SerializeField] private KitchenObjectSO output;
        [SerializeField, Min(0.1f)] private float chopTime = 1.5f;

        public KitchenObjectSO Input => input;
        public KitchenObjectSO Output => output;
        public float ChopTime => chopTime;
    }
}
