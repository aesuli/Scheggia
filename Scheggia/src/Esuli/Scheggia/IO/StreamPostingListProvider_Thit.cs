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

namespace Esuli.Scheggia.IO
{
    using System;
    using System.IO;
    using Esuli.Scheggia.Core;
    using System.IO.MemoryMappedFiles;

    public class StreamPostingListProvider<Thit> : IPostingListProvider<Thit>
    {
        private long[]idToPositionMapping;
        private MemoryMappedFile primaryMemoryMap;
        private MemoryMappedFile secondaryMemoryMap;
        private IPostingEnumeratorSerialization<Thit> postingEnumeratorSerialization;

        public StreamPostingListProvider(long[] idToPositionMapping, MemoryMappedFile primaryMemoryMap, MemoryMappedFile secondaryMemoryMap, IPostingEnumeratorSerialization<Thit> postingEnumeratorSerialization)
        {
            this.idToPositionMapping = idToPositionMapping;
            this.primaryMemoryMap = primaryMemoryMap;
            this.secondaryMemoryMap = secondaryMemoryMap;
            this.postingEnumeratorSerialization = postingEnumeratorSerialization;
        }
           
        public IPostingEnumerator<Thit> GetSpecializedPostingEnumerator(int enumeratorId, int postingListId)
        {
            long length = 0;
            if(postingListId<idToPositionMapping.Length-1)
            {
                length = idToPositionMapping[postingListId + 1]- idToPositionMapping[postingListId];
            }
            var primaryStream = primaryMemoryMap.CreateViewStream(idToPositionMapping[postingListId], length);
            return postingEnumeratorSerialization.SpecializedRead(enumeratorId, primaryStream, secondaryMemoryMap);
        }

        public IPostingEnumerator GetPostingEnumerator(int enumeratorId, int postingListId)
        {
            return GetSpecializedPostingEnumerator(enumeratorId, postingListId);
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
                primaryMemoryMap.Dispose();
                if (secondaryMemoryMap != null)
                {
                    secondaryMemoryMap.Dispose();
                }
            }
        }

        public int Count
        {
            get
            {
                return idToPositionMapping.Length;
            }
        }
    }
}
