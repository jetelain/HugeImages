using Pmad.HugeImages;
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
            // With HugeImage
            // -> Needs ~6 GiB of RAM + 30 MiB of mass storage
            using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));
            await himage.MutateAllParallelAsync(d =>
            {
                d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            });
            // Most ImaheSharp operations are multi-threaded, MutateAllParallelAsync is not usefull for them
            // -> Needs ~8 GiB of RAM, due to double-buffer requirement
            //await himage.MutateAllAsync(d =>
            //{
            //    d.GaussianBlur(10); 
            //});
            await himage.OffloadAsync();

            // Without HugeImage
            // -> Needs ~30 GiB of RAM
            // using var simage = new Image<Rgb24>(100_000, 100_000);
            // simage.Mutate(d =>
            // {
            //     d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
            // });
            // -> Ask for ~60 GiB of RAM (if your hardware can), and will fail with ArgumentOutOfRangeException
            // simage.Mutate(d =>
            // {
            //     d.GaussianBlur(10);
            // });

            // Take a slice of the HugeImage (49_500,0)->(50_500,1000)
            using var slice = new Image<Rgb24>(1000, 1000);
            slice.Mutate(d =>
            {
                d.DrawHugeImage(himage, new Point(49_500, 0));
            });
            await slice.SaveAsPngAsync("circletop.png");

            // Create a thumbail of the HugeImage
            using var thumbail = await himage.ToScaledImageAsync(1000, 1000);
            await thumbail.SaveAsPngAsync("circle.png");

        }
    }
}