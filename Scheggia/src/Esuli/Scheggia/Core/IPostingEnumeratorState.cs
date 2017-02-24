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
    /// The <see cref="IPostingEnumeratorState"/> interface represents the
    /// state of a non-specialized postings enumerator.
    /// </summary>
    /// <remarks><para>The <see cref="IPostingEnumeratorState"/> interface is
    /// stricly coupled with the <see cref="IPostingEnumerator"/> 
    /// interface.</para></remarks>
    /// <seealso cref="IPostingEnumerator"/>
    /// <seealso cref="IPostingEnumerator{Thit}"/>
    public interface IPostingEnumeratorState : IDisposable
    {
        /// <summary>
        /// Gets an <b>estimate</b> of the total number of postings.
        /// </summary>
        /// <remarks>Although <see cref="Count"/> is an estimate of the total 
        /// number of postings, its value <b>must be always <i>greater</i> or
        /// <i>equal</i></b> than the <see cref="Progress"/> value.
        /// </remarks>
        /// <seealso cref="Progress"/>
        int Count
        {
            get;
        }

        /// <summary>
        /// Gets an <b>estimate</b> of the number of postings enumerated so far.
        /// </summary>
        /// <remarks><para>Although <see cref="Progress"/> is an estimate of 
        /// the total number of postings, its value <b>must be always 
        /// <i>lesser</i> or <i>equal</i></b> than the <see cref="Count"/> 
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
        /// Gets the Id of the current posting.
        /// </summary>
        /// <remarks>
        /// <see cref="CurrentPostingId"/> is not required to be defined when the
        /// enumerator has not yet advanced to the first element or it has
        /// passed over the last.
        /// </remarks>
        int CurrentPostingId
        {
            get;
        }

        /// <summary>
        /// Gets an <b>estimate</b> of the total number of hits of the
        /// current posting.
        /// </summary>
        /// <remarks>
        /// <see cref="CurrentHitCount"/> is not required to be defined when the
        /// enumerator has not yet advanced to the first element or it has
        /// passed over the last.
        /// </remarks>
        int CurrentHitCount
        {
            get;
        }

        /// <summary>
        /// Gets an enumerator of the hits of the current posting.
        /// </summary>
        /// <remarks>
        /// <see cref="GetCurrentHitEnumerator"/> is not required to 
        /// be defined when the enumerator has not yet advanced to the first 
        /// element or it has passed over the last.
        /// </remarks>
        /// <seealso cref="IHitEnumerator"/>
        IHitEnumerator GetCurrentHitEnumerator();

        /// <summary>
        /// Delegate method for scoring definition.
        /// </summary>
        /// <remarks>
        /// <para>The <ScoreFunction> property has a double use:
        /// <list type="bullet">
        /// <item>Getting and setting the <see cref="ScoreFuction"/> delegate 
        /// enables the definition of the scoring model.</item>
        /// <item>Invoking the delegate method returns the score of the 
        /// current posting.</item>
        /// </list>
        /// <example>
        /// <code>
        /// IPostingListEnumeratorState enumerator;
        /// // scoring model definition
        /// enumerator.ScoreFunction = delegate() { 
        ///         return Math.Log(enumerator.CurrentHitCount+1);
        ///     };
        /// // getting a score
        /// double score = enumerator.ScoreFunction();
        /// </code>
        /// </example>
        /// </para>
        /// </remarks>
        ScoreFunction ScoreFunction
        {
            get;
            set;
        }
    }
}
