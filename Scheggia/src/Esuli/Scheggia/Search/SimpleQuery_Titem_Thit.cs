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

namespace Esuli.Scheggia.Search
{
    using System.Collections.Generic;
    using System.Text;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;

    public class SimpleQuery<Titem, Tcomparer, Thit> : 
        IQuery<Thit> where  Tcomparer : IComparer<Titem>
    {
        public static readonly string fieldItemSeparator = ":";

        private string fieldName;
        private Titem query;

        public SimpleQuery(string fieldName, Titem query)
        {
            this.fieldName = fieldName;
            this.query = query;
        }

        public string Describe()
        {
            StringBuilder description = new StringBuilder(fieldName);
            description.Append(fieldItemSeparator);
            description.Append(query.ToString());
            return description.ToString();
        }

        public IPostingEnumerator Apply(IIndex index)
        {
            return ApplySpecialized(index);
        }

        public IPostingEnumerator<Thit> ApplySpecialized(IIndex index)
        {
            IField<Titem, Tcomparer, Thit> field = index.GetSpecializedField<Titem, Tcomparer, Thit>(fieldName);
            int position = field.SpecializedLexicon.Search(query);
            if (position >= 0)
            {
                return field.SpecializedPostingListProvider.GetSpecializedPostingEnumerator(0, position);
            }
            else
            {
                return new EmptyPostingEnumerator<Thit>();
            }
        }
    }
}
