using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(FlowDirectionConverter))]
	public enum FlowDirection
	{
		MatchParent = 0,
		LeftToRight = 1,
		RightToLeft = 2,
	}

	[Xaml.TypeConversion(typeof(FlowDirection))]
	public class FlowDirectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				if (Enum.TryParse(strValue, out FlowDirection direction))
					return direction;

				if (strValue.Equals("ltr", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.LeftToRight;
				if (strValue.Equals("rtl", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.RightToLeft;
				if (strValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.MatchParent;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(FlowDirection)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not FlowDirection direction)
				throw new NotSupportedException();
			return direction.ToString();
		}
	}
}