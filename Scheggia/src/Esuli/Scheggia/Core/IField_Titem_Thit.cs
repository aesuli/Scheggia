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
    using System.Collections.Generic;

    /// <summary>
    /// A specialized Field
    /// </summary>
    /// <typeparam name="Titem">Type of the items in the lexicon</typeparam>
    /// <typeparam name="Tcomparer">Type of the comparer for the items in the lexicon</typeparam>
    /// <typeparam name="Thit">Type of information associated to each hit in the posting lists.</typeparam>
    public interface IField<Titem, Tcomparer, Thit> 
        : IField
        where Tcomparer : IComparer<Titem>
    {
        /// <summary>
        /// The specialized lexicon associated to the field. See <see cref="ILexicon{Titem,Tcomparer}"/>.
        /// </summary>
        ILexicon<Titem, Tcomparer> SpecializedLexicon
        {
            get;
        }

        /// <summary>
        /// The specialized posting list provider associated to the field. See <see cref="IPostingListProvider{Thit}"/>.
        /// </summary>
        IPostingListProvider<Thit> SpecializedPostingListProvider
        {
            get;
        }
    }
}