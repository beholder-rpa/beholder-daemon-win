namespace beholder_occipital.Util
{
  using beholder_occipital.Models;
  using OpenCvSharp;
  using System;
  using System.Collections.Generic;

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
      using Mat pixels = new();
      using Mat labels = new();
      using Mat centers = new();

      var width = input.Cols;
      var height = input.Rows;

      pixels.Create(width * height, 1, MatType.CV_32FC3);
      centers.Create(k, 1, pixels.Type());

      // Input Image Data
      int ix = 0;
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++, ix++)
        {
          var val = input.At<Vec3b>(y, x);
          var vec3f = new Vec3f
          {
            Item0 = val.Item0,
            Item1 = val.Item1,
            Item2 = val.Item2
          };

          pixels.Set(ix, vec3f);
        }
      }

      // Criteria:
      // – Stop the algorithm iteration if specified accuracy, epsilon, is reached.
      // – Stop the algorithm after the specified number of iterations, MaxIter.
      var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, maxCount: 10000, epsilon: 0.01);

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
    }
  }
}
