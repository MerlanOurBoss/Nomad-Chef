using NomadChef.KitchenObjects;
using NomadChef.Orders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NomadChef.UI
{
    /// <summary>One order ticket: dish name, ingredient icons and a patience bar that drains green to red.</summary>
    public class OrderTicketUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text dishName;
        [SerializeField] private Transform iconContainer;
        [SerializeField] private Image iconTemplate;
        [SerializeField] private RectTransform patienceFill;
        [SerializeField] private Image patienceFillImage;
        [SerializeField] private Gradient patienceColors = DefaultPatienceColors();

        private Order order;

        public void Bind(Order newOrder)
        {
            order = newOrder;
            dishName.text = order.Recipe.DisplayName;

            iconTemplate.gameObject.SetActive(false);
            foreach (KitchenObjectSO ingredient in order.Recipe.Ingredients)
            {
                Image icon = Instantiate(iconTemplate, iconContainer);
                icon.color = ingredient.IconColor;
                icon.gameObject.SetActive(true);
            }

            Refresh();
        }

        private void Update()
        {
            if (order != null)
                Refresh();
        }

        private void Refresh()
        {
            float t = Mathf.Clamp01(order.NormalizedTimeLeft);
            patienceFill.anchorMax = new Vector2(t, 1f);
            patienceFillImage.color = patienceColors.Evaluate(t);
        }

        private static Gradient DefaultPatienceColors()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.90f, 0.25f, 0.20f), 0f),
                    new GradientColorKey(new Color(0.95f, 0.75f, 0.20f), 0.4f),
                    new GradientColorKey(new Color(0.40f, 0.80f, 0.30f), 0.75f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
