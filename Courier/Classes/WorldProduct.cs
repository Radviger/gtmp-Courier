using System;
using GrandTheftMultiplayer.Server.API;
using Elements = GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public class WorldProduct
    {
        public Guid ID { get; set; }
        public ProductType Type { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public DateTime SpawnTime { get; set; }

        // Entities
        public Elements.Object Object { get; set; }
        public Elements.TextLabel Label { get; set; }

        public WorldProduct(ProductType type, NetHandle? objHandle, Vector3 position, Vector3 rotation)
        {
            ID = Methods.GenerateProductGuid();
            Type = type;
            Position = position;
            Rotation = rotation;
            SpawnTime = DateTime.Now;

            // Create entities
            Object = (objHandle == null) ? API.shared.createObject(API.shared.getHashKey(ProductDefinitions.Products[type].PropName), position, rotation) : API.shared.getEntityFromHandle<Elements.Object>(objHandle.Value);

            // Existing object, detach it and set position
            if (API.shared.isEntityAttachedToAnything(Object.handle))
            {
                Object.detach();
                Object.position = position;
                Object.rotation = rotation;
            }

            Object.setSyncedData("Courier_DetectorID", ID.ToString());
            API.shared.sendNativeToPlayersInRange(position, 300f, GrandTheftMultiplayer.Server.Constant.Hash.SET_ENTITY_PROOFS, Object.handle, true, true, true, true, true, true, 1, true);

            Label = API.shared.createTextLabel($"{type}", position + new Vector3(0.0, 0.0, 0.5), 10f, 0.5f, true);
        }

        public void DeleteEntities(bool deleteObj = true)
        {
            if (deleteObj) Object?.delete();
            Label?.delete();
        }
    }
}
