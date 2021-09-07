namespace beholder_occipital.Util
{
  using beholder_occipital.Models;
  using OpenCvSharp;
  using System.Collections.Generic;
  using System.Linq;

  public static class OpenCvUtil
  {
    public static ImageStatistics CalculateImageStatistics(Mat image)
    {
      var mean = Cv2.Mean(image);
      var colors = GetDominantColors(image, 5);

      var blueHistogram = new Histogram()
      {
        Mean = mean.Val0,
      };

      var greenHistogram = new Histogram()
      {
        Mean = mean.Val1,
      };

      var redHistogram = new Histogram()
      {
        Mean = mean.Val2,
      };

      return new ImageStatistics()
      {
        Blue = blueHistogram,
        Green = greenHistogram,
        Red = redHistogram,
        DominantColors = colors,
      };
    }

    /// <summary>
    /// Color Quantization using K-Means Clustering in OpenCVSharp.
    /// </summary>
    /// <param name="input">Input image.</param>
    /// <param name="k">Number of colors required.</param>
    public static IList<Color> GetDominantColors(Mat input, int k)
    {
      using Mat pixels = new Mat();
      using Mat labels = new Mat();
      using Mat centers = new Mat();

      var scale = 0.5;
      var width = input.Cols * scale;
      var height = input.Rows * scale;

      input.ConvertTo(pixels, MatType.CV_32FC3);
      //pixels.Reshape(1, (int)input.Total());
      Cv2.Resize(pixels, pixels, new Size(width, height));
      //Cv2.CvtColor(pixels, pixels, ColorConversionCodes.BGR2RGB);

      pixels.SaveImage("foo.png");
      //int width = input.Cols;
      //int height = input.Rows;

      // Criteria:
      // – Stop the algorithm iteration if specified accuracy, epsilon, is reached.
      // – Stop the algorithm after the specified number of iterations, MaxIter.
      var criteria = new TermCriteria(CriteriaTypes.MaxIter, maxCount: 10, epsilon: 1.0);

      // Finds centers of clusters and groups input samples around the clusters.
      Cv2.Kmeans(data: pixels, k: k, bestLabels: labels, criteria: criteria, attempts: 3, flags: KMeansFlags.PpCenters, centers);

      var colors = new List<Color>();
      for (int i = 0; i < centers.Rows; i++)
      {
        var color = centers.At<Vec3f>(i, 0);

        colors.Add(new Color()
        {
          Red = (int)color.Item2,
          Green = (int)color.Item1,
          Blue = (int)color.Item0,
        });
        
      }

      return colors;

      //centers.SaveImage("foo.png");
      //int i = 0;
      //var colors = new Dictionary<Color, int>();
      //for (int y = 0; y < height; y++)
      //{
      //  for (int x = 0; x < width; x++, i++)
      //  {
      //    var f = input.At<Vec3b>(y, x);
      //    var color = new Color()
      //    {
      //      Red = f.Item2,
      //      Green = f.Item1,
      //      Blue = f.Item0,
      //    };
      //    if (!colors.ContainsKey(color))
      //    {
      //      colors.Add(color, 1);
      //    }
      //    else
      //    {
      //      colors[color]++;
      //    }
      //  }
      //}
      //return colors
      //  .OrderBy(k => k.Value)
      //  .Select(k => k.Key)
      //  .Take(k)
      //  .ToList();
    }
  }
}
