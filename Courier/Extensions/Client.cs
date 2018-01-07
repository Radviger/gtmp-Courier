using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public static class ClientExtensions
    {
        public static void sendErrorMessage(this Client player, string message)
        {
            API.shared.sendNativeToPlayer(player, Hash._SET_NOTIFICATION_BACKGROUND_COLOR, 6);
            API.shared.playSoundFrontEnd(player, "OTHER_TEXT", "HUD_AWARDS");
            player.sendNotification("Error", message, false);
        }

        public static bool isCarrying(this Client player)
        {
            return player.hasData("Courier_CarryingType");
        }

        // use isCarrying before this!
        public static ProductType getCarryingType(this Client player)
        {
            return (ProductType)player.getData("Courier_CarryingType");
        }

        public static void startCarrying(this Client player, ProductType type, NetHandle? objHandle = null)
        {
            if (player.hasData("Courier_CarryingType") || !ProductDefinitions.Products.ContainsKey(type)) return;

            Object attachObj = (objHandle == null) ? API.shared.createObject(API.shared.getHashKey(ProductDefinitions.Products[type].PropName), player.position, new Vector3()) : API.shared.getEntityFromHandle<Object>(objHandle.Value);
            if (attachObj == null) return;

            attachObj.attachTo(player.handle, "PH_R_Hand", Offsets.PlayerAttachmentOffsets[type].Item1, Offsets.PlayerAttachmentOffsets[type].Item2);
            player.playAnimation("anim@heists@box_carry@", "idle", (int)(AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.AllowRotation));
            player.setData("Courier_CarryingType", (int)type);
            player.setData("Courier_BoxHandle", attachObj.handle);
            player.triggerEvent("Courier_SetCarryingState", true, attachObj.model);
            API.shared.sendNativeToPlayer(player, Hash.SET_CURRENT_PED_WEAPON, player.handle, (int)WeaponHash.Unarmed, true);
        }

        public static void stopCarrying(this Client player, bool deleteObj = true)
        {
            if (!player.hasData("Courier_CarryingType")) return;

            if (deleteObj)
            {
                Object attachObj = API.shared.getEntityFromHandle<Object>(player.getData("Courier_BoxHandle"));
                attachObj?.delete();
            }

            player.stopAnimation();
            player.resetData("Courier_CarryingType");
            player.resetData("Courier_BoxHandle");
            player.triggerEvent("Courier_SetCarryingState", false, -1);
        }
    }
}
