using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace lumi
{
    public class serialBuffer
    {

        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler bufferUpdated;

        private byte[] _RxBuffer;
        public byte[] RxBuffer
        {
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _RxBuffer);
                bufferUpdated(this, null);
            }
            private get { return _RxBuffer; }

        }

        public byte[] ReadFromRxBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if (RxBuffer.Length > 0)
            {
                byte[] returnBytes = RxBuffer.Take(numberOfBytes).ToArray();
                RxBuffer = RxBuffer.Skip(numberOfBytes).Take(RxBuffer.Length - numberOfBytes).ToArray();
                return returnBytes;
            }
            else
            {
                byte[] empty = { 0x00 };
                return empty;
            }
        }

        private byte[] _txBuffer;
        public byte[] txBuffer
        {
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _txBuffer);
                bufferUpdated(this, null);
            }
            private get { return _txBuffer; }

        }

        public byte[] ReadFromTxBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if (txBuffer.Length > 0)
            {
                byte[] returnBytes = txBuffer.Take(numberOfBytes).ToArray();
                txBuffer = txBuffer.Skip(numberOfBytes).Take(txBuffer.Length - numberOfBytes).ToArray();
                return returnBytes;
            }
            else
            {
                byte[] empty = { 0x00 };
                return empty;
            }

        }
    }
}
