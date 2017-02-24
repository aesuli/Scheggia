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
    using System;
    using System.Collections.Generic;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;

    public class FieldMetaData<Titem, Tcomparer, Thit> 
        : IFieldMetaData
      where Tcomparer : IComparer<Titem>, new ()
    {
        private string name;
        private IPostingListProviderSerialization<Thit> postingListProviderSerialization;
        private ILexiconSerialization<Titem, Tcomparer> lexiconSerialization;

        public FieldMetaData(string name, ILexiconSerialization<Titem, Tcomparer> lexiconSerialization, IPostingListProviderSerialization<Thit> postingListProviderSerialization)
        {
            this.name = name;
            this.postingListProviderSerialization = postingListProviderSerialization;
            this.lexiconSerialization = lexiconSerialization;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Type ItemType
        {
            get
            {
                return typeof(Titem);
            }
        }

        public Type ComparerType
        {
            get
            {
                return typeof(Tcomparer);
            }
        }

        public Type HitType
        {
            get
            {
                return typeof(Thit);
            }
        }

        public object PostingListProviderSerialization
        {
            get
            {
                return postingListProviderSerialization;
            }
        }

        public object LexiconSerialization
        {
            get
            {
                return lexiconSerialization;
            }
        }
    }
}
