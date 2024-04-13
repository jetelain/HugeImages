using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugeImages.Test.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace HugeImages.Test.IO
{
    internal class IOHelper
    {

        internal static async Task CheckSampleImage(HugeImageStorageMock storage1, MemoryStream mem)
        {
            var archive = new ZipArchive(mem, ZipArchiveMode.Read);
            Assert.Equal(5, archive.Entries.Count);

            var index = ReadIndex(archive);
            Assert.Equal(@"{""PartMimeType"":""image/png"",""Size"":[1000,1000],""Parts"":[{""PartId"":1,""FileName"":""1.png"",""Rectangle"":[0,0,500,500],""RealRectangle"":[0,0,506,506]},{""PartId"":2,""FileName"":""2.png"",""Rectangle"":[0,500,500,500],""RealRectangle"":[0,494,506,506]},{""PartId"":3,""FileName"":""3.png"",""Rectangle"":[500,0,500,500],""RealRectangle"":[494,0,506,506]},{""PartId"":4,""FileName"":""4.png"",""Rectangle"":[500,500,500,500],""RealRectangle"":[494,494,506,506]}],""Background"":""000000FF""}", index);

            await CheckPart(archive.GetEntry("1.png"), storage1.Storage[1]);
            await CheckPart(archive.GetEntry("2.png"), storage1.Storage[2]);
            await CheckPart(archive.GetEntry("3.png"), storage1.Storage[3]);
            await CheckPart(archive.GetEntry("4.png"), storage1.Storage[4]);
        }

        internal static async Task CheckPart(ZipArchiveEntry? zipArchiveEntry, Image expected)
        {
            Assert.NotNull(zipArchiveEntry);
            var stream = zipArchiveEntry!.Open();
            using var img = Image.Load<Rgb24>(stream);
            await Samples.AssertEqual((Image<Rgb24>)expected, img);
        }

        internal static string ReadIndex(ZipArchive archive)
        {
            var index = archive.GetEntry("index.json");
            Assert.NotNull(index);
            using (var reader = new StreamReader(index!.Open()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
