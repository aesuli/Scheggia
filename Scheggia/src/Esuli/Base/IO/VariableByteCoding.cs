// Copyright (C) 2016 Andrea Esuli
// http://www.esuli.it
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace Esuli.Base.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Variable byte coding
    /// </summary>
    public class VariableByteCoding
	{
        public static long Read(byte [] buffer,ref long position)
        {
            long value = 0;
            long byteCode = 0;
            int shift = 0;
            do
            {
                byteCode = buffer[position++];
                value += (byteCode & 127) << shift;
                shift += 7;
            }
            while (byteCode > 127);

            return value;
        }

        /// <summary>
        /// This function should be used only in cases in which efficiency is not crucial, because the
        /// stream.ReadByte() is not efficient.
        /// Use VariableByteCoding.Read(byte[] buffer, ref long position) instead in cases in which
        /// effinciency is a must (i.e.: pre-read a bunch of bytes from the stream in a buffer).
        /// </summary>
        /// <param name="stream">Input stream from which to read a number</param>
        /// <returns>Number read</returns>
        public static long Read(Stream stream)
        {
            long value = 0;
            long byteCode = 0;
            int shift = 0;
            do
            {
                byteCode = stream.ReadByte();
                value += (byteCode & 127) << shift;
                shift += 7;
            }
            while (byteCode > 127);

            return value;
        }

        public static void Write(long value, Stream stream)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("Cannot write a negative value");
            do
			{
                byte byteCode = (byte) (value & 127);
                value = value >> 7;
                if (value != 0)
                {
                    byteCode |= 128;
                    stream.WriteByte(byteCode);
                }
                else
                {
                    stream.WriteByte(byteCode);
                }
			}
            while (value != 0);
        }

        public static int ByteSize(long value)
        {
            int size = 0;
            do
            {
                ++size;
                value = value >> 7;
            }
            while(value != 0);
            return size;
        }

        public static void Write(long value, int byteSize, Stream stream)
        {
            int realByteSize = ByteSize(value);
            if (realByteSize > byteSize)
            {
                throw new Exception("The given value requires more bytes than specified to be encoded");
            }
            else if (realByteSize == byteSize)
            {
                Write(value, stream);
            }
            else
            {
                do
                {
                    byte byteCode = (byte)(value & 127);
                    value = value >> 7;
                    byteCode |= 128;
                    stream.WriteByte(byteCode);
                    --byteSize;
                }
                while (value != 0);

                while (byteSize > 1)
                {
                    stream.WriteByte(128);
                    --byteSize;
                }
                stream.WriteByte(0);
            }
        }
    }
}
