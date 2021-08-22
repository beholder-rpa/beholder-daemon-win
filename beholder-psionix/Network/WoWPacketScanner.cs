// NOTE: this file is here for reference purposes and needs to be re-built on PacketDotNet or the SharpPCap library added.
// Also, this no longer works in Classic/Retail WoW as the packets are now encrypted.
// Grabbing the encryption key from memory is outside this project.

namespace beholder_psionix
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SharpPcap;

    public class WoWPacketScanner : IDisposable
    {
        //private static readonly ushort[] s_ports = new ushort[] { 3724, 1119, 6012 };
        private static readonly ushort[] s_ports = { 3724 };

        private const ushort ChatBaseLength = 16;

        private static readonly ushort[] s_messageSequence = { 6, 3, 9 };
        //private static readonly ushort[] s_ports = new ushort[] { 80 };

        private ICaptureDevice _captureDevice;
        private Stack<UInt64> _messageStack = new Stack<UInt64>(9);

        public WoWPacketScanner(ICaptureDevice captureDevice)
        {
            if (captureDevice == null)
            {
                throw new ArgumentNullException(nameof(captureDevice));
            }

            if (captureDevice.Started == true)
            {
                throw new InvalidOperationException($"When initializing the WoW Packet Scanner, {captureDevice.Name} was already started.");
            }

            _captureDevice = captureDevice;
            _captureDevice.OnPacketArrival += CaptureDevice_OnPacketArrival;
        }

        public void Start()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(WoWPacketScanner));
            }

            _captureDevice.Open(DeviceMode.Promiscuous, 250);
            _captureDevice.Filter = "tcp";
            _captureDevice.StartCapture();
        }

        public void Stop()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(WoWPacketScanner));
            }

            _captureDevice.OnPacketArrival -= CaptureDevice_OnPacketArrival;
            _captureDevice.StopCapture();
            _captureDevice.Close();
        }

        /// <summary>
        /// Prints the time and length of each received packet
        /// </summary>
        private static void CaptureDevice_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var tcpPacket = packet.Extract<PacketDotNet.TcpPacket>();
            if (tcpPacket == null)
            {
                return;
            }

            var ipPacket = (PacketDotNet.IPPacket)tcpPacket.ParentPacket;
            var srcIp = ipPacket.SourceAddress;
            var dstIp = ipPacket.DestinationAddress;
            int srcPort = tcpPacket.SourcePort;
            int dstPort = tcpPacket.DestinationPort;

            if (!s_ports.Any(p => p == dstPort))
            {
                return;
            }

            if (!tcpPacket.HasPayloadData || tcpPacket.PayloadData.Length < 4)
            {
                return;
            }

            // First 4 bytes are the length
            var payloadLengthBytes = tcpPacket.PayloadData.Take(4).ToArray();
            uint payloadLength = BitConverter.ToUInt32(payloadLengthBytes);

            // The remaining bytes are the RC4 encrypted data bytes.
            // It appears a nonce is in use as the value is different each time.

            // We still could use the comms channel to transmit information using payload length...
            // but it becomes hard to decern between real packets and generated ones. without some protocol
            if (payloadLength != 25)
            {
                return;
            }

            Task.Run(() =>
            {
                var time = e.Packet.Timeval.Date;
                var len = tcpPacket.PayloadData.Length;

                Console.WriteLine("{0}:{1}:{2},{3} Len={4} Seq={5} {6}:{7} -> {8}:{9}",
                    time.Hour, time.Minute, time.Second, time.Millisecond, len, tcpPacket.SequenceNumber,
                    srcIp, srcPort, dstIp, dstPort);

                //var bytes = tcpPacket.PayloadPacket.Bytes;
                //Console.WriteLine(BitConverter.ToString(bytes));
                var str = BitConverter.ToString(tcpPacket.PayloadData);
                //var str = Convert.ToBase64String(tcpPacket.PayloadData);
                //var str = tcpPacket.ToString(PacketDotNet.StringOutputType.Colored);
                Console.WriteLine(str);

                // if (!str.ToLower().Contains("beholder"))
                // return;

                // Console.WriteLine("{0}:{1}:{2},{3} Len={4}",
                //     time.Hour, time.Minute, time.Second, time.Millisecond, len);
            });
        }

        #region IDisposable Support
        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_captureDevice.Started)
                    {
                        Stop();
                    }

                    _captureDevice = null;
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
