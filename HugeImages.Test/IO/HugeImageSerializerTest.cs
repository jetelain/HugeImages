using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HugeImages.IO;
using HugeImages.Processing;
using HugeImages.Test.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test.IO
{
    public class HugeImageSerializerTest
    {
        [Fact]
        public async Task Save_Loaded_Generic()
        {
            var storage1 = new HugeImageStorageMock();

            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            Assert.Equal(0, storage1.WriteCalls);

            var mem = new MemoryStream();
            await HugeImageSerializer<Rgb24>.Save(image, mem);
            mem.Position = 0;

            Assert.Equal(0, storage1.WriteCalls);

            await image.OffloadAsync();

            Assert.Equal(4, storage1.WriteCalls);

            await IOHelper.CheckSampleImage(storage1, mem);
        }

        [Fact]
        public async Task Save_Offloaded_Generic()
        {
            var storage1 = new HugeImageStorageMock();

            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            Assert.Equal(0, storage1.WriteCalls);

            await image.OffloadAsync();

            Assert.Equal(4, storage1.WriteCalls);

            var mem = new MemoryStream();
            await HugeImageSerializer<Rgb24>.Save(image, mem);
            mem.Position = 0;

            Assert.Equal(4, storage1.WriteCalls);

            await IOHelper.CheckSampleImage(storage1, mem);
        }
        [Fact]
        public async Task Save_Loaded_Copyable()
        {
            var storage1 = new HugeImageStorageCopyableMock();

            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            Assert.Equal(0, storage1.CopyCalls);
            Assert.Equal(0, storage1.WriteCalls);

            var mem = new MemoryStream();
            await HugeImageSerializer<Rgb24>.Save(image, mem);
            mem.Position = 0;

            Assert.Equal(4, storage1.CopyCalls);
            Assert.Equal(4, storage1.WriteCalls);

            await image.OffloadAsync();

            Assert.Equal(4, storage1.CopyCalls);
            Assert.Equal(4, storage1.WriteCalls);

            await IOHelper.CheckSampleImage(storage1, mem);
        }

        [Fact]
        public async Task Save_Offloaded_Copyable()
        {
            var storage1 = new HugeImageStorageCopyableMock();

            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            Assert.Equal(0, storage1.CopyCalls);
            Assert.Equal(0, storage1.WriteCalls);

            await image.OffloadAsync();

            Assert.Equal(0, storage1.CopyCalls);
            Assert.Equal(4, storage1.WriteCalls);

            var mem = new MemoryStream();
            await HugeImageSerializer<Rgb24>.Save(image, mem);
            mem.Position = 0;

            Assert.Equal(4, storage1.CopyCalls);
            Assert.Equal(4, storage1.WriteCalls);

            await IOHelper.CheckSampleImage(storage1, mem);
        }
    }
}
