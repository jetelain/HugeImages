using Pmad.HugeImages;
using Pmad.HugeImages.IO;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DemoApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await BasicDrawingExample();
            await TilingExample();
            await ThumbnailExample();
            await CustomSettingsExample();
            await PersistentStorageExample();
            await SaveAndLoadExample();
        }

        // --- Example 1: Basic drawing with MutateAllParallelAsync and MutateAllAsync ---
        static async Task BasicDrawingExample()
        {
            // Creates a 100,000 x 100,000 image using temporary disk storage.
            // With HugeImage: ~6 GiB RAM + ~30 MiB disk storage.
            // Without HugeImage: ~30 GiB RAM would be required.
            using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));

            // Fill and draw operations are independent per part: use the Parallel variant.
            await himage.MutateAllParallelAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            });

            // Most ImageSharp operations are already multi-threaded; use the non-parallel variant.
            // ~8 GiB RAM due to double-buffer requirement. Without HugeImage: ~60 GiB required.
            await himage.MutateAllAsync(d =>
            {
                d.GaussianBlur(10);
            });

            // Flush all parts from RAM to disk.
            await himage.OffloadAsync();

            // Mutate only a specific area of the image.
            var area = new Rectangle(40_000, 40_000, 20_000, 20_000);
            await himage.MutateAreaAsync(area, d =>
            {
                d.Fill(Color.Red);
            });

            // Mutate with automatic detection of the affected area (keeps operations in memory).
            await himage.MutateAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Green), new EllipsePolygon(new PointF(50_000, 50_000), 5_000));
            });

            // Create a thumbnail of the finished image.
            using var thumbnail = await himage.ToScaledImageAsync(1000, 1000);
            await thumbnail.SaveAsPngAsync("circle.png");
        }

        // --- Example 2: Tiling – extract a slice of a HugeImage ---
        static async Task TilingExample()
        {
            using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));
            await himage.MutateAllParallelAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            });

            // Extract the top of the circle: source area (49_500, 0) -> (50_500, 1000).
            using var slice = new Image<Rgb24>(1000, 1000);
            slice.Mutate(d =>
            {
                d.DrawHugeImage(himage, new Point(49_500, 0));
            });
            await slice.SaveAsPngAsync("circletop.png");

            // Extract with explicit target location and size.
            using var slice2 = new Image<Rgb24>(2000, 2000);
            slice2.Mutate(d =>
            {
                d.DrawHugeImage(himage, new Point(49_000, 49_000), new Point(500, 500), new Size(1000, 1000));
            });
            await slice2.SaveAsPngAsync("circlecenter.png");
        }

        // --- Example 3: Thumbnail generation ---
        static async Task ThumbnailExample()
        {
            using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));
            await himage.MutateAllParallelAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            });

            using var thumbnail = await himage.ToScaledImageAsync(1000, 1000);
            await thumbnail.SaveAsPngAsync("thumbnail.png");
        }

        // --- Example 4: Custom settings (memory limit, part size, overlap) ---
        static async Task CustomSettingsExample()
        {
            var settings = new HugeImageSettings
            {
                // Limit RAM usage to 2 GiB for this image instance.
                MemoryLimit = 2L * 1024 * 1024 * 1024,
                // Reduce part size to 8192 x 8192 (256 MiB at 32bpp).
                PartMaxSize = 8192,
                // Set overlap to 32 pixels to support blur radius up to 32.
                PartOverlap = 32
            };

            using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000), settings);
            await himage.MutateAllParallelAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            });

            using var thumbnail = await himage.ToScaledImageAsync(500, 500);
            await thumbnail.SaveAsPngAsync("custom_settings.png");
        }

        // --- Example 5: Persistent storage – changes survive the session ---
        static async Task PersistentStorageExample()
        {
            var storagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyHugeImageStorage");
            Directory.CreateDirectory(storagePath);

            using var storage = new PersistentHugeImageStorage(storagePath);

            // Create or update the image.
            using (var himage = new HugeImage<Rgb24>(storage, "myimage", new Size(50_000, 50_000)))
            {
                await himage.MutateAllParallelAsync(d =>
                {
                    d.Fill(new SolidBrush(Color.DarkGreen), new EllipsePolygon(new PointF(25_000, 25_000), 25_000));
                });
                // Flush to disk before disposing.
                await himage.OffloadAsync();
            }

            using var thumbnail = await himage_load(storage);
            await thumbnail.SaveAsPngAsync("persistent.png");

            static async Task<Image<Rgb24>> himage_load(PersistentHugeImageStorage storage)
            {
                using var himage = new HugeImage<Rgb24>(storage, "myimage", new Size(50_000, 50_000));
                return await himage.ToScaledImageAsync(500, 500);
            }
        }

        // --- Example 6: Save and load a HugeImage using the HugeImage format ---
        static async Task SaveAndLoadExample()
        {
            var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "myimage.himg");

            // Create and save.
            using (var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(50_000, 50_000)))
            {
                await himage.MutateAllParallelAsync(d =>
                {
                    d.Fill(new SolidBrush(Color.Purple), new EllipsePolygon(new PointF(25_000, 25_000), 25_000));
                });
                await himage.SaveAsync(filePath);
            }

            // Load a read-only locked copy (zero disk copy – reads directly from the archive).
            using var readOnly = await HugeImageIO.LoadReadOnlyLockedAsync<Rgb24>(filePath);
            using var thumbnail = await readOnly.ToScaledImageAsync(500, 500);
            await thumbnail.SaveAsPngAsync("loaded.png");

            // Load a mutable clone into new storage.
            using var storage = new TemporaryHugeImageStorage();
            using var mutable = await HugeImageIO.LoadCloneAsync<Rgb24>(filePath, storage);
            await mutable.MutateAllAsync(d => d.GaussianBlur(5));
            using var thumbnail2 = await mutable.ToScaledImageAsync(500, 500);
            await thumbnail2.SaveAsPngAsync("loaded_blurred.png");
        }
    }
}