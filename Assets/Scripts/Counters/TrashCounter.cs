using NomadChef.KitchenObjects;
using NomadChef.Player;

namespace NomadChef.Counters
{
    /// <summary>Destroys food the player is holding. Plates are kept and only emptied; tools are refused.</summary>
    public class TrashCounter : BaseCounter
    {
        public override void Interact(PlayerController player)
        {
            if (!player.HasKitchenObject())
                return;

            if (player.KitchenObject.TryGetPlate(out PlateKitchenObject plate))
                plate.ClearIngredients();
            else if (player.KitchenObject.Data.CanBeTrashed)
                player.KitchenObject.DestroySelf();
        }

        public override bool TryCatch(KitchenObject item)
        {
            item.DestroySelf();
            return true;
        }
    }
}
