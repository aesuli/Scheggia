// Copyright (C) 2016 Andrea Esuli (andrea@esuli.it)
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

namespace Esuli.Base.IO.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Esuli.Base.Enumerators;

    public class ReadOnlyStorage<Tobject>
        : IReadOnlyList<Tobject>, IDisposable
    {
        private List<long> pointers;
        private BinaryFormatter formatter;
        private Stream stream;

        public ReadOnlyStorage(string name, string location)
        {
            formatter = new BinaryFormatter();
            stream = new FileStream(location + Path.DirectorySeparatorChar + name + Constants.StorageFileExtension, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > 0)
            {
                long offset = (long)formatter.Deserialize(stream);
                stream.Position = offset;
                pointers = formatter.Deserialize(stream) as List<long>;
            }
            else
            {
                pointers = new List<long>();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
        }

        public Tobject this[int index]
        {
            get
            {
                long newPosition = pointers[index];
                if (stream.Position != newPosition)
                {
                    stream.Position = newPosition;
                }
                return (Tobject)formatter.Deserialize(stream);
            }
        }

        public int Count
        {
            get
            {
                return pointers.Count;
            }
        }

        public IEnumerator<Tobject> GetEnumerator()
        {
            for (int index = 0; index < Count; ++index)
            {
                yield return this[index];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Tobject>)this).GetEnumerator();
        }
    }
}
