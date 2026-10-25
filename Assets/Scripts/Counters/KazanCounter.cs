using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Stove with a built-in kazan. Accepts only ingredients that have a cooking recipe,
    /// cooks them on its own (no player needed) and burns them if they are left in too long.
    /// </summary>
    public class KazanCounter : BaseCounter, IHasProgress
    {
        public enum State
        {
            Idle,
            Cooking,
            Cooked,
            Burned,
        }

        [SerializeField] private CookingRecipeSO[] recipes;
        [Tooltip("Burn progress (0..1) at which the warning sign and beeps start.")]
        [SerializeField, Range(0f, 1f)] private float burnWarningAt = 0.4f;
        [Tooltip("Seconds burned food can sit in the kazan before it catches fire.")]
        [SerializeField, Min(0f)] private float igniteDelay = 4f;

        private State state;
        private CookingRecipeSO recipe;
        private float timer;
        private bool ignited;

        public event Action<float> ProgressChanged;
        public event Action<State> StateChanged;

        public State CurrentState => state;

        /// <summary>0..1 how close the cooked food is to burning (0 outside the Cooked state).</summary>
        public float BurnProgress => state == State.Cooked && recipe.CanBurn ? timer / recipe.BurnTime : 0f;

        public bool IsAboutToBurn => BurnProgress >= burnWarningAt;

        public override void Interact(PlayerController player)
        {
            if (!this.HasKitchenObject())
            {
                if (player.HasKitchenObject() && TryFindRecipe(player.KitchenObject.Data, out CookingRecipeSO found, out State startState))
                {
                    player.KitchenObject.SetParent(this);
                    recipe = found;
                    SetState(startState);
                    timer = KitchenObject.HeatTime;
                }
            }
            else if (!player.HasKitchenObject())
            {
                if (state == State.Cooking || state == State.Cooked)
                    KitchenObject.HeatTime = timer;
                KitchenObject.SetParent(player);
                recipe = null;
                SetState(State.Idle);
            }
            else if (state == State.Cooked && player.KitchenObject.TryGetPlate(out PlateKitchenObject plate)
                     && plate.TryAddIngredient(KitchenObject.Data))
            {
                // Serve straight from the kazan onto the plate the player is carrying.
                KitchenObject.DestroySelf();
                recipe = null;
                SetState(State.Idle);
            }
        }

        private void Update()
        {
            switch (state)
            {
                case State.Cooking:
                    timer += Time.deltaTime;
                    ProgressChanged?.Invoke(Mathf.Clamp01(timer / recipe.CookTime));
                    if (timer >= recipe.CookTime)
                    {
                        ReplaceKitchenObject(recipe.Cooked);
                        SetState(State.Cooked);
                    }
                    break;

                case State.Cooked:
                    if (!recipe.CanBurn)
                        break;

                    timer += Time.deltaTime;
                    ProgressChanged?.Invoke(Mathf.Clamp01(timer / recipe.BurnTime));
                    if (timer >= recipe.BurnTime)
                    {
                        ReplaceKitchenObject(recipe.Burned);
                        SetState(State.Burned);
                    }
                    break;

                case State.Burned:
                    timer += Time.deltaTime;
                    if (!ignited && timer >= igniteDelay && Flammable != null)
                    {
                        ignited = true;
                        Flammable.Ignite();
                    }
                    break;
            }
        }

        /// <summary>Raw input starts cooking; already-cooked food put back in keeps burning.</summary>
        private bool TryFindRecipe(KitchenObjectSO item, out CookingRecipeSO found, out State startState)
        {
            foreach (CookingRecipeSO candidate in recipes)
            {
                if (candidate.Input == item)
                {
                    found = candidate;
                    startState = State.Cooking;
                    return true;
                }
                if (candidate.Cooked == item)
                {
                    found = candidate;
                    startState = State.Cooked;
                    return true;
                }
            }

            found = null;
            startState = State.Idle;
            return false;
        }

        private void ReplaceKitchenObject(KitchenObjectSO data)
        {
            KitchenObject.DestroySelf();
            KitchenObject.Spawn(data, this);
        }

        private void SetState(State newState)
        {
            state = newState;
            timer = 0f;
            ignited = false;
            ProgressChanged?.Invoke(0f);
            StateChanged?.Invoke(state);
        }
    }
}
