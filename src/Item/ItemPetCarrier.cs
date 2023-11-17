using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace PetAI
{
    public class ItemPetCarrier : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            var pet = entitySel?.Entity;
            var tameable = pet?.GetBehavior<EntityBehaviorTameable>();
            var canPickUp = Attributes["canPickUp"];
            api.Logger.Debug(JsonConvert.SerializeObject(Attributes));
            handling = EnumHandHandling.Handled;
            if (pet != null && api.Side == EnumAppSide.Server && byEntity.Controls.Sneak && canPickUp[pet.Code.Path].Exists && tameable?.cachedOwner?.Entity == byEntity && tameable?.domesticationLevel == DomesticationLevel.DOMESTICATED)
            {
                string newVariant = canPickUp[pet.Code.Path]["type"].AsString();
                if (canPickUp[pet.Code.Path]["appendTextureIndex"].AsBool())
                {
                    int textureIndex = entitySel.Entity.WatchedAttributes.GetInt("textureIndex", 0) + 1;
                    newVariant += textureIndex;
                }
                slot.Itemstack = new ItemStack(byEntity.World.GetItem(slot.Itemstack.Item.CodeWithVariant("type", newVariant)).Id,
                                        EnumItemClass.Item,
                                        slot.Itemstack.StackSize,
                                        (TreeAttribute)slot.Itemstack.Attributes,
                                        byEntity.World);
                slot.Itemstack.Attributes.MergeTree(PetUtil.EntityToTree(pet));
                byEntity.Api.Logger.Debug(slot.Itemstack.Attributes.GetString("class"));
                entitySel.Entity.Die(EnumDespawnReason.PickedUp);
                slot.MarkDirty();
            }
            else if (api.Side == EnumAppSide.Server && byEntity.Controls.Sneak && !(Variant["type"] == "empty"))
            {
                byEntity.Api.Logger.Debug(slot.Itemstack.Attributes.GetString("class"));
                pet = PetUtil.EntityFromTree(slot.Itemstack.Attributes, byEntity.World);
                pet.ServerPos.SetPos(byEntity.ServerPos);
                byEntity.World.SpawnEntity(pet);
                slot.Itemstack = new ItemStack(byEntity.World.GetItem(slot.Itemstack.Item.CodeWithVariant("type", "empty")).Id,
                                        EnumItemClass.Item,
                                        slot.Itemstack.StackSize,
                                        (TreeAttribute)slot.Itemstack.Attributes,
                                        byEntity.World);
                slot.MarkDirty();
            }
            else
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }
    }
}