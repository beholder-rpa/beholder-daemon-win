namespace beholder_occipital_tests
{
  using beholder_occipital.Models;
  using beholder_occipital.Util;
  using OpenCvSharp;
  using System;
  using Xunit;

  public class CalculateImageStatisticsTests
  {
    [Fact]
    public void BlueTest()
    {
      using var queryImage = Cv2.ImRead("./Images/blueSquare.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(0, stats.Red.Mean);
      Assert.Equal(0, stats.Green.Mean);
      Assert.Equal(255, stats.Blue.Mean);

      Assert.Equal(5, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[0]);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[1]);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[2]);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[3]);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[4]);
    }

    [Fact]
    public void RedTest()
    {
      using var queryImage = Cv2.ImRead("./Images/redSquare.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(255, stats.Red.Mean);
      Assert.Equal(0, stats.Green.Mean);
      Assert.Equal(0, stats.Blue.Mean);

      Assert.Equal(5, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[0]);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[1]);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[2]);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[3]);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[4]);
    }

    [Fact]
    public void GreenTest()
    {
      using var queryImage = Cv2.ImRead("./Images/greenSquare.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(0, stats.Red.Mean);
      Assert.Equal(255, stats.Green.Mean);
      Assert.Equal(0, stats.Blue.Mean);

      Assert.Equal(5, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[0]);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[1]);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[2]);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[3]);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[4]);
    }

    [Fact]
    public void CookieTest()
    {
      using var queryImage = Cv2.ImRead("./Images/goldCookie.png");
      {
        var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
        Assert.Equal(193.872, Math.Round(stats.Red.Mean, 3));
        Assert.Equal(172.660, Math.Round(stats.Green.Mean, 3));
        Assert.Equal(136.247, Math.Round(stats.Blue.Mean, 3));

        Assert.Equal(5, stats.DominantColors.Count);

        Assert.Contains(new Color() { Red = 202, Green = 172, Blue = 90 }, stats.DominantColors); // Delicious cookie color
        Assert.Contains(new Color() { Red = 254, Green = 254, Blue = 253 }, stats.DominantColors); // White background
        Assert.Contains(new Color() { Red = 100, Green = 65, Blue = 38 }, stats.DominantColors); // Chips
        Assert.Contains(new Color() { Red = 157, Green = 121, Blue = 64 }, stats.DominantColors); // cookie color again
        Assert.Contains(new Color() { Red = 231, Green = 210, Blue = 139 }, stats.DominantColors); // cookie color again
      }

      // Ensure we're non-deterministic
      using var queryImage2 = Cv2.ImRead("./Images/goldCookie.png");
      {
        var stats2 = OpenCvUtil.CalculateImageStatistics(queryImage2);
        Assert.Equal(193.872, Math.Round(stats2.Red.Mean, 3));
        Assert.Equal(172.660, Math.Round(stats2.Green.Mean, 3));
        Assert.Equal(136.247, Math.Round(stats2.Blue.Mean, 3));

        Assert.Equal(5, stats2.DominantColors.Count);

        Assert.Contains(new Color() { Red = 226, Green = 203, Blue = 129 }, stats2.DominantColors); // Delicious cookie color
        Assert.Contains(new Color() { Red = 254, Green = 254, Blue = 253 }, stats2.DominantColors); // White background
        Assert.Contains(new Color() { Red = 183, Green = 150, Blue = 78 }, stats2.DominantColors); // Chips
        Assert.Contains(new Color() { Red = 81, Green = 49, Blue = 30 }, stats2.DominantColors); // cookie color again
        Assert.Contains(new Color() { Red = 132, Green = 93, Blue = 52 }, stats2.DominantColors); // cookie color again
      }

      // Ensure we're non-deterministic
      using var queryImage3 = Cv2.ImRead("./Images/goldCookie.png");
      {
        var stats2 = OpenCvUtil.CalculateImageStatistics(queryImage3);
        Assert.Equal(193.872, Math.Round(stats2.Red.Mean, 3));
        Assert.Equal(172.660, Math.Round(stats2.Green.Mean, 3));
        Assert.Equal(136.247, Math.Round(stats2.Blue.Mean, 3));

        Assert.Equal(5, stats2.DominantColors.Count);

        Assert.Contains(new Color() { Red = 226, Green = 203, Blue = 129 }, stats2.DominantColors); // Delicious cookie color
        Assert.Contains(new Color() { Red = 254, Green = 254, Blue = 253 }, stats2.DominantColors); // White background
        Assert.Contains(new Color() { Red = 183, Green = 150, Blue = 78 }, stats2.DominantColors); // Chips
        Assert.Contains(new Color() { Red = 81, Green = 49, Blue = 30 }, stats2.DominantColors); // cookie color again
        Assert.Contains(new Color() { Red = 132, Green = 93, Blue = 52 }, stats2.DominantColors); // cookie color again
      }
    }

    [Fact]
    public void RedCookieTest()
    {
      using var queryImage = Cv2.ImRead("./Images/redCookie.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(190.683, Math.Round(stats.Red.Mean, 3));
      Assert.Equal(126.007, Math.Round(stats.Green.Mean, 3));
      Assert.Equal(109.296, Math.Round(stats.Blue.Mean, 3));

      Assert.Equal(5, stats.DominantColors.Count);
      Assert.Contains(new Color() { Red = 239, Green = 180, Blue = 124 }, stats.DominantColors);
      Assert.Contains(new Color() { Red = 152, Green = 38, Blue = 23 }, stats.DominantColors);
      Assert.Contains(new Color() { Red = 209, Green = 96, Blue = 48 }, stats.DominantColors);
      Assert.Contains(new Color() { Red = 254, Green = 253, Blue = 253 }, stats.DominantColors);
      Assert.Contains(new Color() { Red = 83, Green = 16, Blue = 13 }, stats.DominantColors);
    }

    [Fact]
    public void JpTest()
    {
      using var queryImage = Cv2.ImRead("./Images/jp.png");
      {
        var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
        Assert.Equal(32.032, Math.Round(stats.Red.Mean, 3));
        Assert.Equal(17.187, Math.Round(stats.Green.Mean, 3));
        Assert.Equal(10.552, Math.Round(stats.Blue.Mean, 3));

        Assert.Equal(5, stats.DominantColors.Count);
        Assert.Contains(new Color() { Red = 0, Green = 0, Blue = 0 }, stats.DominantColors); // black
        Assert.Contains(new Color() { Red = 251, Green = 248, Blue = 247 }, stats.DominantColors); // white (text)
        Assert.Contains(new Color() { Red = 241, Green = 3, Blue = 2 }, stats.DominantColors); // red
        Assert.Contains(new Color() { Red = 251, Green = 252, Blue = 4 }, stats.DominantColors); // yellow
        Assert.Contains(new Color() { Red = 163, Green = 132, Blue = 95 }, stats.DominantColors); // Orangish.. between red and yellow
      }

      using var queryImage2 = Cv2.ImRead("./Images/jp.png");
      {
        var stats2 = OpenCvUtil.CalculateImageStatistics(queryImage2);
        Assert.Equal(32.032, Math.Round(stats2.Red.Mean, 3));
        Assert.Equal(17.187, Math.Round(stats2.Green.Mean, 3));
        Assert.Equal(10.552, Math.Round(stats2.Blue.Mean, 3));

        Assert.Equal(5, stats2.DominantColors.Count);
        Assert.Contains(new Color() { Red = 0, Green = 0, Blue = 0 }, stats2.DominantColors); // black
        Assert.Contains(new Color() { Red = 251, Green = 248, Blue = 247 }, stats2.DominantColors); // white (text)
        Assert.Contains(new Color() { Red = 241, Green = 3, Blue = 2 }, stats2.DominantColors); // red
        Assert.Contains(new Color() { Red = 251, Green = 252, Blue = 4 }, stats2.DominantColors); // yellow
        Assert.Contains(new Color() { Red = 163, Green = 132, Blue = 95 }, stats2.DominantColors); // Orangish.. between red and yellow
      }
    }
  }
}
