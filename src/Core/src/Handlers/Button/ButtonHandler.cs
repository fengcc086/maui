#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler
	{
		readonly ImageSourceServiceResultManager _sourceManager = new ImageSourceServiceResultManager();

		public static PropertyMapper<IButton, ButtonHandler> ButtonMapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
			[nameof(IButton.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IButton.Font)] = MapFont,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButton.Text)] = MapText,
			[nameof(IButton.TextColor)] = MapTextColor,
			[nameof(IButton.ImageSource)] = MapImageSource,
			[nameof(IButtonContentLayout.ContentLayout)] = MapImageSource,
		};

		public ButtonHandler() : base(ButtonMapper)
		{

		}

		public ButtonHandler(PropertyMapper? mapper = null) : base(mapper ?? ButtonMapper)
		{
		}
	}
}
