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
    /// The specialization of the <see cref="IHitEnumerator"/>
    /// interface.
    /// </summary>
    /// <typeparam name="Thit">The type of the hit.</typeparam>
    /// <seealso cref="IHitEnumerator"/>
    /// <seealso cref="IHitEnumeratorState{Thit}"/>
    public interface IHitEnumerator<Thit>
         : IHitEnumerator, IHitEnumeratorState<Thit>
    {
        /// <summary>
        /// Advances the enumerator to the next hit which is equal or
        /// greater than <paramref name="minHit"/>.
        /// </summary>
        /// <remarks>If the enumerator is already in a state that satisfies this
        /// condition, the enumerator does not advance.</remarks>
        /// <param name="minHit">a reference hit</param>
        /// <returns><c>true</c> if such hit exists, <c>false</c> if the 
        /// enumerator has reached the end not finding it.</returns>
        /// <seealso cref="IHitEnumeratorState{Thit}.Current"/>
        bool MoveNext(Thit minHit);
    }
}
