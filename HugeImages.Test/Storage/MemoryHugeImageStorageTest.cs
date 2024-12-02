using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Storage;
using Pmad.HugeImages.Test.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Test.Storage
{
    public class MemoryHugeImageStorageTest
    {

        [Fact]
        public async Task CreateSlot()
        {
            var storage = new MemoryHugeImageStorage();

            using var image = new HugeImage<Rgb24>(storage, "name", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });

            Assert.True(storage.Slots.ContainsKey("name"));

            var slot = storage.Slots["name"];

            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            await image.OffloadAsync();

            Assert.Equal(4, slot.Parts.Count);

            await Samples.AssertBasicDrawing(image);
        }
    }
}
