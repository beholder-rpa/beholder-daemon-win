namespace beholder_eye
{
  using SixLabors.ImageSharp;
  using SixLabors.ImageSharp.PixelFormats;
  using SixLabors.ImageSharp.Processing;
  using System;
  using System.Collections.Generic;
  using System.Drawing.Imaging;
  using System.IO;
  using System.Linq;
  using Point = System.Drawing.Point;
  using Rectangle = System.Drawing.Rectangle;

  /// <summary>
  /// Provides image data, cursor data, and image metadata about the retrieved desktop frame.
  /// </summary>
  public sealed class DesktopFrame
  {
    public DesktopFrame()
    {
      PointerPosition = new PointerPosition();
      PointerShape = new PointerShape();
    }

    /// <summary>
    /// Gets the buffer representing the last retrieved desktop frame. This image spans the entire bounds of the specified monitor.
    /// </summary>
    internal byte[] DesktopFrameBuffer { get; set; }

    /// <summary>
    /// Gets the buffer containing a 32bit argb bitmap representing the last retrieved pointer.
    /// </summary>
    internal byte[] PointerShapeBuffer { get; set; }

    /// <summary>
    /// Gets the desktop width
    /// </summary>
    public int DesktopWidth { get; internal set; }

    /// <summary>
    /// Gets the desktop height
    /// </summary>
    public int DesktopHeight { get; internal set; }

    /// <summary>
    /// Gets a list of the rectangles of pixels in the desktop image that the operating system moved to another location within the same image.
    /// </summary>
    /// <remarks>
    /// To produce a visually accurate copy of the desktop, an application must first process all moved regions before it processes updated regions.
    /// </remarks>
    public IList<MovedRegion> MovedRegions { get; internal set; }

    /// <summary>
    /// Returns the list of non-overlapping rectangles that indicate the areas of the desktop image that the operating system updated since the last retrieved frame.
    /// </summary>
    /// <remarks>
    /// To produce a visually accurate copy of the desktop, an application must first process all moved regions before it processes updated regions.
    /// </remarks>
    public IList<Rectangle> UpdatedRegions { get; internal set; }

    /// <summary>
    /// The number of frames that the operating system accumulated in the desktop image surface since the last retrieved frame.
    /// </summary>
    public int AccumulatedFrames { get; internal set; }

    /// <summary>
    /// Gets the current position of the pointer on the image
    /// </summary>
    public PointerPosition PointerPosition { get; internal set; }

    /// <summary>
    /// Gets the information representing the current pointer.
    /// </summary>
    public PointerShape PointerShape { get; internal set; }

    /// <summary>
    /// Gets whether the desktop image contains protected content that was already blacked out in the desktop image.
    /// </summary>
    public bool ProtectedContentMaskedOut { get; internal set; }

    /// <summary>
    /// Gets whether the operating system accumulated updates by coalescing updated regions. If so, the updated regions might contain unmodified pixels.
    /// </summary>
    public bool RectanglesCoalesced { get; internal set; }

    /// <summary>
    /// Gets a value that indicates if the desktop image is completely black.
    /// </summary>
    /// <returns></returns>
    public bool IsDesktopImageBufferEmpty { get; internal set; }

    /// <summary>
    /// Generates an alignment map - a collection of rectangles that make up the locations of the rectangles that will be monitored for data.
    /// </summary>
    /// <param name="minimumBlockSize"></param>
    /// <returns></returns>
    public IList<MatrixPixelLocation> GenerateAlignmentMap(int minimumBlockSize = 2)
    {
      if (DesktopFrameBuffer == null || DesktopFrameBuffer.Length <= 0 || IsDesktopImageBufferEmpty)
      {
        return null;
      }

      var pixels = new List<(Point loc, int width)>();
      var span = new ReadOnlySpan<byte>(DesktopFrameBuffer);

      static bool isGreen(Bgra32 pixel)
      {
        return pixel.R == 0 && pixel.G == 255 && pixel.B == 0;
      }

      // First pass - get location of pixels rows that are consecutively green
      using var image = Image.LoadPixelData<Bgra32>(span, DesktopWidth, DesktopHeight);
      {
        for (int y = 0; y < image.Height; y++)
        {
          Span<Bgra32> pixelRowSpan = image.GetPixelRowSpan(y);
          for (int x = 0; x < image.Width; x++)
          {
            for (var x1 = x; x1 < image.Width; x1++)
            {
              var nextPixel = pixelRowSpan[x1];
              if (!isGreen(nextPixel))
              {
                if (x1 - x >= minimumBlockSize)
                {
                  pixels.Add((new Point(x, y), x1 - x));
                }

                // Fast forward to the current position
                x = x1;
                break;
              }
            }
          }
        }
      }

      // Second pass - get blocks of the minimum size.
      var blocks = new List<Rectangle>();
      var rows = pixels.OrderBy(p => p.loc.X).ThenBy(p => p.loc.Y).ToArray();
      for (int i = 0; i < rows.Length; i++)
      {
        var (loc, width) = rows[i];
        if (loc.X == 882 && loc.Y == 2147)
        {
          var foo = rows.TakeLast(rows.Length - i);
        }

        for (int i1 = i + 1; i1 < rows.Length; i1++)
        {
          var nextLoc = rows[i1].loc;
          if (nextLoc.X != loc.X || nextLoc.Y != loc.Y + (i1 - i) || i1 == rows.Length - 1)
          {
            if (i1 - i >= minimumBlockSize)
            {
              blocks.Add(new Rectangle(loc, new System.Drawing.Size(width, i1 - i)));
            }
            // Fast forward to the current position
            i = i1 - 1;
            break;
          }
        }
      }

      return blocks
          .OrderBy(b => b.Y)
          .ThenBy(p => p.X)
          .Select((b, ix) => new MatrixPixelLocation() { X = b.X, Y = b.Y, Width = b.Width, Height = b.Height, Index = ix })
          .ToList();
    }

    public MatrixFrame DecodeMatrixFrame(MatrixSettings settings)
    {
      var outOfBoundsHeight = settings.Map.Select(m => m.Y).FirstOrDefault(yPos => yPos > DesktopHeight);
      if (outOfBoundsHeight != default)
      {
        throw new InvalidOperationException($"Matrix Settings contained a matrix pixel location ({outOfBoundsHeight}) beyond the height of the current desktop ({DesktopHeight}). Please ensure the matrix map for the region is valid.");
      }

      var outOfBoundsWidth = settings.Map.Select(m => m.X).FirstOrDefault(xPos => xPos > DesktopWidth);
      if (outOfBoundsWidth != default)
      {
        throw new InvalidOperationException($"Matrix Settings contained a matrix pixel location ({outOfBoundsWidth}) beyond the width of the current desktop ({DesktopWidth}). Please ensure the matrix map for the region is valid.");
      }

      var rawData = DecodeMatrixFrameRaw(settings.Map);
      return MatrixFrame.CreateMatrixFrame(rawData, settings);
    }

    public byte[] DecodeMatrixFrameRaw(IList<MatrixPixelLocation> map)
    {
      return DecodeMatrixFrameRaw(map.ToArray());
    }

    public byte[] DecodeMatrixFrameRaw(MatrixPixelLocation[] map)
    {
      if (DesktopFrameBuffer == null || DesktopFrameBuffer.Length <= 0 || IsDesktopImageBufferEmpty)
      {
        return null;
      }

      if (map == null)
      {
        throw new ArgumentNullException(nameof(map));
      }

      if (map == null || map.Length == 0)
      {
        throw new InvalidOperationException("The matrix map specified in the settings must be non-null and contain at least 1 matrix pixel location.");
      }

      var bpp = System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppRgb) / 8;

      var rawData = new byte[map.Length * 3];

      var span = new ReadOnlySpan<byte>(DesktopFrameBuffer);
      for (int i = 0; i < map.Length; i++)
      {
        var px = map[i];
        var ix = px.Y * DesktopWidth * bpp + px.X * bpp;
        rawData[px.Index * 3] = span[ix + 2];
        rawData[px.Index * 3 + 1] = span[ix + 1];
        rawData[px.Index * 3 + 2] = span[ix];
      }

      return rawData;
    }

    /// <summary>
    /// Creates a base64 encoded thumbnail image of the current desktop image with the specified width and height.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public byte[] GetThumbnailImage(int width, int height)
    {
      if (DesktopFrameBuffer == null || DesktopFrameBuffer.Length <= 0 || IsDesktopImageBufferEmpty)
      {
        return null;
      }

      var span = new ReadOnlySpan<byte>(DesktopFrameBuffer);
      using var ms = new MemoryStream();
      using var image = Image.LoadPixelData<Bgra32>(span, DesktopWidth, DesktopHeight);
      {
        image.Mutate(x => x
             .Resize(width, height));

        image.SaveAsPng(ms);
      }
      return ms.ToArray();
    }

    public byte[] GetSnapshot(int width, int height, SnapshotFormat format)
    {
      if (DesktopFrameBuffer == null || DesktopFrameBuffer.Length <= 0 || IsDesktopImageBufferEmpty)
      {
        return null;
      }

      var span = new ReadOnlySpan<byte>(DesktopFrameBuffer);
      using var ms = new MemoryStream();
      using var image = Image.LoadPixelData<Bgra32>(span, DesktopWidth, DesktopHeight);
      {
        image.Mutate(x => x
             .Resize(width, height));

        switch (format)
        {
          case SnapshotFormat.Jpeg:
            image.SaveAsJpeg(ms);
            break;
          case SnapshotFormat.Png:
          default:
            image.SaveAsPng(ms);
            break;
        }
      }
      return ms.ToArray();
    }

    public (byte[], Rectangle) GetRegion(int x, int y, int width, int height)
    {
      if (x > DesktopWidth) { x = DesktopWidth; }
      if (y > DesktopHeight) { y = DesktopHeight; }
      if (x < 0) { x = 0; }
      if (y < 0) { y = 0; }
      if (width > DesktopWidth) { width = DesktopWidth; }
      if (height > DesktopHeight) { height = DesktopHeight; }
      if (width < 0) { width = 0; }
      if (height < 0) { height = 0; }

      if ((x + width) > DesktopWidth) { width = DesktopWidth - x; }
      if ((x + height) > DesktopHeight) { height = DesktopHeight - y; }

      if (width == 0 || height == 0)
      {
        return (null, new Rectangle(0, 0, 0, 0));
      }

      var span = new ReadOnlySpan<byte>(DesktopFrameBuffer);
      using var ms = new MemoryStream();
      using var image = Image.LoadPixelData<Bgra32>(span, DesktopWidth, DesktopHeight);
      {
        var regionImage = image.Clone(i => i.Crop(new SixLabors.ImageSharp.Rectangle(x, y, width, height)));
        regionImage.SaveAsPng(ms);
      }
      return (ms.ToArray(), new Rectangle(x, y, width, height));
    }

    public byte[] GetPointerImage()
    {
      if (PointerShapeBuffer == null)
      {
        return null;
      }

      if (PointerShapeBuffer.Length != PointerShape.Width.Value * PointerShape.Height.Value * 4)
      {
        return null;
      }

      var span = new ReadOnlySpan<byte>(PointerShapeBuffer);
      using var ms = new MemoryStream();
      using var image = Image.LoadPixelData<Bgra32>(span, PointerShape.Width.Value, PointerShape.Height.Value);
      {
        image.SaveAsPng(ms);
      }

      return ms.ToArray();
    }

    public static DesktopFrame FromFile(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        throw new ArgumentNullException(nameof(path));
      }

      var result = new DesktopFrame();

      using var image = Image.Load(path);
      {
        using var imageClone = image.CloneAs<Bgra32>();
        {
          var bpp = imageClone.PixelType.BitsPerPixel / 8;
          int bytes = imageClone.Height * imageClone.Width * bpp;

          var frameBuffer = new byte[bytes];
          var bufferSpan = new Span<byte>(frameBuffer);
          if (imageClone.TryGetSinglePixelSpan(out Span<Bgra32> imageSpan))
          {
            for (int i = 0; i < imageSpan.Length; i++)
            {
              bufferSpan[i * bpp] = imageSpan[i].B;
              bufferSpan[i * bpp + 1] = imageSpan[i].G;
              bufferSpan[i * bpp + 2] = imageSpan[i].R;
              bufferSpan[i * bpp + 3] = imageSpan[i].A;
            }

            result.DesktopFrameBuffer = frameBuffer;
            result.DesktopWidth = imageClone.Width;
            result.DesktopHeight = imageClone.Height;
            result.IsDesktopImageBufferEmpty = bufferSpan.Trim((byte)0x00).IsEmpty;
          }
        }
      }

      return result;
    }
  }
}