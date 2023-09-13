using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HugeImages.Test.Storage
{
    public class StorageExtensionsTest
    {
        [Fact]
        public async Task FromUnique()
        {
            var img = new Image<Rgb24>(256, 256);
            using (var himage = StorageExtensions.FromUnique(img))
            {
                Assert.Equal(new Size(256,256), himage.Size);
                var part = Assert.Single(himage.Parts);
                Assert.Equal(new Rectangle(0, 0, 256, 256), part.Rectangle);
                Assert.Equal(new Rectangle(0, 0, 256, 256), part.RealRectangle);
                using (var token = await part.AcquireAsync())
                {
                    Assert.Same(img, token.GetImageReadOnly());
                }
            }
        }

    }
}
