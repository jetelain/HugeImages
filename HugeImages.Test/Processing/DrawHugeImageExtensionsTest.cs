using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HugeImages.Test.Processing
{
    public class DrawHugeImageExtensionsTest
    {
        [Fact]
        public async Task DrawHugeImageExtensions_DrawHugeImage()
        {
            var himage = await CreateBasicImage();

            using var slice = new Image<Rgb24>(500, 500);
            slice.Mutate(d =>
            {
                d.DrawHugeImage(himage, new Point(250, 250));
            });

            using var expected = new Image<Rgb24>(1000, 1000);
            expected.Mutate(d =>
            {
                Samples.BasicDrawing(d);
                d.Crop(new Rectangle(250, 250, 500, 500));
            });

            await Samples.AssertEqual(expected, slice);
        }

        private static async Task<HugeImage<Rgb24>> CreateBasicImage()
        {
            var image = new HugeImage<Rgb24>(new HugeImageStorageMock(), "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            return image;
        }
    }
}
