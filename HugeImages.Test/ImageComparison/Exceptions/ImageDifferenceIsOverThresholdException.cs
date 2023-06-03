// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Text;

namespace SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

public class ImageDifferenceIsOverThresholdException : ImagesSimilarityException
{
    public ImageSimilarityReport[] Reports { get; }

    public ImageDifferenceIsOverThresholdException(IEnumerable<ImageSimilarityReport> reports)
        : base("Image difference is over threshold!" + StringifyReports(reports))
        => this.Reports = reports.ToArray();

    private static string StringifyReports(IEnumerable<ImageSimilarityReport> reports)
    {
        StringBuilder sb = new();

        foreach (ImageSimilarityReport r in reports)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "Report ImageFrame {0}: ", r.Index)
              .Append(r)
              .Append(Environment.NewLine);
        }

        return sb.ToString();
    }
}
