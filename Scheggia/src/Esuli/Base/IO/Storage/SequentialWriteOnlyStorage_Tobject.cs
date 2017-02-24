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

    public class SequentialWriteOnlyStorage<Tobject>
        : ISequentiallyWriteableStorage<Tobject>
    {
        private List<long> pointers;
        private BinaryFormatter formatter;
        private Stream stream;
        private bool modified;

        public SequentialWriteOnlyStorage(string name, string location) :
            this(name, location, false)
        {
        }

        public SequentialWriteOnlyStorage(string name, string location, bool overwrite)
        {
            formatter = new BinaryFormatter();
            stream = new FileStream(location + Path.DirectorySeparatorChar + name + Constants.StorageFileExtension, overwrite ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (stream.Length > 0)
            {
                long offset = (long)formatter.Deserialize(stream);
                stream.Position = offset;
                pointers = formatter.Deserialize(stream) as List<long>;
                stream.Position = offset;
                modified = false;
            }
            else
            {
                pointers = new List<long>();
                long offset = stream.Position;
                formatter.Serialize(stream, offset);
                modified = true;
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
                Close();
                stream.Dispose();
            }
        }

        public void Close()
        {
            if (modified)
            {
                stream.Seek(0, SeekOrigin.End);
                long offset = stream.Position;
                formatter.Serialize(stream, pointers);
                stream.Position = 0;
                formatter.Serialize(stream, offset);
                modified = false;
            }
            pointers = null;
            stream.Close();
        }

        public int Write(Tobject obj)
        {
            int id = pointers.Count;
            pointers.Add(stream.Position);
            formatter.Serialize(stream, obj);
            return id;
        }

        public int Count
        {
            get
            {
                return pointers.Count;
            }
        }
    }
}
