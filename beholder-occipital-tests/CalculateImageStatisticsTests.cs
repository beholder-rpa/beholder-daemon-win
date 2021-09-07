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

      Assert.Equal(1, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 0, Green = 0, Blue = 255 }, stats.DominantColors[0]);
    }

    [Fact]
    public void RedTest()
    {
      using var queryImage = Cv2.ImRead("./Images/redSquare.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(255, stats.Red.Mean);
      Assert.Equal(0, stats.Green.Mean);
      Assert.Equal(0, stats.Blue.Mean);

      Assert.Equal(1, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 255, Green = 0, Blue = 0 }, stats.DominantColors[0]);
    }

    [Fact]
    public void GreenTest()
    {
      using var queryImage = Cv2.ImRead("./Images/greenSquare.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(0, stats.Red.Mean);
      Assert.Equal(255, stats.Green.Mean);
      Assert.Equal(0, stats.Blue.Mean);

      Assert.Equal(1, stats.DominantColors.Count);
      Assert.Equal(new Color() { Red = 0, Green = 255, Blue = 0 }, stats.DominantColors[0]);
    }

    [Fact]
    public void CookieTest()
    {
      using var queryImage = Cv2.ImRead("./Images/goldCookie.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(193.872, Math.Round(stats.Red.Mean, 3));
      Assert.Equal(172.660, Math.Round(stats.Green.Mean, 3));
      Assert.Equal(136.247, Math.Round(stats.Blue.Mean, 3));

      Assert.True(stats.DominantColors.Count <= 5);
      Assert.Equal(new Color() { Red = 180, Green = 164, Blue = 133 }, stats.DominantColors[0]);
    }

    [Fact]
    public void RedCookieTest()
    {
      using var queryImage = Cv2.ImRead("./Images/redCookie.png");
      var stats = OpenCvUtil.CalculateImageStatistics(queryImage);
      Assert.Equal(190.683, Math.Round(stats.Red.Mean, 3));
      Assert.Equal(126.007, Math.Round(stats.Green.Mean, 3));
      Assert.Equal(109.296, Math.Round(stats.Blue.Mean, 3));

      Assert.True(stats.DominantColors.Count <= 5);
      Assert.Equal(new Color() { Red = 188, Green = 79, Blue = 74 }, stats.DominantColors[0]);
    }
  }
}
