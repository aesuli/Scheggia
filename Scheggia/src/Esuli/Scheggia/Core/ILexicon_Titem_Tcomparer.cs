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

    /// <summary>
    /// A specialized lexicon.
    /// 
    /// Each object in the lexicon is associated to a unique integer value indicating its position in the list of all the lexicon items sorted by the comparer of the lexicon.
    /// The identifiers start from zero and go up to <see cref="ILexicon.Count"/>-1.
    /// </summary>
    /// <typeparam name="Titem">The type of lexicon items.</typeparam>
    /// <typeparam name="Tcomparer">The comparer of lexicon items.</typeparam>
    public interface ILexicon<Titem,Tcomparer> 
        : ILexicon
        where Tcomparer : IComparer<Titem>
    {
        /// <summary>
        /// Given a query object, it searches for a matching item in the lexicon.
        /// If found, it returns its integer identifier (greater or equal to zero), which can be used to obtain a <see cref="IHitEnumerator"/> from the <see cref="IPostingListProvider"/> of the <see cref="IField"/>.
        /// If not found, it returns a negative number which is the bitwise complement of the position of the first lexicon item that is larger than the query object (with respect to the comparer).
        /// </summary>
        /// <param name="query">The query object.</param>
        /// <returns>The unique identifier of the lexicon item matching the query, or a negative number which is the bitwise complement of the position of the first lexicon item that is larger than the query object (with respect to the comparer).</returns>
        int Search(Titem query);

        /// <summary>
        /// Gets the Titem relative to a given identifier.
        /// </summary>
        /// <param name="position">The identifier of a lexicon item.</param>
        /// <returns>A Titem, if the identifier is valid, or null otherwise.</returns>
        Titem this[int position] {get;}
    }
}