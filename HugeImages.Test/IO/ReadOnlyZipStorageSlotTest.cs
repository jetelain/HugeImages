using System.IO.Compression;
using Pmad.HugeImages.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.HugeImages.Test.IO
{
    public class ReadOnlyZipStorageSlotTest
    {
        private static readonly HugeImageIndex DefaultIndex = new HugeImageIndex("image.png", new Size(50, 50), Color.Black, new List<HugeImageIndexPart>() { new HugeImageIndexPart(new Rectangle(0, 0, 50, 50), new Rectangle(0, 0, 50, 50), 1, "1.png") });
        private static MemoryStream CreateArchive()
        {
            var mem = new MemoryStream();
            using (var archive1 = new ZipArchive(mem, ZipArchiveMode.Create, true))
            {
                var entry = archive1.CreateEntry("1.png");
                using (var stream = entry.Open())
                {
                    new Image<Rgb24>(50, 50, new Rgb24(128, 128, 128)).SaveAsPng(stream);
                }
            }
            mem.Position = 0;
            return mem;
        }

        [Fact]
        public async Task LoadImagePart()
        {
            using var slot = new ReadOnlyZipStorageSlot(new ZipArchive(CreateArchive(), ZipArchiveMode.Read), DefaultIndex, new HugeImageSettingsBase());

            var part1 = await slot.LoadImagePart<Rgb24>(1);
            Assert.NotNull(part1);
            Assert.Equal(50, part1!.Width);
            Assert.Equal(50, part1!.Height);
            Assert.Equal(new Rgb24(128, 128, 128), part1[0, 0]);

            Assert.Null(await slot.LoadImagePart<Rgb24>(2));
        }

        [Fact]
        public void ImagePartExists()
        {
            using var slot = new ReadOnlyZipStorageSlot(new ZipArchive(CreateArchive(), ZipArchiveMode.Read), DefaultIndex, new HugeImageSettingsBase());

            Assert.True(slot.ImagePartExists(1));

            Assert.False(slot.ImagePartExists(2));
        }

        [Fact]
        public async Task CopyImagePartTo()
        {
            using var slot = new ReadOnlyZipStorageSlot(new ZipArchive(CreateArchive(), ZipArchiveMode.Read), DefaultIndex, new HugeImageSettingsBase());

            var mem = new MemoryStream();
            await slot.CopyImagePartTo(1, mem);
            Assert.NotEqual(0, mem.Position);
            mem.Position = 0;

            var img = Image.Load<Rgb24>(mem);
            Assert.Equal(50, img.Width);
            Assert.Equal(50, img.Height);
            Assert.Equal(new Rgb24(128, 128, 128), img[0, 0]);
        }
    }
}
