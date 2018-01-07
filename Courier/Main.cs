using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using Newtonsoft.Json;

namespace CourierScript
{
    public class Main : Script
    {
        // You can change these from meta.xml
        public static int SaveInterval = 120;
        public static int WorldPropLife = 10;
        public static int FactoryProductInterval = 180;
        public static int FactoryMaxAutoStock = 10;
        public static int BuyerSaleInterval = 180;
        public static int BuyerMinStockPercentage = 50;

        // Don't touch these
        public static string ResourceFolder;
        public static List<Factory> Factories = new List<Factory>();
        public static List<Buyer> Buyers = new List<Buyer>();
        public static List<WorldProduct> DroppedProduct = new List<WorldProduct>();
        public static Dictionary<NetHandle, LoadedProduct[]> LoadedProducts = new Dictionary<NetHandle, LoadedProduct[]>();
        public Timer WorldCleaner = null;

        public Main()
        {
            API.onResourceStart += Courier_Init;
            API.onPlayerDisconnected += Courier_PlayerLeave;
            API.onPlayerDeath += Courier_PlayerDeath;
            API.onVehicleDeath += Courier_VehicleDeath;
            API.onClientEventTrigger += Courier_EventTrigger;
            API.onResourceStop += Courier_Exit;
        }

        #region Events
        public void Courier_Init()
        {
            ResourceFolder = API.getResourceFolder();

            // Load settings
            if (API.hasSetting("saveInterval")) SaveInterval = API.getSetting<int>("saveInterval");
            if (API.hasSetting("worldPropLife")) WorldPropLife = API.getSetting<int>("worldPropLife");
            if (API.hasSetting("factoryProductInterval")) FactoryProductInterval = API.getSetting<int>("factoryProductInterval");
            if (API.hasSetting("factoryMaxAutoStock")) FactoryMaxAutoStock = API.getSetting<int>("factoryMaxAutoStock");
            if (API.hasSetting("buyerSaleInterval")) BuyerSaleInterval = API.getSetting<int>("buyerSaleInterval");
            if (API.hasSetting("buyerMinStockPercentage")) BuyerMinStockPercentage = API.getSetting<int>("buyerMinStockPercentage");

            // Print settings
            API.consoleOutput("-> Save Interval: {0}", TimeSpan.FromSeconds(SaveInterval).ToString(@"hh\:mm\:ss"));
            API.consoleOutput("-> Dropped Item Lifetime: {0}", TimeSpan.FromMinutes(WorldPropLife).ToString(@"hh\:mm\:ss"));
            API.consoleOutput("-> Factory Product Interval: {0}", TimeSpan.FromSeconds(FactoryProductInterval).ToString(@"hh\:mm\:ss"));
            API.consoleOutput("-> Factory Max. Auto Stock: {0}", FactoryMaxAutoStock);
            API.consoleOutput("-> Buyer Sale Interval: {0}", TimeSpan.FromSeconds(BuyerSaleInterval).ToString(@"hh\:mm\:ss"));
            API.consoleOutput("-> Buyer Min. Stock Percentage: {0}%", BuyerMinStockPercentage);

            // Load factories
            string factoryDir = ResourceFolder + Path.DirectorySeparatorChar + "FactoryData";
            if (!Directory.Exists(factoryDir)) Directory.CreateDirectory(factoryDir);

            foreach (string file in Directory.EnumerateFiles(factoryDir, "*.json"))
            {
                Factory newFactory = JsonConvert.DeserializeObject<Factory>(File.ReadAllText(file));
                Factories.Add(newFactory);

                newFactory.CreateEntities();
            }

            API.consoleOutput("Loaded {0} factories.", Factories.Count);

            // Load buyers
            string buyerDir = ResourceFolder + Path.DirectorySeparatorChar + "BuyerData";
            if (!Directory.Exists(buyerDir)) Directory.CreateDirectory(buyerDir);

            foreach (string file in Directory.EnumerateFiles(buyerDir, "*.json"))
            {
                Buyer newBuyer = JsonConvert.DeserializeObject<Buyer>(File.ReadAllText(file));
                Buyers.Add(newBuyer);

                newBuyer.CreateEntities();
            }

            API.consoleOutput("Loaded {0} buyers.", Buyers.Count);

            // Start world cleaner
            WorldCleaner = API.startTimer(60000, false, () =>
            {
                foreach (WorldProduct product in DroppedProduct)
                {
                    if (DateTime.Now.Subtract(product.SpawnTime).TotalMinutes >= WorldPropLife) product.DeleteEntities();
                }

                DroppedProduct.RemoveAll(p => DateTime.Now.Subtract(p.SpawnTime).TotalMinutes >= WorldPropLife);
            });
        }

        public void Courier_PlayerDeath(Client player, NetHandle killer, int reason)
        {
            if (player.isCarrying()) player.stopCarrying(true);
        }

        public void Courier_EventTrigger(Client player, string eventName, params object[] args)
        {
            switch (eventName)
            {
                case "Courier_RequestVehicleProducts":
                {
                    if (args.Length < 1) return;

                    NetHandle vehHandle = (NetHandle)args[0];
                    Vehicle vehicle = API.getEntityFromHandle<Vehicle>(vehHandle);
                    if (vehicle == null)
                    {
                        player.sendErrorMessage("Invalid vehicle.");
                        return;
                    }

                    player.triggerEvent("Courier_ReceiveVehicleProducts", API.toJson(vehicle.getLoadedProducts()));
                    break;
                }

                case "Courier_LoadToVehicle":
                {
                    if (args.Length < 2) return;

                    if (!player.isCarrying())
                    {
                        player.sendErrorMessage("You're not carrying a product.");
                        return;
                    }

                    NetHandle vehHandle = (NetHandle)args[0];
                    Vehicle vehicle = API.getEntityFromHandle<Vehicle>(vehHandle);
                    if (vehicle == null)
                    {
                        player.sendErrorMessage("Invalid vehicle.");
                        return;
                    }

                    if (player.position.DistanceTo(vehicle.position) > 5f)
                    {
                        player.sendErrorMessage("Vehicle is too far away.");
                        return;
                    }

                    int idx = Convert.ToInt32(args[1]);
                    if (vehicle.loadProduct(idx, player.getCarryingType(), (NetHandle?)player.getData("Courier_BoxHandle")))
                    {
                        player.stopCarrying(false);
                    }
                    else
                    {
                        player.sendErrorMessage("Couldn't load your product to the vehicle.");
                    }

                    break;
                }

                case "Courier_TakeFromVehicle":
                {
                    if (args.Length < 2) return;

                    if (player.isCarrying())
                    {
                        player.sendErrorMessage("You're already carrying a product.");
                        return;
                    }

                    NetHandle vehHandle = (NetHandle)args[0];
                    Vehicle vehicle = API.getEntityFromHandle<Vehicle>(vehHandle);
                    if (vehicle == null)
                    {
                        player.sendErrorMessage("Invalid vehicle.");
                        return;
                    }

                    if (player.position.DistanceTo(vehicle.position) > 5f)
                    {
                        player.sendErrorMessage("Vehicle is too far away.");
                        return;
                    }

                    int idx = Convert.ToInt32(args[1]);
                    if (!vehicle.unloadProduct(player, idx))
                    {
                        player.sendErrorMessage("Couldn't take the product from the vehicle.");
                        return;
                    }

                    break;
                }

                case "Courier_Drop":
                {
                    if (args.Length < 1) return;

                    if (!player.isCarrying())
                    {
                        player.sendErrorMessage("You're not carrying a product.");
                        return;
                    }

                    Vector3 position = (Vector3)args[0];
                    if (player.position.DistanceTo(position) >= 5)
                    {
                        player.sendErrorMessage("You can't drop your product.");
                        return;
                    }

                    DroppedProduct.Add(new WorldProduct(player.getCarryingType(), (NetHandle?)player.getData("Courier_BoxHandle"), position, player.rotation));
                    player.stopCarrying(false);
                    break;
                }

                case "Courier_Take":
                {
                    if (args.Length < 1) return;

                    if (player.isCarrying())
                    {
                        player.sendErrorMessage("You already have a product.");
                        return;
                    }

                    Guid productID = new Guid(args[0].ToString());
                    WorldProduct product = DroppedProduct.FirstOrDefault(p => p.ID == productID);
                    if (product == null)
                    {
                        player.sendErrorMessage("Invalid product.");
                        return;
                    }

                    product.DeleteEntities(false);
                    product.Object.resetSyncedData("Courier_DetectorID");
                    player.startCarrying(product.Type, product.Object.handle);
                    DroppedProduct.Remove(product);
                    break;
                }

                case "Courier_FactoryBuy":
                {
                    if (player.isCarrying())
                    {
                        player.sendErrorMessage("You already have a product.");
                        return;
                    }

                    if (!player.hasData("Courier_MarkerID"))
                    {
                        player.sendErrorMessage("You're not in a factory's marker.");
                        return;
                    }

                    Factory factory = Methods.GetFactory(player.getData("Courier_MarkerID"));
                    if (factory == null)
                    {
                        player.sendErrorMessage("Invalid factory.");
                        return;
                    }

                    if (API.exported.MoneyAPI.GetMoney(player) < ProductDefinitions.Products[factory.Type].Price)
                    {
                        player.sendErrorMessage("You can't afford this product.");
                        return;
                    }

                    if (factory.Stock < 1)
                    {
                        player.sendErrorMessage("This factory has no product left.");
                        return;
                    }

                    API.exported.MoneyAPI.ChangeMoney(player, -ProductDefinitions.Products[factory.Type].Price);
                    player.startCarrying(factory.Type);

                    factory.Stock--;
                    factory.Save();
                    break;
                }

                case "Courier_FactoryRefund":
                {
                    if (!player.isCarrying())
                    {
                        player.sendErrorMessage("You're not carrying a product.");
                        return;
                    }

                    if (!player.hasData("Courier_MarkerID"))
                    {
                        player.sendErrorMessage("You're not in a factory's marker.");
                        return;
                    }

                    Factory factory = Methods.GetFactory(player.getData("Courier_MarkerID"));
                    if (factory == null)
                    {
                        player.sendErrorMessage("Invalid factory.");
                        return;
                    }

                    if (factory.Type != player.getCarryingType())
                    {
                        player.sendErrorMessage("This factory doesn't refund the product you have.");
                        return;
                    }

                    API.exported.MoneyAPI.ChangeMoney(player, ProductDefinitions.Products[factory.Type].Price);
                    player.stopCarrying();

                    factory.Stock++;
                    factory.Save();
                    break;
                }

                case "Courier_BuyerSell":
                {
                    if (!player.isCarrying())
                    {
                        player.sendErrorMessage("You're not carrying a product!");
                        return;
                    }

                    if (!player.hasData("Courier_MarkerID"))
                    {
                        player.sendErrorMessage("You're not in a buyer's marker.");
                        return;
                    }

                    Buyer buyer = Methods.GetBuyer(player.getData("Courier_MarkerID"));
                    if (buyer == null)
                    {
                        player.sendErrorMessage("Invalid buyer.");
                        return;
                    }

                    if (buyer.Type != player.getCarryingType())
                    {
                        player.sendErrorMessage("This buyer doesn't buy the product you're selling.");
                        return;
                    }

                    if ((buyer.Stock + 1) > buyer.MaxStock)
                    {
                        player.sendErrorMessage("This buyer can't accept any more product.");
                        return;
                    }

                    API.exported.MoneyAPI.ChangeMoney(player, Methods.GetFinalPrice(player.getCarryingType()));
                    player.stopCarrying();

                    buyer.Stock++;
                    buyer.Save();
                    break;
                }
            }
        }

        public void Courier_PlayerLeave(Client player, string reason)
        {
            if (player.isCarrying()) player.stopCarrying(true);
        }

        public void Courier_VehicleDeath(NetHandle vehicle)
        {
            Vehicle veh = API.getEntityFromHandle<Vehicle>(vehicle);
            if (veh == null) return;

            if (LoadedProducts.ContainsKey(vehicle))
            {
                for (int i = 0; i < LoadedProducts[vehicle].Length; i++) LoadedProducts[vehicle][i]?.Object?.delete();
                LoadedProducts.Remove(vehicle);
            }
        }

        public void Courier_Exit()
        {
            foreach (Client player in API.getAllPlayers().Where(p => p.isCarrying())) player.stopCarrying();

            foreach (Factory factory in Factories)
            {
                factory.DeleteEntities();
                factory.Save(true);
            }

            foreach (Buyer buyer in Buyers)
            {
                buyer.DeleteEntities();
                buyer.Save(true);
            }

            foreach (WorldProduct product in DroppedProduct) product.DeleteEntities();

            foreach (LoadedProduct[] loadedProduct in LoadedProducts.Values)
            {
                for (int i = 0; i < loadedProduct.Length; i++) loadedProduct[i]?.Object?.delete();
            }

            Factories.Clear();
            Buyers.Clear();
            DroppedProduct.Clear();
            LoadedProducts.Clear();
            if (WorldCleaner != null) API.stopTimer(WorldCleaner);
        }
        #endregion
    }
}
