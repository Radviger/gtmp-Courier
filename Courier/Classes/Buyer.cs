using System;
using System.IO;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using Newtonsoft.Json;

namespace CourierScript
{
    public class Buyer
    {
        public Guid ID { get; set; }
        public Vector3 Position { get; set; }
        public ProductType Type { get; set; }

        [JsonIgnore]
        private string LabelText => $"~g~{Type} Buyer~n~~n~~w~Unit Price: ~g~${Methods.GetFinalPrice(Type):n0}~n~~w~Capacity: {Methods.GetBuyerFullnessColor(Stock, MaxStock)}{Stock:n0}/{MaxStock:n0}";

        [JsonIgnore]
        public string FilePath => Main.ResourceFolder + Path.DirectorySeparatorChar + "BuyerData" + Path.DirectorySeparatorChar + ID + ".json";

        [JsonIgnore]
        private int _stock;

        public int Stock
        {
            get { return _stock; }

            set
            {
                _stock = value;
                if (Label != null) Label.text = LabelText;

                // Start "selling" product when stock reaches full (will stop when buyer has less than X% stock)
                if (_stock >= MaxStock && SaleTimer == null)
                {
                    SaleTimer = API.shared.startTimer(Main.BuyerSaleInterval * 1000, false, () =>
                    {
                        Stock--;
                        if (Methods.GetBuyerFullness(Stock, MaxStock) < Main.BuyerMinStockPercentage) KillTimer();
                    });
                }
            }
        }

        [JsonIgnore]
        private int _maxStock;

        public int MaxStock
        {
            get { return _maxStock; }

            set
            {
                _maxStock = value;
                if (Label != null) Label.text = LabelText;
            }
        }

        [JsonIgnore]
        private DateTime _lastSave;

        [JsonIgnore]
        public Blip Blip { get; set; }

        [JsonIgnore]
        public Marker Marker { get; set; }

        [JsonIgnore]
        public TextLabel Label { get; set; }

        [JsonIgnore]
        public ColShape ColShape { get; set; }

        [JsonIgnore]
        private Timer SaleTimer { get; set; }

        public Buyer(Guid id, Vector3 position, ProductType type, int stock = 0, int maxStock = 100)
        {
            ID = id;
            Position = position;
            Type = type;
            MaxStock = maxStock;
            Stock = stock;
        }

        public void CreateEntities()
        {
            // blip
            Blip = API.shared.createBlip(Position);
            Blip.name = $"{Type} Buyer";
            Blip.sprite = 500;
            Blip.color = 11;
            Blip.shortRange = true;
            Blip.scale = 1f;

            // marker
            Marker = API.shared.createMarker(1, Position - new Vector3(0.0, 0.0, 1.0), new Vector3(), new Vector3(), new Vector3(3.0, 3.0, 1.0), 150, 146, 208, 171);

            // label
            Label = API.shared.createTextLabel(LabelText, Position + new Vector3(0.0, 0.0, 0.5), 20f, 0.5f);

            // colshape
            ColShape = API.shared.createCylinderColShape(Position, 1.5f, 2f);
            ColShape.onEntityEnterColShape += (shape, entityHandle) =>
            {
                Client player = null;

                if ((player = API.shared.getPlayerFromHandle(entityHandle)) != null)
                {
                    player.setData("Courier_MarkerID", ID);
                    player.triggerEvent("Courier_SetMarkerType", (int)MarkerType.Buyer);
                }
            };

            ColShape.onEntityExitColShape += (shape, entityHandle) =>
            {
                Client player = null;

                if ((player = API.shared.getPlayerFromHandle(entityHandle)) != null)
                {
                    player.resetData("Courier_MarkerID");
                    player.triggerEvent("Courier_SetMarkerType", (int)MarkerType.None);
                }
            };
        }

        private void KillTimer()
        {
            if (SaleTimer != null) API.shared.stopTimer(SaleTimer);
            SaleTimer = null;
        }

        public void DeleteEntities()
        {
            Blip?.delete();
            Marker?.delete();
            Label?.delete();
            if (ColShape != null) API.shared.deleteColShape(ColShape);
            KillTimer();
        }

        public void Save(bool force = false)
        {
            if (!force && DateTime.Now.Subtract(_lastSave).TotalSeconds < Main.SaveInterval) return;

            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            _lastSave = DateTime.Now;
        }
    }
}
