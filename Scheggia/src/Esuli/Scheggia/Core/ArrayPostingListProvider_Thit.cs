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

namespace Esuli.Scheggia.Core
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Enumerators;
    
    /// <summary>
    /// Simple in-memory array-based implementation of <see cref=" IPostingListProvider{Thit}" />.
    /// </summary>
    /// <typeparam name="Thit">Type of the hit.</typeparam>
    public class ArrayPostingListProvider<Thit> : IPostingListProvider<Thit>
    {
        private KeyValuePair<int [], Thit[][]> [] postingLists;

        public ArrayPostingListProvider(KeyValuePair<int[], Thit[][]>[] postingLists)
        {
            this.postingLists = postingLists;
        }

        public IPostingEnumerator GetPostingEnumerator(int enumeratorId, int postingListId)
        {
            return GetSpecializedPostingEnumerator(enumeratorId, postingListId);
        }

        public IPostingEnumerator<Thit> GetSpecializedPostingEnumerator(int enumeratorId, int postingListId)
        {
            return new ArrayPostingEnumerator<Thit>(enumeratorId, postingLists[postingListId].Key, postingLists[postingListId].Value);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public int Count
        {
            get
            {
                return postingLists.Length;
            }
        }
    }
}
