using System;
using System.Linq;

namespace CourierScript
{
    public class Methods
    {
        public static int GetFinalPrice(ProductType productType)
        {
            return (ProductDefinitions.Products.ContainsKey(productType)) ? (int)Math.Floor(ProductDefinitions.Products[productType].Price * ProductDefinitions.Products[productType].Profit) : 0;
        }

        public static int GetBuyerFullness(int stock, int maxStock)
        {
            return (stock * 100) / maxStock;
        }

        public static string GetBuyerFullnessColor(int stock, int maxStock)
        {
            int percentage = (stock * 100) / maxStock;

            if (percentage <= 25)
            {
                return "~g~";
            }
            else if (percentage >= 26 && percentage <= 50)
            {
                return "~y~";
            }
            else if (percentage >= 51 && percentage <= 75)
            {
                return "~o~";
            }
            else
            {
                return "~r~";
            }
        }

        public static Buyer GetBuyer(Guid ID)
        {
            return Main.Buyers.FirstOrDefault(b => b.ID == ID);
        }

        public static Factory GetFactory(Guid ID)
        {
            return Main.Factories.FirstOrDefault(f => f.ID == ID);
        }

        public static Guid GenerateProductGuid()
        {
            Guid generated;

            do
            {
                generated = Guid.NewGuid();
            } while (Main.DroppedProduct.FirstOrDefault(p => p.ID == generated) != null);

            return generated;
        }
    }
}
