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
    /// Serialization of types.
    /// </summary>
    public class TypeSerialization : IObjectSerialization<Type>
    {
        public static void Skip(Stream s)
        {
            StringSerialization.Skip(s);
        }

        public static Type Read(Stream s)
        {
            String typeName = StringSerialization.Read(s);
            return Type.GetType(typeName);
        }

        public static Type Read(byte[] buffer, ref long position)
        {
            String typeName = StringSerialization.Read(buffer, ref position);
            return Type.GetType(typeName);
        }

        public static void Write(Type t, Stream s)
        {
            StringSerialization.Write(t.AssemblyQualifiedName, s);
        }

        public static T CreateInstance<T>(Stream s) where T : class
        {
            Type t = Read(s);
            return Activator.CreateInstance(t) as T;
        }

        void IObjectSerialization<Type>.Write(Type obj, Stream stream)
        {
            TypeSerialization.Write(obj, stream);
        }

        Type IObjectSerialization<Type>.Read(Stream stream)
        {
            return TypeSerialization.Read(stream);
        }

        Type IObjectSerialization<Type>.Read(byte [] buffer, ref long position)
        {
            return TypeSerialization.Read(buffer, ref position);
        }
    }
}
