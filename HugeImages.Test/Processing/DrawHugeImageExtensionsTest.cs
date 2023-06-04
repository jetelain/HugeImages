using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace HugeImages.Test.Processing
{
    public class DrawHugeImageExtensionsTest
    {
        [Fact]
        public async Task DrawHugeImage_OnRegularImage()
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

        [Fact]
        public async Task DrawHugeImage_OnHugeImage_MutateAllAsync_Aligned()
        {
            var storage1 = new HugeImageStorageMock();
            var himage1 = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            var storage2 = new HugeImageStorageMock();
            var himage2 = new HugeImage<Rgb24>(storage2, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            await himage1.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            await himage1.OffloadAsync();
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(4, storage1.ReadCalls);
            Assert.Equal(0, himage1.LoadedPartsCount);

            await himage2.MutateAllAsync(async d =>
            {
                await d.DrawHugeImageAsync(himage1, Point.Empty);
            });
            await himage2.OffloadAsync();

            Assert.Equal(4, storage2.WriteCalls);
            Assert.Equal(4, storage2.ReadCalls);
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(8, storage1.ReadCalls);

            var i1 = await himage1.ToScaledImageAsync(1000, 1000);
            var i2 = await himage2.ToScaledImageAsync(1000, 1000);
            //await i1.SaveAsPngAsync(@"c:\temp\i1.png");
            //await i2.SaveAsPngAsync(@"c:\temp\i2.png");

            await Samples.AssertEqual(i1, i2);
        }

        [Fact]
        public async Task DrawHugeImage_OnHugeImage_MutateAllAsync_Unaligned()
        {
            var storage1 = new HugeImageStorageMock();
            var himage1 = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            var storage2 = new HugeImageStorageMock();
            var himage2 = new HugeImage<Rgb24>(storage2, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 6, MemoryLimit = 1 });

            await himage1.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            await himage1.OffloadAsync();
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(4, storage1.ReadCalls);
            Assert.Equal(0, himage1.LoadedPartsCount);

            await himage2.MutateAllAsync(async d =>
            {
                await d.DrawHugeImageAsync(himage1, Point.Empty);
            });
            await himage2.OffloadAsync();

            Assert.Equal(25, storage2.WriteCalls);
            Assert.Equal(25, storage2.ReadCalls);
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(20, storage1.ReadCalls);

            var i1 = await himage1.ToScaledImageAsync(1000, 1000);
            var i2 = await himage2.ToScaledImageAsync(1000, 1000);
            //await i1.SaveAsPngAsync(@"c:\temp\i1.png");
            //await i2.SaveAsPngAsync(@"c:\temp\i2.png");

            await Samples.AssertEqual(i1, i2);
        }

        [Fact]
        public async Task DrawHugeImage_OnHugeImage_MutateAsync_Aligned()
        {
            var storage1 = new HugeImageStorageMock();
            var himage1 = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            var storage2 = new HugeImageStorageMock();
            var himage2 = new HugeImage<Rgb24>(storage2, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            await himage1.MutateAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            await himage1.OffloadAsync();
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(4, storage1.ReadCalls);
            Assert.Equal(0, himage1.LoadedPartsCount);

            await himage2.MutateAsync(async d =>
            {
                await d.DrawHugeImageAsync(himage1, Point.Empty);
            });
            await himage2.OffloadAsync();

            Assert.Equal(4, storage2.WriteCalls);
            Assert.Equal(4, storage2.ReadCalls);
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(8, storage1.ReadCalls);

            var i1 = await himage1.ToScaledImageAsync(1000, 1000);
            var i2 = await himage2.ToScaledImageAsync(1000, 1000);
            //await i1.SaveAsPngAsync(@"c:\temp\i1.png");
            //await i2.SaveAsPngAsync(@"c:\temp\i2.png");

            await Samples.AssertEqual(i1, i2);
        }

        [Fact]
        public async Task DrawHugeImage_OnHugeImage_MutateAsync_Unaligned()
        {
            var storage1 = new HugeImageStorageMock();
            var himage1 = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });

            var storage2 = new HugeImageStorageMock();
            var himage2 = new HugeImage<Rgb24>(storage2, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 6, MemoryLimit = 1 });

            await himage1.MutateAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            await himage1.OffloadAsync();
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(4, storage1.ReadCalls);
            Assert.Equal(0, himage1.LoadedPartsCount);

            await himage2.MutateAsync(async d =>
            {
                await d.DrawHugeImageAsync(himage1, Point.Empty);
            });
            await himage2.OffloadAsync();

            Assert.Equal(25, storage2.WriteCalls);
            Assert.Equal(25, storage2.ReadCalls);
            Assert.Equal(4, storage1.WriteCalls);
            Assert.Equal(20, storage1.ReadCalls);

            var i1 = await himage1.ToScaledImageAsync(1000, 1000);
            var i2 = await himage2.ToScaledImageAsync(1000, 1000);
            //await i1.SaveAsPngAsync(@"c:\temp\i1.png");
            //await i2.SaveAsPngAsync(@"c:\temp\i2.png");

            await Samples.AssertEqual(i1, i2);
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
