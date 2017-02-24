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

namespace Esuli.Scheggia.IO
{
    using System.Collections.Generic;
    using System.IO;
    using Esuli.Scheggia.Core;

    public class FieldSerialization<Titem, Tcomparer, Thit>
        : IFieldSerialization
        where Tcomparer : IComparer<Titem>, new()
    {
        private ILexiconSerialization<Titem, Tcomparer> lexiconSerialization;
        private IPostingListProviderSerialization<Thit> postingListProviderSerialization;

        public FieldSerialization(ILexiconSerialization<Titem, Tcomparer> lexiconHandler, IPostingListProviderSerialization<Thit> postingListProviderSerialization)
        {
            this.lexiconSerialization = lexiconHandler;
            this.postingListProviderSerialization = postingListProviderSerialization;
        }

        public void Write(IField genericField, string indexName, string indexLocation)
        {
            IField<Titem, Tcomparer, Thit> field = genericField as IField<Titem, Tcomparer, Thit>;
            ILexicon<Titem, Tcomparer> lexicon = field.SpecializedLexicon;
            lexiconSerialization.Write(lexicon, indexName, indexLocation, field.Name);
            IPostingListProvider<Thit> postingListProvider = field.SpecializedPostingListProvider;
            postingListProviderSerialization.Write(postingListProvider, indexName, indexLocation, field.Name);
        }

        public IField Read(string indexName, string indexLocation, string fieldName)
        {
            ILexicon<Titem, Tcomparer> lexicon = lexiconSerialization.Read(indexName, indexLocation, fieldName) as ILexicon<Titem, Tcomparer>;
            IPostingListProvider<Thit> postingListProvider = postingListProviderSerialization.Read(indexName, indexLocation, fieldName);
            return new Field<Titem, Tcomparer, Thit>(fieldName, lexicon, postingListProvider);
        }
    }
}
