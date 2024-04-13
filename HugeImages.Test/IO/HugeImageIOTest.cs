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

    }
}
