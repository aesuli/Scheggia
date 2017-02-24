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
    /// <summary>
    /// An non-specialized enumerator of postings.
    /// </summary>
    /// <seealso cref="IPostingEnumeratorState"/>
    public interface IPostingEnumerator 
        : IPostingEnumeratorState
    {
        /// <summary>
        /// Advances the enumerator to the next posting.
        /// </summary>
        /// <returns><c>true</c> if the next posting exists, <c>false</c> if the 
        /// enumerator has reached the end.</returns>
        bool MoveNext();

        /// <summary>
        /// Advances the enumerator to the next posting which is equal or
        /// greater than <paramref name="minPostingId"/>.
        /// </summary>
        /// <remarks>If the enumerator is already in a state that satisfies this
        /// condition, the enumerator does not advance.</remarks>
        /// <param name="minHit">a reference posting id</param>
        /// <returns><c>true</c> if such posting exists, <c>false</c> if the 
        /// enumerator has reached the end not finding it.</returns>
        /// <seealso cref="IPostingEnumeratorState.CurrentPostingId"/>
        bool MoveNext(int minPostingId);
    }
}
