using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Cutting board. Put an ingredient down, press "use" once and the chef keeps chopping
    /// while standing at the board. Walking away pauses; progress is kept until the item is taken.
    /// </summary>
    public class CuttingCounter : BaseCounter, IHasProgress, IWorkStation
    {
        [SerializeField] private CuttingRecipeSO[] recipes;

        private float chopProgress;
        private PlayerController chopper;

        public event Action<float> ProgressChanged;
        public event Action<bool> ChoppingChanged;
        public event Action Chopped;

        public bool IsChopping => chopper != null;
        public WorkMotion Motion => WorkMotion.Chop;

        public bool IsWorkedBy(PlayerController player) => chopper == player;

        public override void Interact(PlayerController player)
        {
            KitchenObject before = KitchenObject;
            SwapWithPlayer(player);

            if (KitchenObject != before)
            {
                StopChopping();
                ResetProgress();
            }
        }

        public override void InteractAlternate(PlayerController player)
        {
            if (IsChopping || GetCurrentRecipe() == null)
                return;

            chopper = player;
            ChoppingChanged?.Invoke(true);
        }

        private void Update()
        {
            if (!IsChopping)
                return;

            CuttingRecipeSO recipe = GetCurrentRecipe();
            if (recipe == null || chopper.SelectedCounter != this || !chopper.isActiveAndEnabled)
            {
                StopChopping();
                return;
            }

            chopProgress += Time.deltaTime;
            ProgressChanged?.Invoke(Mathf.Clamp01(chopProgress / recipe.ChopTime));

            if (chopProgress >= recipe.ChopTime)
            {
                KitchenObject.DestroySelf();
                KitchenObject.Spawn(recipe.Output, this);
                StopChopping();
                ResetProgress();
                Chopped?.Invoke();
            }
        }

        private CuttingRecipeSO GetCurrentRecipe()
        {
            if (!this.HasKitchenObject())
                return null;

            foreach (CuttingRecipeSO recipe in recipes)
            {
                if (recipe.Input == KitchenObject.Data)
                    return recipe;
            }
            return null;
        }

        private void StopChopping()
        {
            if (!IsChopping)
                return;

            chopper = null;
            ChoppingChanged?.Invoke(false);
        }

        private void ResetProgress()
        {
            chopProgress = 0f;
            ProgressChanged?.Invoke(0f);
        }
    }
}
