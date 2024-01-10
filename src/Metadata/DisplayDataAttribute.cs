using System;

namespace Hangfire.Dashboard.Management.v2.Metadata
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
	public sealed class DisplayDataAttribute : Attribute
	{
		/// <summary>
		/// Label for the input control.  Defaults to the Name of the property.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Place holder text.  Defaults to the Name of the property.
		/// NOTE: This value is ignored for the following checkbox inputs.
		/// </summary>
		public string Placeholder { get; set; }

		/// <summary>
		/// Optional description for the input control.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// If this has a valid value for the input control, it will be added as the default.
		/// The type should match the parameter's type with one exception.
		/// DateTime's default should be passed a s string in the format 'mm/dd/yyyy hh:mm AM/PM'
		/// </summary>
		public object DefaultValue { get; set; }

		/// <summary>
		/// Is the input control a textarea or textbox?
		/// This only applies to string inputs.
		/// </summary>
		public bool IsMultiLine { get; set; }

		/// <summary>
		/// CSS classes that will be applied to the wrapping div of the input control.
		/// </summary>
		public string CssClasses { get; set; } = null;

		/// <summary>
		/// Should the input control be disabled on the interface?
		/// If you disable a control that needs a value, make sure to set a default value.
		/// </summary>
		public bool IsDisabled { get; set; } = false;

		/// <summary>
		/// Should the input control be required on the interface?
		/// </summary>
		public bool IsRequired { get; set; } = false;

		public DisplayDataAttribute() { }
		public DisplayDataAttribute(string label = null, string placeholder = null, string description = null, object defaultValue = null, string cssClasses = null, bool isDisabled = false, bool isRequired = false, bool isMultiline = false)
		{
			this.Label = label;
			this.Placeholder = placeholder;
			this.Description = description;
			this.DefaultValue = defaultValue;
			this.CssClasses = cssClasses;
			this.IsDisabled = isDisabled;
			this.IsRequired = isRequired;
			this.IsMultiLine = isMultiline;
		}
	}
}
