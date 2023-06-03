# HugeImages
Library to manipulate extremely large images with [ImageSharp](https://github.com/SixLabors/ImageSharp) from [SixLabors](https://sixlabors.com/)

Image is splitted into parts that can be handled safely by ImageSharp (default is 16 kilo x 16 kilo => 256 mega pixels, 1 GiB with 32bpp)

Use mass storage to limit memory consumption.

Can handle tera, or even peta, pixels images, depending on mass storage capacity and file format encoder performance (png by default).

Theoric limit is 2 giga x 2 giga => 4 exa pixels (16 EiB with 32bpp).

Note: ImageSharp drawing primitives are float32 encoded, this can result in precision loss on very large images (more than 1px error with dimensions above 8 mega x 8 mega square => 64 tera pixels, 256 TiB with 32bpp).

Each part have an overlap with adjacent parts to allow each processing operation to be done independtly on each part. This reduce the number of parts required to be simultaneously loaded into memory.
