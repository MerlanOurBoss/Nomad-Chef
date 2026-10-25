using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// Data for one kind of item (ingredient, dish, plate...). The prefab is what gets spawned in the world.
    /// </summary>
    [CreateAssetMenu(menuName = "Nomad Chef/Kitchen Object", fileName = "KitchenObject")]
    public class KitchenObjectSO : ScriptableObject
    {
        [SerializeField] private KitchenObject prefab;
        [SerializeField] private string displayName;
        [SerializeField] private Color iconColor = Color.white;
        [Tooltip("Tools and plates must never be thrown away.")]
        [SerializeField] private bool canBeTrashed = true;

        public KitchenObject Prefab => prefab;
        public string DisplayName => displayName;
        public Color IconColor => iconColor;
        public bool CanBeTrashed => canBeTrashed;
    }
}
