using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class Shipping
    {
        public ShippingData data { get; set; }
    }

    public class ShippingData : Data
    {
        public ShippingAttributes attributes { get; set; }
    }

    public class ShippingAttributes : Attributes
    {
        public float distance_value { get; set; }
        public string distance_unit { get; set; }
        public float weight_value { get; set; }
        public string weight_unit { get; set; }
        public string transport_method { get; set; }
    }

    public class ShippingRequest
    {
        public string type { get; set; }
        public float distance_value { get; set; }
        public string distance_unit { get; set; }
        public float weight_value { get; set; }
        public string weight_unit { get; set; }
        public string transport_method { get; set; }
    }
}
