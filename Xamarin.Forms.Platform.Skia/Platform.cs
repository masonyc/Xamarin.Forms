﻿using SkiaSharp;
using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Skia
{
	public class Platform : IPlatform
	{
		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			SizeRequest? result = null;

			if (view is Button || view is Label || view is Entry)
			{
				string text = null;
				TextDrawingData drawingData = null;
				if (view is Button button)
				{
					text = button.Text;

					drawingData = new TextDrawingData(button);
				}
				else if (view is Label label)
				{
					text = label.Text;
					drawingData = new TextDrawingData(label);
				}
				else if (view is Entry entry)
				{
					text = entry.Text;
					drawingData = new TextDrawingData(entry);
				}

				if (string.IsNullOrEmpty(text))
					text = " ";

				drawingData.Rect = new Rectangle(0, 0,
					double.IsPositiveInfinity(widthConstraint) ? float.MaxValue : widthConstraint,
					double.IsPositiveInfinity(heightConstraint) ? float.MaxValue : heightConstraint);

				Forms.GetTextLayout(text, drawingData, true, out var lines);

				Size size;
				if (lines.Count > 0)
					size = new Size(lines.Max(l => l.Width), lines.Sum(l => l.Height));
				else
					size = new Size();

				if (view is Button)
					size += new Size(10, 10);

				if (view is Entry)
					size += new Size(15, 15);

				return new SizeRequest(size);
			}
			else if (view is Image image)
			{
				if (image.Source is UriImageSource uri)
				{
					var url = uri.Uri.AbsoluteUri;
					var localPath = uri.Uri.LocalPath;
					int multiplier = 1;
					if(localPath.EndsWith("@2x.png"))
					{
						multiplier = 2;
					}
					if (localPath.EndsWith("@3x.png"))
					{
						multiplier = 3;
					}
					var bitmap = ImageCache.TryGetValue(url);
					if (bitmap != null)
					{
						return new SizeRequest(new Size(bitmap.Width/multiplier, bitmap.Height/multiplier));
					}
				}
				if(image.Source is FileImageSource fileSource)
				{
					var s = ImageCache.FromFile(fileSource.File);
					var bitmap = s.bitmap;
					int multiplier = s.scale;
					if (bitmap != null)
					{
						return new SizeRequest(new Size(bitmap.Width / multiplier, bitmap.Height / multiplier));
					}
				}
				return new SizeRequest(new Size(10, 10));
			}
			else if(view is Switch)
			{
				return new SizeRequest(new Size(81, 28));
			}
			else
				result = new SizeRequest(new Size(100, 100));

			if (result == null)
				throw new NotImplementedException();

			return result.Value;
		}
	}
}