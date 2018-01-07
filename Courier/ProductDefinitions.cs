using System.Collections.Generic;
using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public class ProductDefinitions
    {
        public static Dictionary<ProductType, Product> Products = new Dictionary<ProductType, Product>
        {
            { ProductType.Cigarette, new Product("Cigarettes", "prop_cardbordbox_02a", 60, 1.15) }, // 15% profit - 1.15
            { ProductType.Beer, new Product("Beer", "v_ret_ml_beerpis1", 45, 1.15, new Vector3(0.0, 0.0, 0.15)) }, // 15% profit - 1.15
            { ProductType.Clothes, new Product("Clothes", "prop_tshirt_box_01", 80, 1.25, vehRotation: new Vector3(0.0, 0.0, 90.0)) }, // 25% profit - 1.25
            { ProductType.Paper, new Product("Paper", "prop_paper_box_04", 30, 1.15, new Vector3(0.0, 0.0, 0.195)) } // 15% profit - 1.15
        };
    }
}
