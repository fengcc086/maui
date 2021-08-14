using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this UIButton nativeButton, IButton button) =>
			nativeButton.SetTitle(button.Text, UIControlState.Normal);

		public static void UpdateTextColor(this UIButton nativeButton, IButton button) =>
			nativeButton.UpdateTextColor(button);

		public static void UpdateTextColor(this UIButton nativeButton, IButton button, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{

			if (button.TextColor == null)
			{
				nativeButton.SetTitleColor(buttonTextColorDefaultNormal, UIControlState.Normal);
				nativeButton.SetTitleColor(buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
				nativeButton.SetTitleColor(buttonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var color = button.TextColor.ToNative();

				nativeButton.SetTitleColor(color, UIControlState.Normal);
				nativeButton.SetTitleColor(color, UIControlState.Highlighted);
				nativeButton.SetTitleColor(color, UIControlState.Disabled);

				nativeButton.TintColor = color;
			}
		}

		public static void UpdateCharacterSpacing(this UIButton nativeButton, ITextStyle textStyle)
		{
			nativeButton.TitleLabel.UpdateCharacterSpacing(textStyle);
		}

		public static void UpdateFont(this UIButton nativeButton, ITextStyle textStyle, IFontManager fontManager)
		{
			nativeButton.TitleLabel.UpdateFont(textStyle, fontManager, UIFont.ButtonFontSize);
		}

		public static void UpdatePadding(this UIButton nativeButton, IButton button)
		{
			nativeButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)button.Padding.Top,
				(float)button.Padding.Left,
				(float)button.Padding.Bottom,
				(float)button.Padding.Right);
		}

		public static void UpdateContentLayout(this UIButton nativeButton, IButton button)
		{
			if (nativeButton.Bounds.Width == 0 || 
				button is not IButtonContentLayout bcl)
			{
				return;
			}

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var layout = bcl.ContentLayout;
			var spacing = (nfloat)layout.Spacing;

			var image = nativeButton.CurrentImage;
			if (image != null && !string.IsNullOrEmpty(nativeButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				var title =
					nativeButton.CurrentAttributedTitle ??
					new NSAttributedString(nativeButton.CurrentTitle, new UIStringAttributes { Font = nativeButton.TitleLabel.Font });

				var titleRect = title.GetBoundingRect(
					nativeButton.Bounds.Size,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
					null);

				var titleWidth = titleRect.Width;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = nativeButton.Bounds.Width;
				var buttonHeight = nativeButton.Bounds.Height;

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidth / 2;
				imageInsets.Right -= titleWidth / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top ||
					layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					int invert = (layout.Position == ButtonContentLayout.ImagePosition.Top) ?
						1 : -1;

					imageInsets.Top -= invert * (buttonHeight - imageHeight) / 2;
					imageInsets.Bottom += invert * (buttonHeight - imageHeight) / 2;

					titleInsets.Top += invert * (imageHeight / 2 + spacing);
					titleInsets.Bottom -= invert * imageHeight / 2;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left ||
					layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					int invert = (layout.Position == ButtonContentLayout.ImagePosition.Left) ?
						1 : -1;

					imageInsets.Left -= invert * (buttonWidth - imageWidth) / 2;
					imageInsets.Right += invert * (buttonWidth - imageWidth) / 2;

					titleInsets.Left += invert * (imageWidth / 2 + spacing);
					titleInsets.Right -= invert * imageWidth / 2;
				}
			}

			nativeButton.ImageEdgeInsets = imageInsets;
			nativeButton.TitleEdgeInsets = titleInsets;
		}
	}
}