using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;
using GamiAutoClicker.WindowManager;

namespace GamiAutoClicker;

internal static class Utilities {

	public static void OpenSettingsWindow(object _, RoutedEventArgs __) {
		ThemeHelper.CreateWindow(WindowKey.Settings);
	}



	//Yoinked from Windows Community Toolkit
	public static async Task<ImageBrush> BitmapToBrushAsync(
	byte[] bitmap,
	int width,
	int height) {
		var writableBitmap = new WriteableBitmap(width, height);
		using (Stream stream = writableBitmap.PixelBuffer.AsStream()) {
			await stream.WriteAsync(bitmap).ConfigureAwait(true);
		}

		var brush = new ImageBrush() {
			ImageSource = writableBitmap,
			Stretch = Stretch.None
		};

		return brush;
	}
	//Yoinked from Windows Community Toolkit

	internal static readonly Color CheckerBackgroundColor = Color.FromArgb(0x19, 0x80, 0x80, 0x80);



	//Yoinked from Windows Community Toolkit
	public static async Task<byte[]> CreateCheckeredBitmapAsync(
	int width,
	int height,
	Color checkerColor) {
		// The size of the checker is important. You want it big enough that the grid is clearly discernible.
		// However, the squares should be small enough they don't appear unnaturally cut at the edge of backgrounds.
		int checkerSize = 4;

		if (width == 0 || height == 0) {
			return null!;
		}

		var bitmap = await Task.Run(() => {
			int pixelDataIndex = 0;
			byte[] bgraPixelData;

			// Allocate the buffer
			// BGRA formatted color channels 1 byte each (4 bytes in a pixel)
			bgraPixelData = new byte[width * height * 4];

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					// We want the checkered pattern to alternate both vertically and horizontally.
					// In order to achieve that, we'll toggle visibility of the current pixel on or off
					// depending on both its x- and its y-position.  If x == CheckerSize, we'll turn visibility off,
					// but then if y == CheckerSize, we'll turn it back on.
					// The below is a shorthand for the above intent.
					bool pixelShouldBeBlank = ((x / checkerSize) + (y / checkerSize)) % 2 == 0 ? true : false;

					// Remember, use BGRA pixel format with pre-multiplied alpha values
					if (pixelShouldBeBlank) {
						bgraPixelData[pixelDataIndex + 0] = 0;
						bgraPixelData[pixelDataIndex + 1] = 0;
						bgraPixelData[pixelDataIndex + 2] = 0;
						bgraPixelData[pixelDataIndex + 3] = 0;
					}
					else {
						bgraPixelData[pixelDataIndex + 0] = Convert.ToByte(checkerColor.B * checkerColor.A / 255);
						bgraPixelData[pixelDataIndex + 1] = Convert.ToByte(checkerColor.G * checkerColor.A / 255);
						bgraPixelData[pixelDataIndex + 2] = Convert.ToByte(checkerColor.R * checkerColor.A / 255);
						bgraPixelData[pixelDataIndex + 3] = checkerColor.A;
					}

					pixelDataIndex += 4;
				}
			}

			return bgraPixelData;
		}).ConfigureAwait(true);

		return bitmap;
	}
}
