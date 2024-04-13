using HugeImages.IO;
using HugeImages.Processing;
using HugeImages.Test.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test.IO
{
    public class HugeImageDeserializerTest
    {
        [Fact]
        public async Task LoadReadOnlyLocked()
        {
            var storage1 = new HugeImageStorageMock();

            var stream = await CreateBasicDrawing(storage1);

            var himg2 = await HugeImageDeserializer<Rgb24>.LoadReadOnlyLocked(stream, null);

            Assert.Equal(new Size(1000, 1000), himg2.Size);
            Assert.Equal(new Rgb24(0,0,0), himg2.Background);
            Assert.Equal(4, himg2.Parts.Count);

            await Samples.AssertBasicDrawing(himg2);

            // Make a mutation on image => Will fail, as Slot is read only
            await himg2.MutateAllAsync(d => { });
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await himg2.OffloadAsync());
        }

        [Fact]
        public async Task LoadCopy()
        {
            var storage1 = new HugeImageStorageMock();

            var stream = await CreateBasicDrawing(storage1);

            var storage2 = new HugeImageStorageMock();

            var himg2 = await HugeImageDeserializer<Rgb24>.LoadCopy(stream, storage2, "unused", null);

            Assert.Equal(new Size(1000, 1000), himg2.Size);
            Assert.Equal(new Rgb24(0, 0, 0), himg2.Background);
            Assert.Equal(4, himg2.Parts.Count);

            Assert.Equal(4, storage2.Storage.Count);
            Assert.Equal(0, storage2.WriteCalls);
            Assert.Equal(4, storage2.CopyFromCalls);
            await Samples.AssertBasicDrawing(himg2);

            // Make a mutation on image => OK
            await himg2.MutateAllAsync(d => { });
            await himg2.OffloadAsync();
            Assert.Equal(4, storage2.WriteCalls);
        }

        internal static async Task<MemoryStream> CreateBasicDrawing(HugeImageStorageMock storage1)
        {
            using var image = new HugeImage<Rgb24>(storage1, "unused", new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });
            await image.OffloadAsync();
            var mem = new MemoryStream();
            await HugeImageSerializer<Rgb24>.Save(image, mem);
            mem.Position = 0;
            return mem;
        }
    }
}
