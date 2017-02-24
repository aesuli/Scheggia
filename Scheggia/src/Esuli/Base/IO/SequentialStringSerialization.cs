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
    /// Sequential serialization of strings. Uses prefix factorization to save space.
    /// This sequential serialization could save space only if used with alphabetically sorted sequences of strings.
    /// </summary>
    public class SequentialStringSerialization
        : ISequentialObjectSerialization<string>
    {
        public SequentialStringSerialization()
        {
        }

        public void WriteFirst(string obj, Stream stream)
        {
            StringSerialization.Write(obj, stream);
        }

        public void Write(string obj, string previousObj, Stream stream)
        {
            int commonPartLenght = 0;
            int minLenght = Math.Min(obj.Length, previousObj.Length);
            while (commonPartLenght < minLenght)
            {
                if (obj[commonPartLenght] == previousObj[commonPartLenght])
                {
                    ++commonPartLenght;
                }
                else
                {
                    break;
                }
            }
            VariableByteCoding.Write(commonPartLenght, stream);
            StringSerialization.Write(obj.Substring(commonPartLenght), stream);
        }

        public string ReadFirst(Stream stream)
        {
            return StringSerialization.Read(stream);
        }

        public string Read(string previousObj, Stream stream)
        {
            int commonPartLenght = (int)VariableByteCoding.Read(stream);
            string suffix = StringSerialization.Read(stream);
            if (commonPartLenght == 0)
            {
                return suffix;
            }
            string prefix = previousObj.Substring(0, commonPartLenght);
            return prefix + suffix;
        }

        public string ReadFirst(byte[] buffer, ref long position)
        {
            return StringSerialization.Read(buffer, ref position);
        }

        public string Read(string previousObj, byte[] buffer, ref long position)
        {
            int commonPartLenght = (int)VariableByteCoding.Read(buffer, ref position);
            string suffix = StringSerialization.Read(buffer, ref position);
            if (commonPartLenght == 0)
            {
                return suffix;
            }
            string prefix = previousObj.Substring(0, commonPartLenght);
            return prefix + suffix;
        }
    }
}
