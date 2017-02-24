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
    /// Interface for a class that implements the methods to sequentially read and write a Tobject from a stream.
    /// </summary>
    /// <typeparam name="Tobject"></typeparam>
    public interface ISequentialObjectSerialization<Tobject>
    {
        void WriteFirst(Tobject obj, Stream stream);

        Tobject ReadFirst(Stream stream);

        Tobject ReadFirst(byte[] buffer, ref long position);

        void Write(Tobject obj, Tobject previousObj, Stream stream);

        Tobject Read(Tobject previousObj, Stream stream);

        Tobject Read(Tobject previousObj, byte[] buffer, ref long position);
    }
}
