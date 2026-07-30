/*
--------------------------------------------------------------------------------

MIT License

Copyright 2020 Mark Heath

Permission  is hereby granted, free of charge, to any person obtaining a copy of
this  software  and  associated documentation files (the "Software"), to deal in
the  Software  without  restriction,  including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the  Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions: 

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE  SOFTWARE  IS  PROVIDED  "AS  IS",  WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR  A  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT  HOLDERS  BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN  AN  ACTION  OF  CONTRACT,  TORT  OR  OTHERWISE,  ARISING  FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.  

--------------------------------------------------------------------------------

About

This is a copy of CircularBuffer.cs from NAudio project by Mark Heath.


Original file, copyright notice and license can be found on NAudio GitHub page:
https://github.com/naudio/NAudio/blob/master/NAudio.Core/Utils/CircularBuffer.cs

--------------------------------------------------------------------------------
*/


using DotBase.Core;
using System.Diagnostics;

namespace DotBase.Buffers
{
    /// <summary>
    /// A very basic circular buffer implementation
    /// </summary>
    public class CircularBufferUnlocked : DisposableBase, IByteRingBuffer
    {
        public bool IsOpen { get { return isOpen; } }
        public long TotalRead { get { return totalRead; } }
        public long TotalWritten { get { return totalWritten; } }

        private byte[] buffer;
        private int writePosition;
        private int readPosition;
        private int byteCount;
        private bool isOpen = true;
        private long totalRead;
        private long totalWritten;

        /// <summary>
        /// Create a new circular buffer
        /// </summary>
        /// <param name="size">Max buffer size in bytes</param>
        public CircularBufferUnlocked(int size)
        {
            isOpen = (size > 0);
            buffer = isOpen ? new byte[size] : Array.Empty<byte>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Write data to the buffer
        /// </summary>
        /// <param name="data">Data to write</param>
        /// <param name="offset">SampleOffset into data</param>
        /// <param name="count">Number of bytes to write</param>
        /// <returns>number of bytes written</returns>
        public virtual unsafe int Write(byte[] data, int offset, int count)
        {
            fixed (byte* dataPtr = data) { return Write(dataPtr, offset, count); }
        }
        public virtual unsafe int Write(byte* dataPtr, int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (isOpen)
            {
                var bytesWritten = 0;
                fixed (byte* bufferPtr = buffer) {

                if (count > buffer.Length - byteCount)
                {
                    count = buffer.Length - byteCount;
                }
                // write to end
                int writeToEnd = Math.Min(buffer.Length - writePosition, count);
                Buffer.MemoryCopy(dataPtr + offset, bufferPtr + writePosition, buffer.Length - writePosition, writeToEnd);
                writePosition += writeToEnd;
                writePosition %= buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(writePosition == 0);
                    // must have wrapped round. Write to start
                    Buffer.MemoryCopy(dataPtr + offset + bytesWritten, bufferPtr, buffer.Length, count - bytesWritten);
                    writePosition += count - bytesWritten;
                    bytesWritten = count;
                }
                byteCount += bytesWritten;
                totalWritten += bytesWritten;

                } // fixed(byte* bufferPtr = data)
                return bytesWritten;
            }
            return 0;
        }

        /// <summary>
        /// Read from the buffer
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="offset">SampleOffset into read buffer</param>
        /// <param name="count">Bytes to read</param>
        /// <returns>Number of bytes actually read</returns>
        public virtual unsafe int Read(byte[] data, int offset, int count)
        {
            fixed(byte* dataPtr = data) { return Read(dataPtr, offset, count); }
        }
        public virtual unsafe int Read(byte* dataPtr, int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (isOpen)
            {
                if (count > byteCount)
                {
                    count = byteCount;
                }
                int bytesRead = 0;

                fixed (byte* bufferPtr = buffer) {
                
                int readToEnd = Math.Min(buffer.Length - readPosition, count);
                Buffer.MemoryCopy(bufferPtr + readPosition, dataPtr + offset, count, readToEnd);
                bytesRead += readToEnd;
                readPosition += readToEnd;
                readPosition %= buffer.Length;

                if (bytesRead < count)
                {
                    // must have wrapped round. Read from start
                    Debug.Assert(readPosition == 0);
                    Buffer.MemoryCopy(bufferPtr, dataPtr + offset + bytesRead, count - bytesRead, count - bytesRead);
                    readPosition += count - bytesRead;
                    bytesRead = count;
                }

                byteCount -= bytesRead;
                totalRead += bytesRead;
                Debug.Assert(byteCount >= 0);
                }
                return bytesRead;
            }
            return 0;
        }

        /// <summary>
        /// Maximum length of this circular buffer
        /// </summary>
        public int Capacity => buffer.Length;

        /// <summary>
        /// Number of bytes currently stored in the circular buffer
        /// </summary>
        public int Count
        {
            get
            {
                return byteCount;
            }
        }

        public virtual void Close()
        {
            ResetInner();
            isOpen = false;
            buffer = Array.Empty<byte>();
        }

        /// <summary>
        /// Resets the buffer
        /// </summary>
        public virtual void ClearBuffer()
        {
            ResetInner();
        }

        private void ResetInner()
        {
            byteCount = 0;
            readPosition = 0;
            writePosition = 0;
        }

        /// <summary>
        /// Advances the buffer, discarding bytes
        /// </summary>
        /// <param name="count">Bytes to advance</param>
        public virtual void Advance(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (isOpen)
            {
                if (count >= byteCount)
                {
                    ResetInner();
                }
                else
                {
                    byteCount -= count;
                    readPosition += count;
                    readPosition %= Capacity;
                }
            }
        }
    }
}
