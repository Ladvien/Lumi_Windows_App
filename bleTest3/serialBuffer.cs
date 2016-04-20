using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace lumi
{
    public class serialBuffer
    {

        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler RXbufferUpdated;
        public event CallBackEventHandler TXbufferUpdated;

        private byte[] _RxBuffer;
        public byte[] RxBuffer
        {
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _RxBuffer);
                // Only send update if buffer was added to.
                RXbufferUpdated(this, null);          
            }
            private get { return _RxBuffer; }

        }

        public byte[] ReadFromRxBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.

            while (RxBuffer.Length <= numberOfBytes)
            {
                try
                {
                    if (RxBuffer.Length > 0)
                    {
                        if(RxBuffer.Length > numberOfBytes)
                        {
                            byte[] returnBytes = RxBuffer.Take(numberOfBytes).ToArray();
                            RxBuffer = RxBuffer.Skip(numberOfBytes).Take(RxBuffer.Length - numberOfBytes).ToArray();
                            return returnBytes;
                        } else
                        {
                            byte[] returnBytes = RxBuffer.Take(numberOfBytes).ToArray();
                            return returnBytes;
                        }
                        
                    } else
                    {
                        return new byte[0];
                    }
                } catch ( Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
                
            }
            byte[] empty = { 0x00 };
            return empty;
        }

        public byte[] readAllBytesFromRXBuffer()
        {
            byte[] returnByteArray = ReadFromRxBuffer(RxBuffer.Length);
            return returnByteArray;
        }

        private byte[] _txBuffer;
        public byte[] txBuffer
        {
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _txBuffer);
                TXbufferUpdated(this, null);
            }
            private get { return _txBuffer; }

        }

        public int bytesInTxBuffer()
        {
            return txBuffer.Length;
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
