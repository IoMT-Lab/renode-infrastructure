using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;

namespace Antmicro.Renode.Peripherals.UART
{
    public class Generic_UART : UARTBase
    {
        public Generic_UART(IMachine machine, Bits stopBits = Bits.None, Parity parity = Parity.None, uint baudRate = 115200) : base(machine)
        {
            StopBits = stopBits;
            ParityBit = parity;
            BaudRate = baudRate;

            TxIRQ = new GPIO();
            RxIRQ = new GPIO();
        }

        public void TransmitBuffer(byte[] buffer)
        {
            foreach(var b in buffer)
            {
                TransmitCharacter(b);
            }
            TxIRQ.Set();
            TxIRQ.Unset();
        }

        public void TransmitChar(byte character)
        {
            TransmitCharacter(character);
            TxIRQ.Set();
            TxIRQ.Unset();
        }

        public bool ReceiveBuffer(int count, out byte[] buffer)
        {
            buffer = new byte[count];
            for(int i = 0; i < count; i++)
            {
                if(!TryGetCharacter(out buffer[i]))
                {
                    var receivedBuffer = new byte[i];
                    System.Array.Copy(buffer, receivedBuffer, i);
                    buffer = receivedBuffer;
                    return false;
                }
            }
            return true;
        }

        public bool ReceiveChar(out byte character)
        {
            return TryGetCharacter(out character);
        }

        public bool CanReceive()
        {
            return Count > 0;
        }

        public override Bits StopBits { get; }

        public override Parity ParityBit { get; }

        public override uint BaudRate { get; }

        public GPIO TxIRQ { get; private set; }

        public GPIO RxIRQ { get; private set; }

        protected override void CharWritten()
        {
            this.Log(LogLevel.Noisy, "Char written to UART");
            RxIRQ.Set();
            RxIRQ.Unset();
        }

        protected override void QueueEmptied()
        {
            this.Log(LogLevel.Noisy, "UART queue emptied");
            RxIRQ.Set();
            RxIRQ.Unset();
        }
    }
}