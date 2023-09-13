using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test
{
    public class HugeImageTest
    {
        [Fact]
        public void GetPartSize()
        {
            Assert.Equal(50, HugeImageSettings.GetPartSize(100, 50));
            Assert.Equal(34, HugeImageSettings.GetPartSize(102, 50));
            Assert.Equal(25, HugeImageSettings.GetPartSize(100, 30));
            Assert.Equal(26, HugeImageSettings.GetPartSize(102, 30));
        }

        [Fact]
        public void Parts()
        {
            var image = new HugeImage<Rgb24>(new HugeImageStorageMock(), new Size(1000, 1000), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            var parts = image.Parts;
            Assert.Equal(4, parts.Count);
            Assert.Equal(new Rectangle(0, 0, 500, 500), parts[0].Rectangle);
            Assert.Equal(new Rectangle(0, 0, 506, 506), parts[0].RealRectangle);

            Assert.Equal(new Rectangle(0, 500, 500, 500), parts[1].Rectangle);
            Assert.Equal(new Rectangle(0, 494, 506, 506), parts[1].RealRectangle);

            Assert.Equal(new Rectangle(500, 0, 500, 500), parts[2].Rectangle);
            Assert.Equal(new Rectangle(494, 0, 506, 506), parts[2].RealRectangle);

            Assert.Equal(new Rectangle(500, 500, 500, 500), parts[3].Rectangle);
            Assert.Equal(new Rectangle(494, 494, 506, 506), parts[3].RealRectangle);

            image = new HugeImage<Rgb24>(new HugeImageStorageMock(), new Size(1500, 1500), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            parts = image.Parts;
            Assert.Equal(9, parts.Count);
            Assert.Equal(new Rectangle(0, 0, 500, 500), parts[0].Rectangle);
            Assert.Equal(new Rectangle(0, 0, 506, 506), parts[0].RealRectangle);

            Assert.Equal(new Rectangle(0, 500, 500, 500), parts[1].Rectangle);
            Assert.Equal(new Rectangle(0, 494, 506, 512), parts[1].RealRectangle);

            Assert.Equal(new Rectangle(0, 1000, 500, 500), parts[2].Rectangle);
            Assert.Equal(new Rectangle(0, 994, 506, 506), parts[2].RealRectangle);

            Assert.Equal(new Rectangle(500, 0, 500, 500), parts[3].Rectangle);
            Assert.Equal(new Rectangle(494, 0, 512, 506), parts[3].RealRectangle);

            Assert.Equal(new Rectangle(500, 500, 500, 500), parts[4].Rectangle);
            Assert.Equal(new Rectangle(494, 494, 512, 512), parts[4].RealRectangle);

            Assert.Equal(new Rectangle(500, 1000, 500, 500), parts[5].Rectangle);
            Assert.Equal(new Rectangle(494, 994, 512, 506), parts[5].RealRectangle);

            Assert.Equal(new Rectangle(1000, 0, 500, 500), parts[6].Rectangle);
            Assert.Equal(new Rectangle(994, 0, 506, 506), parts[6].RealRectangle);

            Assert.Equal(new Rectangle(1000, 500, 500, 500), parts[7].Rectangle);
            Assert.Equal(new Rectangle(994, 494, 506, 512), parts[7].RealRectangle);

            Assert.Equal(new Rectangle(1000, 1000, 500, 500), parts[8].Rectangle);
            Assert.Equal(new Rectangle(994, 994, 506, 506), parts[8].RealRectangle);
        }

        [Fact]
        public void MaxLoadedPartsCount()
        {
            var image = new HugeImage<Rgb24>(new HugeImageStorageMock(), new Size(1500, 1500), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });
            Assert.Equal(9, image.MaxLoadedPartsCount);

            image = new HugeImage<Rgb24>(new HugeImageStorageMock(), new Size(1500, 1500), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 512*512*3*2 });
            Assert.Equal(2, image.MaxLoadedPartsCount); 
            
            image = new HugeImage<Rgb24>(new HugeImageStorageMock(), new Size(1500, 1500), new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6, MemoryLimit = 1 });
            Assert.Equal(1, image.MaxLoadedPartsCount);
        }
    }
}
