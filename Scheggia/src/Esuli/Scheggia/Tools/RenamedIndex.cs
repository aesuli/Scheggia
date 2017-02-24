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

namespace Esuli.Scheggia.Tools
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    public class RenamedIndex : IIndex
    {
        private string indexName;
        private IIndex index;

        public RenamedIndex(string indexName, IIndex index)
        {
            this.indexName = indexName;
            this.index = index;
        }

        public string Name
        {
            get
            {
                return indexName;
            }
        }

        public int FieldCount
        {
            get
            {
                return index.FieldCount;
            }
        }

        public int MaxId
        {
            get
            {
                return index.MaxId;
            }
        }

        public IEnumerator<string> GetFieldNameEnumerator()
        {
            return index.GetFieldNameEnumerator();
        }

        public IField GetField(string fieldName)
        {
            return index.GetField(fieldName);
        }

        public IField<Titem, Tcomparer, ThitInfo> GetSpecializedField<Titem, Tcomparer, ThitInfo>(string fieldName) 
            where Tcomparer : IComparer<Titem>
        {
            return index.GetSpecializedField<Titem, Tcomparer, ThitInfo>(fieldName);
        }

        public void Dispose()
        {
            index.Dispose();
            GC.SuppressFinalize(index);
        }
    }
}
