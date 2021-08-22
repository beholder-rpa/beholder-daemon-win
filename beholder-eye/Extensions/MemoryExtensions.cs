// See https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/MemoryExtensions.Trim.cs

namespace beholder_eye
{
  using System;
  using System.Diagnostics;

  public static class MemoryExtensions
  {
    /// <summary>
    /// Removes all leading and trailing occurrences of a specified element from the span.
    /// </summary>
    /// <param name="span">The source span from which the element is removed.</param>
    /// <param name="trimElement">The specified element to look for and remove.</param>
    public static Span<T> Trim<T>(this Span<T> span, T trimElement) where T : IEquatable<T>
    {
      int start = ClampStart(span, trimElement);
      int length = ClampEnd(span, start, trimElement);
      return span.Slice(start, length);
    }

    // <summary>
    /// Removes all leading and trailing occurrences of a specified element from the span.
    /// </summary>
    /// <param name="span">The source span from which the element is removed.</param>
    /// <param name="trimElement">The specified element to look for and remove.</param>
    public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>
    {
      int start = ClampStart(span, trimElement);
      int length = ClampEnd(span, start, trimElement);
      return span.Slice(start, length);
    }

    /// <summary>
    /// Delimits all leading occurrences of a specified element from the span.
    /// </summary>
    /// <param name="span">The source span from which the element is removed.</param>
    /// <param name="trimElement">The specified element to look for and remove.</param>
    private static int ClampStart<T>(ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>
    {
      int start = 0;

      if (trimElement != null)
      {
        for (; start < span.Length; start++)
        {
          if (!trimElement.Equals(span[start]))
          {
            break;
          }
        }
      }
      else
      {
        for (; start < span.Length; start++)
        {
          if (span[start] != null)
          {
            break;
          }
        }
      }

      return start;
    }

    /// <summary>
    /// Delimits all trailing occurrences of a specified element from the span.
    /// </summary>
    /// <param name="span">The source span from which the element is removed.</param>
    /// <param name="start">The start index from which to being searching.</param>
    /// <param name="trimElement">The specified element to look for and remove.</param>
    private static int ClampEnd<T>(ReadOnlySpan<T> span, int start, T trimElement) where T : IEquatable<T>
    {
      // Initially, start==len==0. If ClampStart trims all, start==len
      Debug.Assert((uint)start <= span.Length);

      int end = span.Length - 1;

      if (trimElement != null)
      {
        for (; end >= start; end--)
        {
          if (!trimElement.Equals(span[end]))
          {
            break;
          }
        }
      }
      else
      {
        for (; end >= start; end--)
        {
          if (span[end] != null)
          {
            break;
          }
        }
      }

      return end - start + 1;
    }
  }
}