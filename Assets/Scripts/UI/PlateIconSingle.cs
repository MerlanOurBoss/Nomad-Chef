using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>One ingredient icon: a colored swatch until real sprites exist.</summary>
    public class PlateIconSingle : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer colorRenderer;

        public void SetIngredient(KitchenObjectSO ingredient)
        {
            var block = new MaterialPropertyBlock();
            colorRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, ingredient.IconColor);
            colorRenderer.SetPropertyBlock(block);
        }
    }
}
