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

using Esuli.Base.IO;
using Esuli.Scheggia.Core;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Esuli.Scheggia.IO
{
    public class StreamPostingListProviderSerialization<Thit, TpostingListSerialization> : IPostingListProviderSerialization<Thit>
        where TpostingListSerialization : IPostingEnumeratorSerialization<Thit>, new()
    {
        public void Write(IPostingListProvider<Thit> postingListProvider, string indexName, string indexLocation, string fieldName)
        {
            int indexWriteBuffer = 10 * 1024 * 1024;

            using (Stream primaryStream = new FileStream(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.primaryPostingsFileExtension, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, indexWriteBuffer))
            using (Stream secondaryStream = new FileStream(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.secondaryPostingsFileExtension, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, indexWriteBuffer))
            {
                var postingListProviderSerialization = new TpostingListSerialization();

                long mappingPointerPosition = primaryStream.Position;
                primaryStream.Position += sizeof(long);
                long[] idToPositionMapping = new long[postingListProvider.Count + 1];
                for (int i = 0; i < postingListProvider.Count; ++i)
                {
                    idToPositionMapping[i] = primaryStream.Position;
                    using (var postingEnumerator = postingListProvider.GetSpecializedPostingEnumerator(i, i))
                    {
                        postingListProviderSerialization.Write(postingEnumerator, primaryStream, secondaryStream);
                    }
                }
                idToPositionMapping[postingListProvider.Count] = primaryStream.Position;

                long mappingBegin = primaryStream.Position;
                VariableByteCoding.Write(idToPositionMapping.Length, primaryStream);
                long previousPosition = 0;
                for (int i = 0; i < idToPositionMapping.Length; ++i)
                {
                    long currentValue = idToPositionMapping[i];
                    VariableByteCoding.Write(currentValue - previousPosition, primaryStream);
                    previousPosition = currentValue;
                }
                long endOfWriting = primaryStream.Position;
                primaryStream.Position = mappingPointerPosition;
                byte[] bytes = BitConverter.GetBytes(mappingBegin);
                primaryStream.Write(bytes, 0, bytes.Length);
                primaryStream.Position = endOfWriting;
            }
        }

        public IPostingListProvider<Thit> Read(string indexName, string indexLocation, string fieldName)
        {
            long[] idToPositionMapping = null;
            var primaryStreamFileInfo = new FileInfo(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.primaryPostingsFileExtension);
            using (var tempPrimaryStream = new FileStream(primaryStreamFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] bytes = new byte[sizeof(long)];
                tempPrimaryStream.Read(bytes, 0, bytes.Length);
                long mappingPointerPosition = BitConverter.ToInt64(bytes, 0);
                tempPrimaryStream.Position = mappingPointerPosition;
                int mapSize = (int)VariableByteCoding.Read(tempPrimaryStream);
                idToPositionMapping = new long[mapSize];
                long position = 0;
                for (int i = 0; i < mapSize; ++i)
                {
                    position += VariableByteCoding.Read(tempPrimaryStream);
                    idToPositionMapping[i] = position;
                }
            }

            var primaryMemoryMap = MemoryMappedFile.CreateFromFile(primaryStreamFileInfo.FullName, FileMode.Open);

            var secondaryStreamFileInfo = new FileInfo(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.secondaryPostingsFileExtension);
            MemoryMappedFile secondaryMemoryMap = null;
            if (secondaryStreamFileInfo.Length != 0)
                secondaryMemoryMap = MemoryMappedFile.CreateFromFile(secondaryStreamFileInfo.FullName, FileMode.Open);

            return new StreamPostingListProvider<Thit>(idToPositionMapping, primaryMemoryMap, secondaryMemoryMap, new TpostingListSerialization());
        }
    }
}
