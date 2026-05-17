# HugeImages

A .NET library for manipulating extremely large images with [ImageSharp](https://github.com/SixLabors/ImageSharp) from [SixLabors](https://sixlabors.com/).

## Overview

The virtual image is split into overlapping tiles (*parts*) that ImageSharp can handle safely. Only a configurable number of tiles are kept in RAM at any time; the rest are stored on disk. This drastically reduces memory requirements compared to loading the whole image at once.

| Approach | 100 000 x 100 000 fill | 100 000 x 100 000 blur |
|---|---|---|
| Regular ImageSharp | ~30 GiB RAM | ~60 GiB RAM (fails in practice) |
| **HugeImages** | **~6 GiB RAM + ~30 MiB disk** | **~8 GiB RAM** |

Default configuration: tiles of 16 384 x 16 384 pixels (1 GiB each at 32 bpp) with a 16-pixel overlap.

**Theoretical image size limit**: 2 giga x 2 giga = 4 exa-pixels (16 EiB at 32 bpp).

> **Precision note** - ImageSharp drawing primitives use 32-bit floats. Coordinates above ~8 mega pixels may lose sub-pixel precision (> 1 px error for dimensions above 8 M x 8 M).

---

## Installation

```
dotnet add package Pmad.HugeImages
```

---

## Quick start

```csharp
using Pmad.HugeImages;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

// Creates a 100 000 x 100 000 image using temporary disk storage.
// ~6 GiB RAM + ~30 MiB disk (vs ~30 GiB RAM with regular ImageSharp).
using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000));

// Fill and draw operations are independent per tile - use the Parallel variant.
await himage.MutateAllParallelAsync(d =>
{
    d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(new PointF(50_000, 50_000), 50_000));
});

// Most ImageSharp operations are already multi-threaded; use the non-parallel variant.
// (~8 GiB RAM due to double-buffer requirement; ~60 GiB with regular ImageSharp, and fails with an error)
await himage.MutateAllAsync(d =>
{
    d.GaussianBlur(10);
});

// Flush all tiles from RAM to disk.
await himage.OffloadAsync();
```

---

## Drawing and mutating

HugeImage<TPixel> is designed to be used like a regular Image<TPixel>, but all mutation methods are async to allow transparent I/O.

The Mutate method is replaced by several methods:

| Method | Description |
|---|---|
| MutateAllAsync | Applies the operation to every tile sequentially. |
| MutateAllParallelAsync | Same, but tiles are processed in parallel. Best for fill/draw operations. |
| MutateAreaAsync | Applies the operation only to tiles that intersect the given rectangle. |
| MutateAreaParallelAsync | Same, but tiles are processed in parallel. |
| MutateAsync | Detects the affected area automatically, then delegates to MutateAreaAsync. The operation list is buffered in memory first. |
| MutateParallelAsync | Like MutateAsync but parallelises tile processing. |

```csharp
// Mutate only a specific area.
var area = new Rectangle(40_000, 40_000, 20_000, 20_000);
await himage.MutateAreaAsync(area, d =>
{
    d.Fill(Color.Red, area);
});

// Mutate with automatic affected-area detection.
await himage.MutateAsync(d =>
{
    d.Fill(new SolidBrush(Color.Green), new EllipsePolygon(new PointF(50_000, 50_000), 5_000));
});
```

> Use the Parallel variants only for **fill and draw** operations. Most ImageSharp processing operations (resize, blur, ...) are already parallelised internally.

---

## Thumbnail

```csharp
using var thumbnail = await himage.ToScaledImageAsync(1000, 1000);
await thumbnail.SaveAsPngAsync("thumbnail.png");
```

---

## Tiling - extract a slice

Use DrawHugeImage (or DrawHugeImageAsync) on a regular Image to extract any rectangular area of a HugeImage.

```csharp
// Extract the top of the circle: source area (49 500, 0) -> (50 500, 1 000).
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
```

DrawHugeImage arguments:
1. sourceImage - the HugeImage<TPixel> to read from.
2. sourceLocation - top-left corner of the area to read in the source image.
3. targetLocation (optional, default (0,0)) - top-left corner of the destination in the target image.
4. size (optional, default: target image size) - size of the area to copy.
5. opacity (optional, default 1) - blending opacity.

---

## Storage backends

| Class | Description |
|---|---|
| TemporaryHugeImageStorage | Writes tiles to a randomly-named folder in the system temp directory. All files are deleted on Dispose. |
| PersistentHugeImageStorage | Writes tiles under a given directory. Tiles survive the process lifetime. |
| MemoryHugeImageStorage | Keeps tiles in RAM. Intended for unit testing. |

```csharp
// Persistent storage: changes survive the application session.
var storagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyHugeImageStorage");
Directory.CreateDirectory(storagePath);

using var storage = new PersistentHugeImageStorage(storagePath);

using (var himage = new HugeImage<Rgb24>(storage, "myimage", new Size(50_000, 50_000)))
{
    await himage.MutateAllParallelAsync(d =>
    {
        d.Fill(new SolidBrush(Color.DarkGreen), new EllipsePolygon(new PointF(25_000, 25_000), 25_000));
    });
    // Always flush before disposing when using persistent storage.
    await himage.OffloadAsync();
}
```

---

## Custom settings

HugeImageSettings controls partitioning and memory usage.

```csharp
var settings = new HugeImageSettings
{
    // Limit RAM usage to 2 GiB for this image instance.
    MemoryLimit = 2L * 1024 * 1024 * 1024,
    // Reduce tile size to 8192 x 8192 (256 MiB at 32 bpp).
    PartMaxSize = 8192,
    // Set overlap to 32 pixels to support blur radius up to 32.
    PartOverlap = 32
};

using var himage = new HugeImage<Rgb24>(new TemporaryHugeImageStorage(), new Size(100_000, 100_000), settings);
```

| Property | Default | Description |
|---|---|---|
| MemoryLimit | 6 GiB | Controls how many tiles may be loaded simultaneously: `maxLoadedParts = MemoryLimit / (tileWidth * tileHeight * bytesPerPixel)`. See note below. |
| PartMaxSize | 16 384 | Maximum width/height of a single tile, **including** the overlap border. |
| PartOverlap | 16 | Overlap in pixels between adjacent tiles. Must be >= the heaviest operation radius (e.g. blur radius). |
| StorageFormat | PNG | Image format used to encode tiles on disk. |
| Configuration | Configuration.Default | ImageSharp configuration. |

> **MemoryLimit is not a hard memory cap.** It only determines how many tiles are kept in RAM at once. It does *not* account for ImageSharp processor buffers (e.g. a Gaussian blur needs a second buffer of equal size), thumbnails or other intermediate images, encoder/decoder working memory, or .NET object overhead. To stay within a physical-memory budget, set `MemoryLimit` well below the available RAM to leave headroom for these additional costs.

> **Choosing PartOverlap**: if you plan to apply a GaussianBlur with radius 20, set PartOverlap to at least 20. Insufficient overlap will cause visible seam artefacts at tile boundaries.

---

## Save and load

HugeImages provides its own archive format (.himg) that bundles all tiles in a single file.

```csharp
using Pmad.HugeImages.IO;

var filePath = "myimage.himg";

// Save to the HugeImage archive format.
await himage.SaveAsync(filePath);

// Load as read-only (zero disk-copy; archive file stays open until disposed).
using var readOnly = await HugeImageIO.LoadReadOnlyLockedAsync<Rgb24>(filePath);

// Load a mutable clone into new storage.
using var storage = new TemporaryHugeImageStorage();
using var mutable = await HugeImageIO.LoadCloneAsync<Rgb24>(filePath, storage);
await mutable.MutateAllAsync(d => d.GaussianBlur(5));
using var thumbnail = await mutable.ToScaledImageAsync(500, 500);
await thumbnail.SaveAsPngAsync("loaded_blurred.png");
```