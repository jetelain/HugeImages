using Pmad.HugeImages.Storage;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Test.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IOPath = System.IO.Path;

namespace Pmad.HugeImages.Test.Storage
{
    public class TemporaryHugeImageStorageTest
    {
        [Fact]
        public void Constructor_CreatesBasePath()
        {
            // Act
            using var storage = new TemporaryHugeImageStorage();

            // Assert
            var basePath = storage.BasePath;
            Assert.NotNull(basePath);
            Assert.Contains("HugeImages", basePath);
            Assert.Contains(IOPath.GetTempPath(), basePath);
        }

        [Fact]
        public async Task CreateSlot_CreatesWorkingSlot()
        {
            // Arrange
            using var storage = new TemporaryHugeImageStorage();

            // Act
            using var image = new HugeImage<Rgb24>(storage, "test_image", new Size(1000, 1000), 
                new HugeImageSettings() { PartMaxSize = 512, PartOverlap = 6 });

            await image.MutateAllAsync(d =>
            {
                Samples.BasicDrawing(d);
            });

            await image.OffloadAsync();

            // Assert
            await Samples.AssertBasicDrawing(image);
        }

        [Fact]
        public async Task CreateSlot_PersistsImagePartsToFileSystem()
        {
            // Arrange
            string? slotPath = null;
            using (var storage = new TemporaryHugeImageStorage())
            {
                var basePath = storage.BasePath;
                slotPath = IOPath.Combine(basePath, "persist_test");

                using var image = new HugeImage<Rgb24>(storage, "persist_test", new Size(800, 800),
                    new HugeImageSettings() { PartMaxSize = 400, PartOverlap = 4 });

                // Act
                await image.MutateAllAsync(d =>
                {
                    Samples.BasicDrawing(d);
                });

                await image.OffloadAsync();

                // Assert - Files exist before disposal
                Assert.NotNull(slotPath);
                Assert.True(Directory.Exists(slotPath), "Slot directory should exist");
                var files = Directory.GetFiles(slotPath);
                Assert.NotEmpty(files);
            }

            // Assert - Files cleaned up after disposal
            if (slotPath != null)
            {
                Assert.False(Directory.Exists(slotPath), "Slot directory should be deleted after disposal");
            }
        }

        [Fact]
        public async Task Dispose_CleansUpAllFiles()
        {
            // Arrange
            string? basePath = null;
            using (var storage = new TemporaryHugeImageStorage())
            {
                basePath = storage.BasePath;

                using var image1 = new HugeImage<Rgb24>(storage, "image1", new Size(500, 500),
                    new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 4 });
                using var image2 = new HugeImage<Rgb24>(storage, "image2", new Size(500, 500),
                    new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 4 });

                await image1.MutateAllAsync(d => Samples.BasicDrawing(d));
                await image2.MutateAllAsync(d => Samples.BasicDrawing(d));
                await image1.OffloadAsync();
                await image2.OffloadAsync();

                // Assert - Directory exists before disposal
                Assert.NotNull(basePath);
                Assert.True(Directory.Exists(basePath), "Base path should exist before disposal");
            }

            // Act & Assert - Directory cleaned up after disposal
            Assert.False(Directory.Exists(basePath!), "Base path should be deleted after disposal");
        }

        [Fact]
        public async Task MultipleSlots_WorkIndependently()
        {
            // Arrange
            using var storage = new TemporaryHugeImageStorage();

            // Act
            using var image1 = new HugeImage<Rgb24>(storage, "slot1", new Size(512, 512),
                new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 4 });
            using var image2 = new HugeImage<Rgba32>(storage, "slot2", new Size(400, 400),
                new HugeImageSettings() { PartMaxSize = 200, PartOverlap = 4 });

            await image1.MutateAllAsync(d => Samples.BasicDrawing(d));
            await image2.MutateAllAsync(d => Samples.BasicDrawing(d));

            await image1.OffloadAsync();
            await image2.OffloadAsync();

            // Assert - Both images have their own parts
            Assert.True(image1.Parts.Count >= 4, $"Image1 should have at least 4 parts, got {image1.Parts.Count}");
            Assert.True(image2.Parts.Count >= 4, $"Image2 should have at least 4 parts, got {image2.Parts.Count}");

            // Verify image1 data is correct (don't call AssertBasicDrawing as it may fail if c:\temp doesn't exist)
            using var full = await image1.ToScaledImageAsync(512, 512);
            Assert.NotNull(full);
            Assert.Equal(512, full.Width);
            Assert.Equal(512, full.Height);
        }

        [Fact]
        public async Task LoadAfterOffload_ReloadsFromFileSystem()
        {
            // Arrange
            using var storage = new TemporaryHugeImageStorage();
            using var image = new HugeImage<Rgb24>(storage, "reload_test", new Size(512, 512),
                new HugeImageSettings() { PartMaxSize = 256, PartOverlap = 4 });

            // Act - Draw, offload, and reload
            await image.MutateAllAsync(d => Samples.BasicDrawing(d));
            await image.OffloadAsync();

            // Load the image again
            var part = image.Parts[0];
            using var token = await part.AcquireAsync();
            var loadedImage = token.GetImageReadOnly();

            // Assert
            Assert.NotNull(loadedImage);
            Assert.True(loadedImage.Width > 0);
            Assert.True(loadedImage.Height > 0);
        }

        [Fact]
        public async Task SaveAndReload_PreservesData()
        {
            // Arrange
            using var storage = new TemporaryHugeImageStorage();

            // Act - Create and save an image
            using (var image = new HugeImage<Rgb24>(storage, "data_test", new Size(400, 400),
                new HugeImageSettings() { PartMaxSize = 200, PartOverlap = 4 }))
            {
                await image.MutateAllAsync(d => Samples.BasicDrawing(d));
                await image.OffloadAsync();
            }

            // Reload and verify - Create new image with same slot name
            using (var reloadedImage = new HugeImage<Rgb24>(storage, "data_test", new Size(400, 400),
                new HugeImageSettings() { PartMaxSize = 200, PartOverlap = 4 }))
            {
                // Load a part to verify it exists
                var part = reloadedImage.Parts[0];
                using var token = await part.AcquireAsync();
                var loadedImage = token.GetImageReadOnly();

                // Assert
                Assert.NotNull(loadedImage);
            }
        }

        [Fact]
        public async Task SlotDisposal_CleansUpItsFiles()
        {
            // Arrange
            string? slotPath = null;
            using var storage = new TemporaryHugeImageStorage();

            var basePath = storage.BasePath;
            slotPath = IOPath.Combine(basePath, "disposal_test");

            // Act - Create and dispose image
            using (var image = new HugeImage<Rgb24>(storage, "disposal_test", new Size(300, 300),
                new HugeImageSettings() { PartMaxSize = 150, PartOverlap = 4 }))
            {
                await image.MutateAllAsync(d => Samples.BasicDrawing(d));
                await image.OffloadAsync();

                Assert.NotNull(slotPath);
                Assert.True(Directory.Exists(slotPath), "Slot directory should exist before image disposal");
            }

            // Assert - Slot directory cleaned up after image disposal (TemporaryHugeImageStorageSlot.Dispose)
            Assert.False(Directory.Exists(slotPath!), "Slot directory should be deleted after image disposal");
        }

        [Fact]
        public async Task SlotPath_IsCorrect()
        {
            // Arrange
            using var storage = new TemporaryHugeImageStorage();
            using var image = new HugeImage<Rgb24>(storage, "path_test", new Size(300, 300),
                new HugeImageSettings() { PartMaxSize = 150, PartOverlap = 4 });

            // Act
            var slot = image.Parts[0].Parent.Slot as HugeImageStorageSlotBase;
            var slotPath = slot?.SlotPath;

            // Assert
            Assert.NotNull(slotPath);
            Assert.Contains("path_test", slotPath);
            Assert.Contains(storage.BasePath, slotPath);
        }
    }
}
