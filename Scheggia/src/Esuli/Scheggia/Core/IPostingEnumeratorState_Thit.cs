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
    /// The specialization of the <see cref="IPostingEnumeratorState"/>
    /// interface.
    /// </summary>
    /// <typeparam name="Thit">The type of the hits.</typeparam>
    /// <seealso cref="IPostingEnumeratorState"/>
    public interface IPostingEnumeratorState<Thit> 
        : IPostingEnumeratorState
    {
        /// <summary>
        /// Gets the specialized <see cref="IHitEnumerator{Thit}"/>
        /// enumerator of the hits of the current posting.
        /// </summary>
        /// <remarks>
        /// <see cref="GetSpecializedCurrentHitEnumerator"/> is not required to 
        /// be defined when the enumerator has not yet advanced to the first 
        /// element or it has passed over the last.
        /// </remarks>
        /// <seealso cref="IHitEnumerator{Thit}"/>
        IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator();
    }
}
