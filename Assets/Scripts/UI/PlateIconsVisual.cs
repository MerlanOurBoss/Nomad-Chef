using System.Collections.Generic;
using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>Row of ingredient icons floating above a plate, centered and facing the camera.</summary>
    public class PlateIconsVisual : MonoBehaviour
    {
        [SerializeField] private PlateKitchenObject plate;
        [SerializeField] private Transform iconRow;
        [SerializeField] private PlateIconSingle iconTemplate;
        [SerializeField] private float spacing = 0.26f;

        private readonly List<PlateIconSingle> icons = new List<PlateIconSingle>();

        private void Awake()
        {
            iconTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            plate.IngredientAdded += AddIcon;
            plate.IngredientsCleared += ClearIcons;
        }

        private void OnDisable()
        {
            plate.IngredientAdded -= AddIcon;
            plate.IngredientsCleared -= ClearIcons;
        }

        private void AddIcon(KitchenObjectSO ingredient)
        {
            PlateIconSingle icon = Instantiate(iconTemplate, iconRow);
            icon.gameObject.SetActive(true);
            icon.SetIngredient(ingredient);
            icons.Add(icon);
            Layout();
        }

        private void ClearIcons()
        {
            foreach (PlateIconSingle icon in icons)
                Destroy(icon.gameObject);
            icons.Clear();
        }

        private void Layout()
        {
            float start = -(icons.Count - 1) * spacing * 0.5f;
            for (int i = 0; i < icons.Count; i++)
                icons[i].transform.localPosition = new Vector3(start + i * spacing, 0f, 0f);
        }
    }
}
