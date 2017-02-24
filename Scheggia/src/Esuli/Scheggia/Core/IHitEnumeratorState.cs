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
    /// The <see cref="IHitEnumeratorState"/> interface represents the
    /// state of an hit enumerator on any step of the enumeration process.
    /// </summary>
    /// <remarks><para>The <see cref="IHitEnumeratorState"/> is
    /// stricly coupled with the <see cref="IHitEnumerator"/> 
    /// interface.</para>
    /// <para>
    /// Separation between the state of the enumerator and the methods to advance it 
    /// has been adopted in order to permit the enumerator to expose its state 
    /// without allowing to the external code to change it.
    /// </para></remarks>
    /// <seealso cref="IHitEnumerator"/>
    /// <seealso cref="IHitEnumeratorState{Thit}"/>
    public interface IHitEnumeratorState : IDisposable
    {
        /// <summary>
        /// An <b>estimate</b> of the total number of hits.
        /// </summary>
        /// <remarks>Although <see cref="Count"/> is an estimate of the total 
        /// number of hits, its value <b>must be always <i>greater</i> or
        /// <i>equal</i></b> than the <see cref="Progress"/> value.
        /// </remarks>
        /// <seealso cref="Progress"/>
        int Count
        {
            get;
        }

        /// <summary>
        /// An <b>estimate</b> of the number of hits enumerated so far.
        /// </summary>
        /// <remarks><para>Although <see cref="Progress"/> is an estimate of 
        /// the total number of hits, its value <b>must be always 
        /// <i>less</i> or <i>equal</i></b> than the <see cref="Count"/> 
        /// value.</para>
        /// <para>The <see cref="Progress"/> value <b>must be zero (0)</b>
        /// until the enumerator does not advance for the first time.</para>
        /// </remarks>
        /// <seealso cref="Count"/>
        /// <seealso cref="IHitEnumerator.MoveNext"/>
        /// <seealso cref="IHitEnumerator.MoveNext(int)"/>
        int Progress
        {
            get;
        }

        /// <summary>
        /// The id of the enumerator generating the current hit.
        /// </summary>
        /// <remarks>
        /// <para>A hit enumerator could be the result of the composition of
        /// several hit enumerators. This attribute allows to find which 
        /// of the original hit enumerators is the source of the current
        /// hit.</para>
        /// <para> <see cref="CurrentEnumeratorId"/> is not required to be 
        /// defined when the enumerator has not yet advanced to the first 
        /// element or it has passed over the last.</para>
        /// </remarks>
        int CurrentEnumeratorId
        {
            get;
        }

        /// <summary>
        /// The type of the current hit.
        /// </summary>
        /// <remarks>
        /// <para>A hit enumerator could be the result of the composition 
        /// of several hit enumerators, each one with its one type of hit.
        /// This attribute allows to determine the type of the current
        /// hit.</para>
        /// <para> <see cref="HitType"/> is not required to be defined when
        /// the enumerator has not yet advanced to the first element or it has
        /// passed over the last.</para>
        /// </remarks>
        Type HitType
        {
            get;
        }

        /// <summary>
        /// Gets the current hit object
        /// </summary>
        /// <remarks> <see cref="CurrentHit"/> is not required to be defined when
        /// the enumerator has not yet advanced to the first element or it has
        /// passed over the last.</remarks>
        object CurrentHit
        {
            get;
        }
    }
}
