using System;

namespace Hangfire.Dashboard.Management.v2Unofficial.Metadata
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class DisplayDataAttribute : Attribute
    {
        public string LabelText { get; set; }
        public string PlaceholderText { get; set; }
        public string DescriptionText { get; set; }
        public object DefaultValue { get; set; }
        public bool IsMultiLine { get; set; }
        public Type ConvertType { get; set; }

        public string Css { get; set; } = null;

        public bool IsDisabled { get; set; } = false;
        public DisplayDataAttribute(string labelText = null, string placeholderText = null, string descriptionText = null, object defaultValue = null, string css = null, bool isControlDisabled = false)
        {
            this.LabelText = labelText;
            this.PlaceholderText = placeholderText;
            this.DescriptionText = descriptionText;
            this.DefaultValue = defaultValue;
            this.Css = css;
            this.IsDisabled = isControlDisabled;
        }
    }
}