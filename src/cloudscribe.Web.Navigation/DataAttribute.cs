using System;

namespace cloudscribe.Web.Navigation
{
    [Serializable()]
    public class DataAttribute
    {
        public string Attribute { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
