using System.Collections.Generic;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// Shows the ingredients on a plate by stacking a copy of each ingredient's own model.
    /// New ingredients appear automatically without authoring plate-specific visuals.
    /// </summary>
    public class PlateContentsVisual : MonoBehaviour
    {
        [SerializeField] private PlateKitchenObject plate;
        [SerializeField] private Transform contentsRoot;
        [SerializeField] private float layerHeight = 0.035f;
        [SerializeField] private float ingredientScale = 0.85f;

        private readonly List<GameObject> spawned = new List<GameObject>();

        private void OnEnable()
        {
            plate.IngredientAdded += AddVisual;
            plate.IngredientsCleared += ClearVisuals;
        }

        private void OnDisable()
        {
            plate.IngredientAdded -= AddVisual;
            plate.IngredientsCleared -= ClearVisuals;
        }

        private void AddVisual(KitchenObjectSO ingredient)
        {
            KitchenObject copy = Instantiate(ingredient.Prefab, contentsRoot);
            // Only the model is wanted; the copy must not act as a real kitchen object.
            Destroy(copy);

            Transform t = copy.transform;
            t.localPosition = Vector3.up * (layerHeight * spawned.Count);
            t.localRotation = Quaternion.Euler(0f, spawned.Count * 40f, 0f);
            t.localScale = Vector3.one * ingredientScale;
            spawned.Add(copy.gameObject);
        }

        private void ClearVisuals()
        {
            foreach (GameObject visual in spawned)
                Destroy(visual);
            spawned.Clear();
        }
    }
}
