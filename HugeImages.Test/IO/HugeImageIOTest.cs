using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pmad.HugeImages.IO;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Test.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Test.IO
{
    public class HugeImageIOTest
    {
        [Fact]
        public async Task SaveAsync()
        {
            var storage1 = new HugeImageStorageMock();

            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            var mem = new MemoryStream();
            await image.SaveAsync(mem);
            mem.Position = 0;

            await image.OffloadAsync();

            await IOHelper.CheckSampleImage(storage1, mem);
        }

        [Fact]
        public async Task LoadReadOnlyLockedAsync()
        {
            var storage1 = new HugeImageStorageMock();

            var stream = await HugeImageDeserializerTest.CreateBasicDrawing(storage1);

            var himg2 = await HugeImageIO.LoadReadOnlyLockedAsync<Rgb24>(stream, null);

            await Samples.AssertBasicDrawing(himg2);
        }

        [Fact]
        public async Task LoadCopyAsync()
        {
            var storage1 = new HugeImageStorageMock();

            var stream = await HugeImageDeserializerTest.CreateBasicDrawing(storage1);

            var storage2 = new HugeImageStorageMock();

            var himg2 = await HugeImageIO.LoadCopyAsync<Rgb24>(stream, storage2, "unused", null);

            await Samples.AssertBasicDrawing(himg2);
        }

        [Fact]
        public async Task LoadCloneAsync()
        {
            var storage1 = new HugeImageStorageMock();

            var stream = await HugeImageDeserializerTest.CreateBasicDrawing(storage1);

            var storage2 = new HugeImageStorageMock();

            var himg2 = await HugeImageIO.LoadCloneAsync<Rgb24>(stream, storage2, "unused", null);

            await Samples.AssertBasicDrawing(himg2);
        }
    }
}
