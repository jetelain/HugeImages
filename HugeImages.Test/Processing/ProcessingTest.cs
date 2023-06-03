using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

namespace HugeImages.Test.Processing
{
    public class ProcessingTest
    {
        [Fact]
        public async Task HugeImage_MutateAll_Fill()
        {
            var storage = new HugeImageStorageMock();
            using var image = new HugeImage<Rgb24>(storage, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                BasicDrawing(d);
            });
            await image.OffloadAsync();

            CheckStorage(storage);

            await CheckReferenceImage(image);
        }

        [Fact]
        public async Task HugeImage_MutateAllParallel_Fill()
        {
            var storage = new HugeImageStorageMock();
            using var image = new HugeImage<Rgb24>(storage, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllParallelAsync(d =>
            {
                BasicDrawing(d);
            });
            await image.OffloadAsync();

            CheckStorage(storage);

            await CheckReferenceImage(image);
        }
        [Fact]
        public async Task HugeImage_MutateBuffered_Fill()
        {
            var storage = new HugeImageStorageMock();
            using var image = new HugeImage<Rgb24>(storage, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateBufferedAsync(d =>
            {
                BasicDrawing(d);
            });
            await image.OffloadAsync();

            CheckStorage(storage);

            await CheckReferenceImage(image);
        }

        [Fact]
        public async Task HugeImage_MutateBufferedParallel_Fill()
        {
            var storage = new HugeImageStorageMock();
            using var image = new HugeImage<Rgb24>(storage, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateBufferedParallelAsync(d =>
            {
                BasicDrawing(d);
            });
            await image.OffloadAsync();

            CheckStorage(storage);

            await CheckReferenceImage(image);
        }

        private static void CheckStorage(HugeImageStorageMock storage)
        {
            Assert.Equal(4, storage.Storage.Count);
            Assert.All(storage.Storage.Values, i => { Assert.Equal(506, i.Width); Assert.Equal(506, i.Height); });

            Assert.Equal(Color.Blue.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[1])[250, 250]);
            Assert.Equal(Color.Violet.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[1])[500, 500]);

            Assert.Equal(Color.Red.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[2])[250, 250]);
            Assert.Equal(Color.Violet.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[2])[500, 0]);

            Assert.Equal(Color.Green.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[3])[250, 250]);
            Assert.Equal(Color.Violet.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[3])[0, 500]);

            Assert.Equal(Color.Yellow.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[4])[250, 250]);
            Assert.Equal(Color.Violet.ToPixel<Rgb24>(), ((Image<Rgb24>)storage.Storage[4])[0, 0]);
        }

        private static void BasicDrawing(IImageProcessingContext d)
        {
            d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(250, 250), 250));
            d.Fill(new SolidBrush(Color.Green), new EllipsePolygon(new PointF(750, 250), 250));
            d.Fill(new SolidBrush(Color.Red), new EllipsePolygon(new PointF(250, 750), 250));
            d.Fill(new SolidBrush(Color.Yellow), new EllipsePolygon(new PointF(750, 750), 250));
            d.Fill(new SolidBrush(Color.Violet), new EllipsePolygon(new PointF(500, 500), 250));
        }

        private static async Task CheckReferenceImage(HugeImage<Rgb24> image)
        {
            using var expected = new Image<Rgb24>(1000, 1000);
            expected.Mutate(d =>
            {
                BasicDrawing(d);
            });

            using var full = await image.ToScaledImageAsync(1000, 1000);
            await AssertEqual(expected, full);
        }

        [Fact]
        public async Task HugeImage_ToImageScaled()
        {
            var storage = new HugeImageStorageMock();
            using var image = new HugeImage<Rgb24>(storage, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                BasicDrawing(d);
            });

            using var expected = new Image<Rgb24>(1000, 1000);
            expected.Mutate(d =>
            {
                BasicDrawing(d);
                d.Resize(500, 500);
            });

            var full = await image.ToScaledImageAsync(500, 500);

            await AssertEqual(expected, full);
        }

        private static async Task AssertEqual(Image<Rgb24> expected, Image<Rgb24> full)
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

    }
}
