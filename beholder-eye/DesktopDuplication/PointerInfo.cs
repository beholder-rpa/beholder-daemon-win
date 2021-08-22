namespace beholder_eye
{
  using System.Drawing;
  using Vortice.DXGI;

  internal class PointerInfo
  {
    public byte[] ShapeBuffer;
    public OutduplPointerShapeInfo ShapeInfo;
    public Point Position;
    public bool Visible;
    public int WhoUpdatedPositionLast;
    public long LastTimeStamp;
  }
}