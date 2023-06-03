# HugeImages
Library to manipulate extremely large images with [ImageSharp](https://github.com/SixLabors/ImageSharp) from [SixLabors](https://sixlabors.com/)

Image is splitted into parts that can be handled safely by ImageSharp (default is 16 kilo x 16 kilo => 256 mega pixels, 1 GiB with 32bpp)

Use mass storage to limit memory consumption.

Can handle tera, or even peta, pixels images, depending on mass storage capacity and file format encoder performance (png by default).

Theoric limit is 2 giga x 2 giga => 4 exa pixels (16 EiB with 32bpp).

Note: ImageSharp drawing primitives are float32 encoded, this can result in precision loss on very large images (more than 1px error with dimensions above 8 mega x 8 mega square => 64 tera pixels, 256 TiB with 32bpp).

Each part have an overlap with adjacent parts to allow each processing operation to be done independtly on each part. This reduce the number of parts required to be simultaneously loaded into memory.

## Usage

### Drawing / Processing

The class `HugeImage<TPixel>` is intend to be used like a regular `Image<TPixel>` but most operations are async to allow I/O operations.

The Mutate method is replaced by differents methods :
- `MutateAsync` : applies a mutation with automatic detection of affected area. Supplied code is called only once, but operation list is kept into memory. If operation list is very large it may consumme a lot of memory.
- `MutateAreaAsync` : applies a mutation on specified area only. Supplied code is called for each affected part.
- `MutateAllAsync` : applies a mutation that affects all surface of virtual image. Supplied code is called for each part of image.

Each method have a Parallel variation that will parallelize mutations on each affected part. Those Parallel methods are suitable only for Fill and Draw operations, as most operations are already parallelized by ImageSharp.

```cs
using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));
// Needs ~6 GiB of RAM + 30 MiB of mass storage
// Regular ImageSharp will ask for ~30 GiB do to the same
await himage.MutateAllParallelAsync(d =>
{
    d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
});
// Needs ~8 GiB of RAM (GaussianBlur uses a double-buffer)
// Regular ImageSharp will ask for ~60 GiB do to the same, but operation will fail due to an internal error
await himage.MutateAllAsync(d =>
{
    d.GaussianBlur(10); 
});
```

Note: You can reduce memory limit with `HugeImageSettings`.

### Thumbnail 

The method `ToScaledImageAsync` can be used to generate a regular (scaled) image.

```cs
using var thumbail = await himage.ToScaledImageAsync(1000, 1000);
```

### Tiling

To extract a part of an HugeImage, you can use the `DrawHugeImageAsync` on a regular Image.

This method takes as argument :
- HugeImage
- coordinates in the source HugeImage
- coordinates in the target Image (optionnal, (0,0) by default)
- size of the draw operation (optionnal, size of target Image by default)
- opacity (optionnal, 1 by default)

```cs
using var slice = new Image<Rgb24>(1000, 1000);
slice.Mutate(d =>
{
    d.DrawHugeImage(himage, new Point(49_500, 0));
});
```
