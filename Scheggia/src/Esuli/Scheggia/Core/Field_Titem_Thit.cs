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
    /// Implementation of a <see cref="IField{Titem,Tcomparer,Thit}"/>.
    /// </summary>
    /// <typeparam name="Titem">The type of the lexicon items.</typeparam>
    /// <typeparam name="Tcomparer">The type of the items comparer.</typeparam>
    /// <typeparam name="Thit">The type of the hit.</typeparam>
    public class Field<Titem, Tcomparer, Thit>
        : IField<Titem, Tcomparer, Thit>
        where Tcomparer : IComparer<Titem>
    {
        private string name;
        private ILexicon<Titem, Tcomparer> lexicon;
        private IPostingListProvider<Thit> postingListProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Field{Titem, Tcomparer, Thit}"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="lexicon">The lexicon.</param>
        /// <param name="postingListProvider">The posting lists provider.</param>
        public Field(string name, ILexicon<Titem, Tcomparer> lexicon, IPostingListProvider<Thit> postingListProvider)
        {
            this.name = name;
            this.lexicon = lexicon;
            this.postingListProvider = postingListProvider;
        }


        /// <summary>
        /// The name of the field.
        /// </summary>
        /// <remarks>
        /// A field name must be unique for the <see cref="IIndex" /> the
        /// field belongs to.
        /// </remarks>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// The non-specialized <see cref="ILexicon" /> of the field.
        /// </summary>
        public ILexicon Lexicon
        {
            get
            {
                return lexicon;
            }
        }

        /// <summary>
        /// The specialized lexicon associated to the field. See <see cref="ILexicon{Titem,Tcomparer}" />.
        /// </summary>
        public ILexicon<Titem, Tcomparer> SpecializedLexicon
        {
            get
            {
                return lexicon;
            }
        }

        /// <summary>
        /// The non-specialized <see cref="IPostingListProvider" /> of the field.
        /// </summary>
        public IPostingListProvider PostingListProvider
        {
            get
            {
                return postingListProvider;
            }
        }

        /// <summary>
        /// The specialized posting list provider associated to the field. See <see cref="IPostingListProvider{Thit}" />.
        /// </summary>
        public IPostingListProvider<Thit> SpecializedPostingListProvider
        {
            get
            {
                return postingListProvider;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                postingListProvider.Dispose();
            }
        }
    }
}
