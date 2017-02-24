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
    /// The specialization of the <see cref="IHitEnumeratorState"/>
    /// interface.
    /// </summary>
    /// <typeparam name="Thit">The type of the posting.</typeparam>
    /// <seealso cref="IHitEnumeratorState"/>
    public interface IHitEnumeratorState<Thit> 
        : IHitEnumeratorState
    {
        /// <summary>
        /// Gets the current hit object
        /// </summary>
        /// <remarks> <see cref="Current"/> is not required to be defined when
        /// the enumerator has not yet advanced to the first element or it has
        /// passed over the last.</remarks>
        Thit Current
        {
            get;
        }
    }
}
