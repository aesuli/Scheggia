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
    
    /// <summary>
    /// A non-specialized provider of <see cref="IPostingEnumerator"/>.
    /// </summary>
    public interface IPostingListProvider : IDisposable
    {
        /// <summary>
        /// Returns the <see cref="IPostingEnumerator"/> associated with the
        /// given <paramref name="postingListId"/>.
        /// </summary>
        /// <param name="enumeratorId">The enumerator id to be assigned to the requested
        /// <see cref="IPostingEnumerator"/>. Useful to identify the source of results
        /// when multiple enumerators are combined toghether.</param>
        /// <param name="itemId">The id of the requested postings list.</param>
        /// <returns>The requested <see cref="IPostingEnumerator"/> if the
        /// <paramref name="postingListId"/> value point to an existing postings list,
        /// <c>null</c> otherwise.</returns>
        IPostingEnumerator GetPostingEnumerator(int enumeratorId, int postingListId);
    }
}