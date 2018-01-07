using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public class Product
    {
        public string Name { get; set; }
        public string PropName { get; set; }
        public int Price { get; set; }
        public double Profit { get; set; }

        // vehicle attachment stuff
        public Vector3 VehOffset { get; set; }
        public Vector3 VehRotation { get; set; }

        public Product(string name, string propName, int price, double profit, Vector3 vehOffset = null, Vector3 vehRotation = null)
        {
            Name = name;
            PropName = propName;
            Price = price;
            Profit = profit;
            VehOffset = vehOffset ?? new Vector3();
            VehRotation = vehRotation ?? new Vector3();
        }
    }
}
