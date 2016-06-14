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
    public class SerialBuffer
    {

        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler RXbufferUpdated;
        public event CallBackEventHandler TXbufferUpdated;
        public event CallBackEventHandler bufferUpdated;

        private byte[] _RxBuffer;
        public byte[] RxBuffer
        {
            // 1. Get an IBuffer object from the passed in byte array
            // 2. Copy value to the end of the _RxBuffer object.
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
                    Debug.WriteLine(ex.Message);
                }
                
            }
            byte[] empty = { 0x00 };
            return empty;
        }

        public byte[] readAllBytesFromRXBuffer()
        {
            // 1. Get all the bytes from the RxBuffer
            // 2. Return bytes.
            byte[] returnByteArray = ReadFromRxBuffer(RxBuffer.Length);
            return returnByteArray;
        }

        public int bytesInRxBuffer()
        {
            return RxBuffer.Length;
        }

        private byte[] _txBuffer;
        public byte[] txBuffer
        {
            // 1. Get an IBuffer object from the passed in byte array
            // 2. Copy value to the end of the _txBuffer object.
            // 3. Call TXBufferUpdated.
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _txBuffer);
                TXbufferUpdated(this, null);
            }
            private get
            {
                if (_txBuffer != null) { return _txBuffer; }
                else { return new byte[0]; }
            }

        }

        private byte[] internalTxBuffer
        {
            // 1. Get an IBuffer object from the passed in byte array
            // 2. Copy value to the end of the _txBuffer object.
            // 3. DO NOT call TXBufferUpdated.  If txBuffer is called internally,
            //    a rescursive error pops.  TxBuffer calls TXBufferedUpdated while updating itself.
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _txBuffer);
            }
            get
            {
                if (_txBuffer != null) { return _txBuffer; }
                else { return new byte[0]; }
            }
        }

        public int bytesInTxBuffer()
        {
            if(txBuffer != null) { return txBuffer.Length; }
            else { return 0; }
        }

        public byte[] ReadFromTxBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if (internalTxBuffer.Length > 0)
            {
                byte[] returnBytes = internalTxBuffer.Take(numberOfBytes).ToArray();
                internalTxBuffer = internalTxBuffer.Skip(numberOfBytes).Take(internalTxBuffer.Length - numberOfBytes).ToArray();
                return returnBytes;
            }
            else
            {
                byte[] empty = { 0x00 };
                return empty;
            }
        }
        
        private byte[] _PrivateBuffer;
        public byte[] PrivateBuffer
        {
            // 1. Get an IBuffer object from the passed in byte array
            // 2. Copy value to the end of the _PrivateBuffer object.
            set
            {
                IBuffer tmpBuffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(tmpBuffer, out _PrivateBuffer);
            }
            private get { return _PrivateBuffer; }

        }

        public byte[] ReadFromBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.

            while (PrivateBuffer.Length <= numberOfBytes)
            {
                try
                {
                    if (PrivateBuffer.Length > 0)
                    {
                        if (PrivateBuffer.Length > numberOfBytes)
                        {
                            byte[] returnBytes = PrivateBuffer.Take(numberOfBytes).ToArray();
                            PrivateBuffer = PrivateBuffer.Skip(numberOfBytes).Take(PrivateBuffer.Length - numberOfBytes).ToArray();
                            return returnBytes;
                        }
                        else
                        {
                            byte[] returnBytes = PrivateBuffer.Take(numberOfBytes).ToArray();
                            return returnBytes;
                        }

                    }
                    else
                    {
                        return new byte[0];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
            byte[] empty = { 0x00 };
            return empty;
        }

        public byte[] readAllBytesFromBuffer()
        {
            // 1. Get all the bytes from the Buffer
            // 2. Return bytes.
            byte[] returnByteArray = ReadFromBuffer(PrivateBuffer.Length);
            return returnByteArray;
        }

        public int bytesInBuffer()
        {
            if(PrivateBuffer != null)
            {
                return PrivateBuffer.Length;
            } else
            {
                return 0;
            }
            
        }
    }

    
}
