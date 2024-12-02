using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;
using Pmad.HugeImages.Processing;

namespace Pmad.HugeImages.Test.Processing
{
    internal static class Samples
    {
        public static void BasicDrawing(IImageProcessingContext d)
        {
            d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(250, 250), 250));
            d.Fill(new SolidBrush(Color.Green), new EllipsePolygon(new PointF(750, 250), 250));
            d.Fill(new SolidBrush(Color.Red), new EllipsePolygon(new PointF(250, 750), 250));
            d.Fill(new SolidBrush(Color.Yellow), new EllipsePolygon(new PointF(750, 750), 250));
            d.Fill(new SolidBrush(Color.Violet), new EllipsePolygon(new PointF(500, 500), 250));
        }
        public static async Task AssertEqual(Image<Rgb24> expected, Image<Rgb24> full)
        {
            try
            {
                ImageComparer.TolerantPercentage(5).VerifySimilarity(expected, full);
            }
            catch
            {
                await full.SaveAsPngAsync(@"c:\temp\full.png");
                await expected.SaveAsPngAsync(@"c:\temp\expected.png");
                throw;
            }
        }

        public static async Task AssertBasicDrawing(HugeImage<Rgb24> image)
        {
            using var expected = new Image<Rgb24>(1000, 1000);
            expected.Mutate(d =>
            {
                BasicDrawing(d);
            });
            using var full = await image.ToScaledImageAsync(1000, 1000);
            await AssertEqual(expected, full);
        }
    }
}
