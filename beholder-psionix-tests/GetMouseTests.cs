namespace beholder_psionix_tests
{
  using beholder_psionix;
  using Xunit;

  public class GetMouseTests
  {
    [Fact]
    public void CanGetMouseInfo()
    {
      var result = NativeMethods.GetMouseInfo();

    }
  }
}
