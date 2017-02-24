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
    using System.IO;

    /// <summary>
    /// Serialization of strings.
    /// </summary>
    public class StringSerialization : IObjectSerialization<string>
    {
        void IObjectSerialization<string>.Write(string obj, Stream stream)
        {
            StringSerialization.Write(obj, stream);
        }

        string IObjectSerialization<string>.Read(Stream stream)
        {
            return StringSerialization.Read(stream);
        }

        string IObjectSerialization<string>.Read(byte[] buffer, ref long position)
        {
            return StringSerialization.Read(buffer,ref position);
        }

        public static void Write(string str, Stream stream)
        {
            byte[] chars = System.Text.Encoding.UTF8.GetBytes(str);
            VariableByteCoding.Write(chars.Length, stream);
            stream.Write(chars, 0, chars.Length);
        }

        public static string Read(Stream stream)
        {
            int size = (int)VariableByteCoding.Read(stream);
            byte[] tstr = new byte[size];
            stream.Read(tstr, 0, size);
            return System.Text.Encoding.UTF8.GetString(tstr);
        }

        public static string Read(byte[] buffer,ref long position)
        {
            int size = (int)VariableByteCoding.Read(buffer, ref position);
            int basePosition = (int)position;
            position += size;
            return System.Text.Encoding.UTF8.GetString(buffer, basePosition, size);
        }

        public static void Skip(Stream s)
        {
            int skip = (int)VariableByteCoding.Read(s);
            s.Position += skip;
        }
    }
}
