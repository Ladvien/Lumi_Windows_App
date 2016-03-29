using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace lumi
{
    class serialBuffer
    {

        private byte[] _txBuffer;
        byte[] txBuffer
        {
            set
            {
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(buffer, out _txBuffer);
            }
            get { return _txBuffer; }

        }

        public byte[] bytesFromTxBuffer(int numberOfBytes)
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

        private byte[] _rxBuffer;
        byte[] rxBuffer
        {
            set
            {
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(buffer, out _rxBuffer);
            }
            get { return _rxBuffer; }

        }

        public byte[] BytesFromRXBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if (rxBuffer.Length > 0)
            {
                byte[] returnBytes = rxBuffer.Take(numberOfBytes).ToArray();
                rxBuffer = rxBuffer.Skip(numberOfBytes).Take(rxBuffer.Length - numberOfBytes).ToArray();
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
