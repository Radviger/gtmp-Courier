using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace CourierScript
{
    public class Offsets
    {
        public static Dictionary<ProductType, Tuple<Vector3, Vector3>> PlayerAttachmentOffsets = new Dictionary<ProductType, Tuple<Vector3, Vector3>>
        {
            { ProductType.Cigarette, new Tuple<Vector3, Vector3>(new Vector3(0.0, -0.18, -0.18), new Vector3()) },
            { ProductType.Beer, new Tuple<Vector3, Vector3>(new Vector3(0.0, -0.05, 0.0), new Vector3()) },
            { ProductType.Clothes, new Tuple<Vector3, Vector3>(new Vector3(-0.02, -0.2, -0.18), new Vector3(0.0, 0.0, 90)) },
            { ProductType.Paper, new Tuple<Vector3, Vector3>(new Vector3(0.03, -0.14, 0.06), new Vector3(0.0, 0.0, 90)) }
        };

        public static Dictionary<VehicleHash, List<Tuple<Vector3, Vector3>>> VehicleAttachmentOffsets = new Dictionary<VehicleHash, List<Tuple<Vector3, Vector3>>>
        {
            // Burrito3
            {
                VehicleHash.Burrito3,

                // value
                new List<Tuple<Vector3, Vector3>>
                {
                    new Tuple<Vector3, Vector3>(new Vector3(-0.549999833, -0.549999833, -0.290000051), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.00999999326, -0.549999833, -0.290000051), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.549999833, -0.549999833, -0.290000051), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(-0.250000089, -1.39999902, -0.290000051), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.2700001, -1.39999902, -0.290000051), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(-0.010000064, -2.08999848, -0.290000051), new Vector3(0.0, 0.0, 90.0))
                }
            },

            // Rumpo
            {
                VehicleHash.Rumpo,

                // value
                new List<Tuple<Vector3, Vector3>>
                {
                    new Tuple<Vector3, Vector3>(new Vector3(-0.519999862, -0.549999833, -0.469999909), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.0199999958, -0.549999833, -0.469999909), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.549999833, -0.549999833, -0.469999909), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(-0.250000119, -1.39999902, -0.469999909), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.280000061, -1.39999902, -0.469999909), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0, -2.07999849, -0.469999909), new Vector3(0.0, 0.0, 90.0))
                }
            },

            // Speedo
            {
                VehicleHash.Speedo,

                // value
                new List<Tuple<Vector3, Vector3>>
                {
                    new Tuple<Vector3, Vector3>(new Vector3(-0.449999899, -0.499999881, -0.120000035), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.100000054, -0.499999881, -0.120000035), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.04890966, -1.29999912, -0.120000035), new Vector3(0.0, 0.0, 90.0)),
                    new Tuple<Vector3, Vector3>(new Vector3(0.04890966, -1.87999856, -0.120000035), new Vector3(0.0, 0.0, 90.0))
                }
            },

            // Youga
            {
                VehicleHash.Youga,
                
                new List<Tuple<Vector3, Vector3>>
                {
                    new Tuple<Vector3, Vector3>(new Vector3(-0.499999881, -0.190000102, -0.0100000044), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.499999851, -0.190000102, -0.0100000044), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(-0.340000033, -1.49999893, 0.170000061), new Vector3()),
                    new Tuple<Vector3, Vector3>(new Vector3(0.349999994, -1.49999893, 0.170000061), new Vector3())
                }
            }
        };
    }
}
