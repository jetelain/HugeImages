using Pmad.HugeImages.Storage;
using Pmad.HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using IOPath = System.IO.Path;

namespace Pmad.HugeImages.Test.Storage
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

        [Fact]
        public async Task LoadUniqueReadWriteAsync_LoadsImageCorrectly()
        {
            // Arrange
            var tempFile = IOPath.Combine(IOPath.GetTempPath(), $"test_{Guid.NewGuid()}.png");
            try
            {
                var originalImage = new Image<Rgb24>(128, 64);
                originalImage.Mutate(ctx => ctx.Fill(new SolidBrush(Color.Red), new RectangularPolygon(0, 0, 128, 64)));
                await originalImage.SaveAsPngAsync(tempFile);
                originalImage.Dispose();

                // Act
                using var himage = await StorageExtensions.LoadUniqueReadWriteAsync<Rgb24>(tempFile);

                // Assert
                Assert.Equal(new Size(128, 64), himage.Size);
                var part = Assert.Single(himage.Parts);
                Assert.Equal(new Rectangle(0, 0, 128, 64), part.Rectangle);
                Assert.Equal(new Rectangle(0, 0, 128, 64), part.RealRectangle);

                using var token = await part.AcquireAsync();
                var loadedImage = token.GetImageReadOnly();
                Assert.Equal(128, loadedImage.Width);
                Assert.Equal(64, loadedImage.Height);
                Assert.Equal(new Rgb24(255, 0, 0), loadedImage[0, 0]);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task LoadUniqueReadWriteAsync_ModificationsArePersisted()
        {
            // Arrange
            var tempFile = IOPath.Combine(IOPath.GetTempPath(), $"test_{Guid.NewGuid()}.png");
            try
            {
                var originalImage = new Image<Rgb24>(100, 100);
                originalImage.Mutate(ctx => ctx.Fill(new SolidBrush(Color.Blue), new RectangularPolygon(0, 0, 100, 100)));
                await originalImage.SaveAsPngAsync(tempFile);
                originalImage.Dispose();

                // Act - Load and modify
                using (var himage = await StorageExtensions.LoadUniqueReadWriteAsync<Rgb24>(tempFile))
                {
                    await himage.MutateAllAsync(ctx => ctx.Fill(new SolidBrush(Color.Green), new RectangularPolygon(0, 0, 100, 100)));
                    await himage.OffloadAsync();
                }

                // Assert - Reload and verify changes were persisted
                var reloadedImage = await Image.LoadAsync<Rgb24>(tempFile);
                Assert.Equal(new Rgb24(0, 128, 0), reloadedImage[0, 0]);
                Assert.Equal(new Rgb24(0, 128, 0), reloadedImage[50, 50]);
                Assert.Equal(new Rgb24(0, 128, 0), reloadedImage[99, 99]);
                reloadedImage.Dispose();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task LoadUniqueReadWriteAsync_WorksWithDifferentPixelFormats()
        {
            // Arrange
            var tempFile = IOPath.Combine(IOPath.GetTempPath(), $"test_{Guid.NewGuid()}.png");
            try
            {
                var originalImage = new Image<Rgba32>(50, 50);
                originalImage.Mutate(ctx => ctx.Fill(new SolidBrush(Color.Yellow), new RectangularPolygon(0, 0, 50, 50)));
                await originalImage.SaveAsPngAsync(tempFile);
                originalImage.Dispose();

                // Act
                using var himage = await StorageExtensions.LoadUniqueReadWriteAsync<Rgba32>(tempFile);

                // Assert
                Assert.Equal(new Size(50, 50), himage.Size);
                var part = Assert.Single(himage.Parts);
                using var token = await part.AcquireAsync();
                var loadedImage = token.GetImageReadOnly();
                Assert.Equal(new Rgba32(255, 255, 0, 255), loadedImage[0, 0]);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task LoadUniqueReadWriteAsync_ThrowsWhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFile = IOPath.Combine(IOPath.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.png");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                await StorageExtensions.LoadUniqueReadWriteAsync<Rgb24>(nonExistentFile);
            });
        }

    }
}
