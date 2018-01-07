using System;
using System.IO;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;

namespace CourierScript
{
    public class AdminCommands : Script
    {
        [Command("cfactory")]
        public void CMD_CreateFactory(Client player, ProductType type)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            // generate id
            Guid factoryID;

            do
            {
                factoryID = Guid.NewGuid();
            } while (Main.Factories.FirstOrDefault(f => f.ID == factoryID) != null);

            // create factory & save
            Factory newFactory = new Factory(factoryID, player.position, type);
            Main.Factories.Add(newFactory);

            newFactory.CreateEntities();
            newFactory.Save();
        }

        [Command("cbuyer")]
        public void CMD_CreateBuyer(Client player, ProductType type, int maxStock)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            // generate id
            Guid buyerID;

            do
            {
                buyerID = Guid.NewGuid();
            } while (Main.Buyers.FirstOrDefault(b => b.ID == buyerID) != null);

            // create buyer & save
            Buyer newBuyer = new Buyer(buyerID, player.position, type, maxStock: maxStock);
            Main.Buyers.Add(newBuyer);

            newBuyer.CreateEntities();
            newBuyer.Save();
        }

        [Command("setfactorystock")]
        public void CMD_SetFactoryStock(Client player, int newStock)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            if (!player.hasData("Courier_MarkerID")) return;

            Factory factory = Main.Factories.FirstOrDefault(f => f.ID == player.getData("Courier_MarkerID"));
            if (factory == null)
            {
                player.sendChatMessage("~r~ERROR: ~w~You're not in a factory's marker.");
                return;
            }

            factory.Stock = newStock;
            factory.Save(true);
        }

        [Command("setbuyerstock")]
        public void CMD_SetBuyerStock(Client player, int newStock)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            if (!player.hasData("Courier_MarkerID")) return;

            Buyer buyer = Main.Buyers.FirstOrDefault(b => b.ID == player.getData("Courier_MarkerID"));
            if (buyer == null)
            {
                player.sendChatMessage("~r~ERROR: ~w~You're not in a buyer's marker.");
                return;
            }

            buyer.Stock = newStock;
            buyer.Save(true);
        }

        [Command("setbuyermaxstock")]
        public void CMD_SetBuyerMaxStock(Client player, int newMaxStock)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            if (!player.hasData("Courier_MarkerID")) return;

            Buyer buyer = Main.Buyers.FirstOrDefault(b => b.ID == player.getData("Courier_MarkerID"));
            if (buyer == null)
            {
                player.sendChatMessage("~r~ERROR: ~w~You're not in a buyer's marker.");
                return;
            }

            buyer.MaxStock = newMaxStock;
            buyer.Save(true);
        }

        [Command("rfactory")]
        public void CMD_RemoveFactory(Client player)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            if (!player.hasData("Courier_MarkerID")) return;

            Factory factory = Main.Factories.FirstOrDefault(f => f.ID == player.getData("Courier_MarkerID"));
            if (factory == null)
            {
                player.sendChatMessage("~r~ERROR: ~w~You're not in a factory's marker.");
                return;
            }

            factory.DeleteEntities();
            Main.Factories.Remove(factory);
            if (File.Exists(factory.FilePath)) File.Delete(factory.FilePath);
        }

        [Command("rbuyer")]
        public void CMD_RemoveBuyer(Client player)
        {
            if (API.getPlayerAclGroup(player) != "Admin")
            {
                player.sendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                return;
            }

            if (!player.hasData("Courier_MarkerID")) return;

            Buyer buyer = Main.Buyers.FirstOrDefault(b => b.ID == player.getData("Courier_MarkerID"));
            if (buyer == null)
            {
                player.sendChatMessage("~r~ERROR: ~w~You're not in a buyer's marker.");
                return;
            }

            buyer.DeleteEntities();
            Main.Buyers.Remove(buyer);
            if (File.Exists(buyer.FilePath)) File.Delete(buyer.FilePath);
        }
    }
}
