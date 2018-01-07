using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using Newtonsoft.Json;

namespace CourierScript
{
    public class LoadedProduct
    {
        public string Name { get; set; }

        [JsonIgnore]
        public ProductType Type { get; set; }

        [JsonIgnore]
        public Object Object => API.shared.getEntityFromHandle<Object>(_objHandle);

        [JsonIgnore]
        private NetHandle _objHandle;

        public LoadedProduct(ProductType type, NetHandle objHandle)
        {
            Name = type.ToString();
            Type = type;
            _objHandle = objHandle;
        }
    }
}
