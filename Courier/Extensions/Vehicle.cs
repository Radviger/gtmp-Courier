using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public static class VehicleExtensions
    {
        private static void vehicleProductsCheck(this Vehicle vehicle)
        {
            int loadedItems = Main.LoadedProducts[vehicle.handle].Count(p => p != null); // count non-null items
            if (loadedItems < 1) Main.LoadedProducts.Remove(vehicle.handle); // remove vehicle from dictionary if it has no products
        }

        public static LoadedProduct getLoadedProduct(this Vehicle vehicle, int index)
        {
            if (!Main.LoadedProducts.ContainsKey(vehicle.handle)) return null;
            if (index < 0 || index >= Main.LoadedProducts[vehicle.handle].Length) return null;
            return Main.LoadedProducts[vehicle.handle][index];
        }

        public static LoadedProduct[] getLoadedProducts(this Vehicle vehicle)
        {
            if (!Offsets.VehicleAttachmentOffsets.ContainsKey((VehicleHash)vehicle.model)) return null;
            if (!Main.LoadedProducts.ContainsKey(vehicle.handle)) Main.LoadedProducts.Add(vehicle.handle, new LoadedProduct[ Offsets.VehicleAttachmentOffsets[(VehicleHash)vehicle.model].Count ]);
            return Main.LoadedProducts[vehicle.handle];
        }

        public static bool loadProduct(this Vehicle vehicle, int index, ProductType type, NetHandle? objHandle = null)
        {
            VehicleHash vehicleModel = (VehicleHash)vehicle.model;

            if (!Offsets.VehicleAttachmentOffsets.ContainsKey(vehicleModel)) return false;
            if (!Main.LoadedProducts.ContainsKey(vehicle.handle)) Main.LoadedProducts.Add(vehicle.handle, new LoadedProduct[ Offsets.VehicleAttachmentOffsets[vehicleModel].Count ]);

            if (index < 0 || index >= Main.LoadedProducts[vehicle.handle].Length) return false;
            if (Main.LoadedProducts[vehicle.handle][index] != null) return false;

            NetHandle handle = (objHandle == null) ? API.shared.createObject(API.shared.getHashKey(ProductDefinitions.Products[type].PropName), vehicle.position, new Vector3()) : objHandle.Value;
            Main.LoadedProducts[vehicle.handle][index] = new LoadedProduct(type, handle);

            Main.LoadedProducts[vehicle.handle][index].Object.resetSyncedData("Courier_DetectorID");
            Main.LoadedProducts[vehicle.handle][index].Object.attachTo(
                vehicle.handle,
                null,
                Offsets.VehicleAttachmentOffsets[vehicleModel][index].Item1 + ProductDefinitions.Products[type].VehOffset,
                Offsets.VehicleAttachmentOffsets[vehicleModel][index].Item1 + ProductDefinitions.Products[type].VehRotation
            );

            return true;
        }

        public static bool unloadProduct(this Vehicle vehicle, Client toPlayer, int index)
        {
            if (!Main.LoadedProducts.ContainsKey(vehicle.handle)) return false;
            if (index < 0 || index >= Main.LoadedProducts[vehicle.handle].Length) return false;
            if (toPlayer.isCarrying()) return false;
            if (Main.LoadedProducts[vehicle.handle][index] == null) return false;

            Main.LoadedProducts[vehicle.handle][index].Object.detach();
            toPlayer.startCarrying(Main.LoadedProducts[vehicle.handle][index].Type, Main.LoadedProducts[vehicle.handle][index].Object.handle);
            Main.LoadedProducts[vehicle.handle][index] = null;

            vehicleProductsCheck(vehicle);
            return true;
        }

        public static void removeLoadedProduct(this Vehicle vehicle, int index)
        {
            if (!Main.LoadedProducts.ContainsKey(vehicle.handle)) return;
            if (index < 0 || index >= Main.LoadedProducts[vehicle.handle].Length) return;

            Main.LoadedProducts[vehicle.handle][index].Object?.delete();
            Main.LoadedProducts[vehicle.handle][index] = null;
            vehicleProductsCheck(vehicle);
        }
    }
}
