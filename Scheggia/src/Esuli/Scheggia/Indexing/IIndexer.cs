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
namespace Esuli.Scheggia.Indexing
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    /// <summary>
    /// The interface for an <see cref="IIndex"/> builder.
    /// </summary>
    public interface IIndexer
    {
        /// <summary>
        /// Gets the total number of hits indexed so far.
        /// </summary>
        long HitCount
        {
            get;
        }

        /// <summary>
        /// Adds the definition of a field, specifying its name and the types
        /// for lexicon items and hit infos.
        /// </summary>
        /// <typeparam name="Titem">the type of items in the default lexicon
        /// </typeparam>
        /// <typeparam name="Thit">the type of hits</typeparam>
        /// <param name="fieldName">the name of the field</param>
        /// <remarks>
        /// <para>If the field already exists, with the same types, nothing
        /// happens.</para>
        /// <para>If the field already exists, but the type does not match,
        /// an exception is thrown.</para>
        /// </remarks>
        /// <exception cref="Exception">If the field already exists, but the 
        /// type does not match, an exception is thrown.</exception>
        void AddField<Titem, Tcomparer, Thit>(string fieldName)
            where Tcomparer : IComparer<Titem>, new ()
            where Thit : IComparable<Thit>;

        void Index<Titem, Tcomparer, Thit>(IEnumerator<ReaderHit<Titem, Thit>> hitEnumerator, string fieldName)
            where Tcomparer : IComparer<Titem>, new ()
            where Thit : IComparable<Thit>;

    }
}
