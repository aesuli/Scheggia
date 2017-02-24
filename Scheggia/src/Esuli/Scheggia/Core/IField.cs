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
    /// A field contains a <see cref="ILexicon"/> and a <see cref="IPostingListProvider"/>.
    /// IField is the non-specialized version of <see cref="IField{Titem,Thit}"/>.
    /// </summary>
    public interface IField : IDisposable 
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        /// <remarks>
        /// <para>A field name must be unique for the <see cref="IIndex"/> the
        /// field belongs to.</para>
        /// </remarks>
        string Name
        {
            get;
        }

        /// <summary>
        /// The non-specialized <see cref="ILexicon"/> of the field.
        /// </summary>
        ILexicon Lexicon
        {
            get;
        }

        /// <summary>
        /// The non-specialized <see cref="IPostingListProvider"/> of the field.
        /// </summary>
        IPostingListProvider PostingListProvider
        {
            get;
        }
    }
}