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

namespace Esuli.Scheggia.Merge
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;

    public class UpdatedField<Titem, Tcomparer, Thit>
        : IField<Titem, Tcomparer, Thit>, IPostingListProvider<Thit>
        where Tcomparer : IComparer<Titem>, new()
        where Thit : IComparable<Thit>
    {
        private string name;
        private IPostingListProvider<Thit>[] postingListProviders;
        private ILexicon<Titem, Tcomparer> lexicon;
        private KeyValuePair<int, int>[][] postingListProviderMapping;
        private Dictionary<int, int>[] mapping;

        public UpdatedField(string name, List<KeyValuePair<int, IField>> fieldList, Dictionary<int, int>[] mapping)
        {
            this.mapping = mapping;
            this.name = name;
            postingListProviders = new IPostingListProvider<Thit>[fieldList.Count];
            int i = 0;
            ILexicon<Titem, Tcomparer>[] sourceLexicons = new ILexicon<Titem, Tcomparer>[fieldList.Count];
            foreach (KeyValuePair<int, IField> field in fieldList)
            {
                sourceLexicons[i] = field.Value.Lexicon as ILexicon<Titem, Tcomparer>;
                postingListProviders[i] = (field.Value as IField<Titem, Tcomparer, Thit>).SpecializedPostingListProvider;
                ++i;
            }
            MergedLexiconEnumerator<Titem, Tcomparer> lexiconMerger = new MergedLexiconEnumerator<Titem, Tcomparer>(sourceLexicons);
            var mergedLexicon = new List<Titem>();
            List<KeyValuePair<int, int>[]> postingListProviderMappingList = new List<KeyValuePair<int, int>[]>();
            while (lexiconMerger.MoveNext())
            {
                var lexiconItemsEnumerator = lexiconMerger.GetCurrentLexiconItemsInfo();
                Titem item = lexiconMerger.Current;
                List<KeyValuePair<int, int>> sourceLexiconItems = new List<KeyValuePair<int, int>>();
                foreach (var lexiconItemInfo in lexiconItemsEnumerator)
                {
                    sourceLexiconItems.Add(new KeyValuePair<int, int>(lexiconItemInfo.Key, lexiconItemInfo.Value.Key));
                }

                mergedLexicon.Add(item);
                postingListProviderMappingList.Add(sourceLexiconItems.ToArray());
            }

            postingListProviderMapping = postingListProviderMappingList.ToArray();
            lexicon = new ArrayLexicon<Titem, Tcomparer>(mergedLexicon.ToArray());
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public IPostingEnumerator GetPostingEnumerator(int enumeratorId, int postingId)
        {
            return GetSpecializedPostingEnumerator(enumeratorId, postingId);
        }

        public IPostingEnumerator<Thit> GetSpecializedPostingEnumerator(int enumeratorId, int postingId)
        {
            KeyValuePair<int, int>[] postingIdPairs = postingListProviderMapping[postingId];
            IPostingEnumerator<Thit>[] postingEnumerators = new IPostingEnumerator<Thit>[postingIdPairs.Length];
            int i = 0;
            foreach (KeyValuePair<int, int> postingIdPair in postingIdPairs)
            {
                postingEnumerators[i] = RemappedPostingEnumerator<Thit>.Build(mapping[postingIdPair.Key],
                    postingListProviders[postingIdPair.Key].GetSpecializedPostingEnumerator(enumeratorId, postingIdPair.Value));
                ++i;
            }

            return OrPostingEnumerator<Thit>.Build(postingEnumerators);
        }


        public ILexicon<Titem, Tcomparer> SpecializedLexicon
        {
            get
            {
                return lexicon;
            }
        }

        public IPostingListProvider<Thit> SpecializedPostingListProvider
        {
            get
            {
                return this;
            }
        }

        public ILexicon Lexicon
        {
            get
            {
                return lexicon;
            }
        }

        public IPostingListProvider PostingListProvider
        {
            get
            {
                return this;
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
                foreach (var postingListProvider in postingListProviders)
                {
                    postingListProvider.Dispose();
                }
            }
        }

        public int Count
        {
            get
            {
                return lexicon.Count;
            }
        }
    }
}
