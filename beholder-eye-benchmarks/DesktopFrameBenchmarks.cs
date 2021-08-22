namespace beholder_eye_benchmarks
{
  using beholder_eye;
  using BenchmarkDotNet.Attributes;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;

  public class DesktopFrameBenchmarks
  {
    private DesktopFrame AlignmentFrame;
    private DesktopFrame AlphaFrame;
    private DesktopFrame DataFrame;
    private DesktopFrame TestFrame;
    private IList<MatrixPixelLocation> AlignmentMap;
    private MatrixPixelLocation[] QuickMap;

    [GlobalSetup]
    public void GlobalSetup()
    {
      var mapJson = File.ReadAllText("./mocks/alignmentmap.json");
      AlignmentMap = JsonSerializer.Deserialize<IList<MatrixPixelLocation>>(mapJson);
      QuickMap = AlignmentMap.ToArray();

      AlignmentFrame = DesktopFrame.FromFile("./mocks/alignpattern.bmp");
      AlphaFrame = DesktopFrame.FromFile("./mocks/alphapattern.bmp");
      DataFrame = DesktopFrame.FromFile("./mocks/datapattern.bmp");
      TestFrame = DesktopFrame.FromFile("./mocks/testpattern.bmp");
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
      AlignmentMap = null;

      AlignmentFrame = null;
      AlphaFrame = null;
      DataFrame = null;
      TestFrame = null;
    }

    [Benchmark]
    public void GenerateAlignmentMap()
    {
      AlignmentFrame.GenerateAlignmentMap(2);
    }

    [Benchmark]
    public void DecodeAlphaFrame()
    {
      var rawData = AlphaFrame.DecodeMatrixFrameRaw(QuickMap);
      var result = MatrixFrame.CreateMatrixFrame(rawData, new MatrixSettings()
      {
        Map = AlignmentMap,
        DataFormat = DataMatrixFormat.Text,
      });

    }

    [Benchmark]
    public void DecodeMatrixFrame()
    {
      var rawData = DataFrame.DecodeMatrixFrameRaw(QuickMap);
      MatrixFrame.CreateMatrixFrame(rawData, new MatrixSettings()
      {
        Map = AlignmentMap,
        DataFormat = DataMatrixFormat.Json,
      });
    }

    [Benchmark]
    public void DecodeTestFrame()
    {
      var rawData = TestFrame.DecodeMatrixFrameRaw(QuickMap);
      MatrixFrame.CreateMatrixFrame(rawData, new MatrixSettings()
      {
        Map = AlignmentMap,
        DataFormat = DataMatrixFormat.Raw,
      });
    }

    [Benchmark]
    public void GetThumbnailImage()
    {
      DataFrame.GetThumbnailImage((int)Math.Ceiling(DataFrame.DesktopWidth * 0.15), (int)Math.Ceiling(DataFrame.DesktopHeight * 0.15));
    }
  }
}