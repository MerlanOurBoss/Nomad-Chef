using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Drop dirty plates in, press "use" once and the chef keeps washing plate after plate while
    /// standing at the sink. Each clean plate goes to the linked plate rack.
    /// </summary>
    public class SinkCounter : BaseCounter, IPlateStack, IHasProgress, IWorkStation
    {
        [SerializeField] private PlatesCounter cleanPlatesRack;
        [SerializeField, Min(0.1f)] private float washTime = 1.5f;

        private int dirtyCount;
        private float washProgress;
        private PlayerController washer;

        public event Action<int> PlateCountChanged;
        public event Action<float> ProgressChanged;
        public event Action<bool> WashingChanged;
        public event Action PlateWashed;

        public int PlateCount => dirtyCount;
        public bool IsWashing => washer != null;
        public WorkMotion Motion => WorkMotion.Scrub;

        public bool IsWorkedBy(PlayerController player) => washer == player;

        public override void Interact(PlayerController player)
        {
            if (player.KitchenObject is DirtyPlatesKitchenObject pile)
            {
                int count = pile.PlateCount;
                pile.DestroySelf();
                SetDirtyCount(dirtyCount + count);
            }
        }

        public override void InteractAlternate(PlayerController player)
        {
            if (IsWashing || dirtyCount == 0)
                return;

            washer = player;
            WashingChanged?.Invoke(true);
        }

        private void Update()
        {
            if (!IsWashing)
                return;

            if (dirtyCount == 0 || washer.SelectedCounter != this || !washer.isActiveAndEnabled)
            {
                StopWashing();
                return;
            }

            washProgress += Time.deltaTime;
            ProgressChanged?.Invoke(Mathf.Clamp01(washProgress / washTime));

            if (washProgress >= washTime)
            {
                washProgress = 0f;
                ProgressChanged?.Invoke(0f);
                SetDirtyCount(dirtyCount - 1);
                cleanPlatesRack.AddPlate();
                PlateWashed?.Invoke();
            }
        }

        private void StopWashing()
        {
            washer = null;
            WashingChanged?.Invoke(false);
        }

        private void SetDirtyCount(int count)
        {
            dirtyCount = count;
            PlateCountChanged?.Invoke(dirtyCount);
        }
    }
}
