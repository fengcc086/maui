﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Maui
{
	internal struct ButtonContentLayout
	{
		public enum ImagePosition
		{
			Left,
			Top,
			Right,
			Bottom
		}

		public ButtonContentLayout(ImagePosition position, double spacing)
		{
			Position = position;
			Spacing = spacing;
		}

		public ImagePosition Position { get; }

		public double Spacing { get; }

		public override string ToString() => $"Image Position = {Position}, Spacing = {Spacing}";

		public bool IsHorizontal() =>
			Position == ImagePosition.Left ||
			Position == ImagePosition.Right;
	}
}
