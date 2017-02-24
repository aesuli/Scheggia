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

namespace Esuli.Scheggia.Text.Search
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;
    using Esuli.Scheggia.Search;

    public class PrefixQuery<Tcomparer, Thit> : IQuery<Thit>
        where Tcomparer : IComparer<string>
        where Thit : IComparable<Thit>
    {
        public static readonly string prefixFlag = "*";

        private string fieldName;
        private string prefix;
        private static char lastSymbol;

        static PrefixQuery()
        {
            for (char i = char.MaxValue; i >= char.MinValue; --i)
            {
                if (char.IsSymbol(i))
                {
                    lastSymbol = i;
                    break;
                }
            }
        }

        public PrefixQuery(string fieldName, string prefix)
        {
            this.fieldName = fieldName;
            this.prefix = prefix;
        }

        public string Describe()
        {
            StringBuilder description = new StringBuilder(fieldName);
            description.Append(TextQueryParser<Tcomparer>.fieldWordSeparator);
            description.Append(prefix);
            description.Append(prefixFlag);
            return description.ToString();
        }

        public IPostingEnumerator Apply(IIndex index)
        {
            return ApplySpecialized(index);
        }

        public IPostingEnumerator<Thit> ApplySpecialized(IIndex index)
        {
            IField<string, Tcomparer, Thit> field = index.GetSpecializedField<string, Tcomparer, Thit>(fieldName);
            int firstPosition = field.SpecializedLexicon.Search(prefix);
            int lastPosition = field.SpecializedLexicon.Search(prefix + lastSymbol);
            if (firstPosition < 0)
            {
                firstPosition = ~firstPosition;
            }
            if (lastPosition < 0)
            {
                lastPosition = ~lastPosition;
            }
            if (firstPosition >= field.SpecializedLexicon.Count)
            {
                firstPosition = field.SpecializedLexicon.Count;
            }
            if (lastPosition >= field.SpecializedLexicon.Count)
            {
                lastPosition = field.SpecializedLexicon.Count;
            }

            if (lastPosition - firstPosition > 0)
            {
                IPostingEnumerator<Thit>[] enumerators = new IPostingEnumerator<Thit>[lastPosition - firstPosition];
                for (int i = firstPosition; i < lastPosition; ++i)
                {
                    enumerators[i - firstPosition] = field.SpecializedPostingListProvider.GetSpecializedPostingEnumerator(i, i);
                }

                return OrPostingEnumerator<Thit>.Build(enumerators);
            }
            else
            {
                return new EmptyPostingEnumerator<Thit>();
            }
        }
    }
}
