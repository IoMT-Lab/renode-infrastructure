//
// Copyright (c) 2010-2025 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;

using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Network;

using System.Collections.Generic;


namespace Antmicro.Renode.Peripherals.Network
{
    public class Generic_Ethernet : NetworkWithPHY, IMACInterface
    {

        public Generic_Ethernet(IMachine machine) : base(machine)
        {
            IRQ = new GPIO();
            Reset();
        }

        public bool Receive(out byte[] frameData, bool checkCrc = true)
        {
            frameData = null;
            if(packetQueue.Count == 0)
            {
                return false;
            }

            var frame = packetQueue.Dequeue();
            if(checkCrc && !EthernetFrame.CheckCRC(frame.Bytes))
            {
                this.Log(LogLevel.Warning, "Received frame with invalid CRC, dropping it");
                return false;
            }

            frameData = frame.Bytes;
            return true;
        }

        public bool Send(byte[] frameData, bool addCrc = false, int minFrameSize = 0)
        {
            this.Log(LogLevel.Noisy, "Sending frame with length {0} bytes", frameData.Length);
            if(frameData.Length < minFrameSize)
            {
                this.Log(LogLevel.Noisy, "Frame length is less than minimum frame size ({0} bytes), padding with zeros", minFrameSize);
                var newFrameData = new byte[minFrameSize];
                Array.Copy(frameData, newFrameData, frameData.Length);
                frameData = newFrameData;
            }

            if(EthernetFrame.TryCreateEthernetFrame(frameData, addCrc, out var frame))
            {
                FrameReady?.Invoke(frame);
                return true;
            }
            return false;
        }

        public void ReceiveFrame(EthernetFrame frame)
        {
            this.Log(LogLevel.Noisy, "Received frame with length {0} bytes", frame.Bytes.Length);
            packetQueue.Enqueue(frame);
            IRQ.Set();
            IRQ.Unset();
        }

        public override void Reset()
        {
            MAC = new MACAddress();
            packetQueue.Clear();
        }

        public bool FrameAvailable => packetQueue.Count > 0;

        public MACAddress MAC { get; set; }

        public GPIO IRQ { get; private set; }

        public event Action<EthernetFrame> FrameReady;

        private readonly Queue<EthernetFrame> packetQueue = new Queue<EthernetFrame>();
    }
}