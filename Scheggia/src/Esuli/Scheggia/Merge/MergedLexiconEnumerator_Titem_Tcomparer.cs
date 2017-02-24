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
    using System.Collections;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    public class MergedLexiconEnumerator<Titem, Tcomparer>
        : IEnumerator<Titem>
        where Tcomparer : IComparer<Titem>, new ()
    {
        private ILexicon<Titem, Tcomparer>[] lexicons;
        private int[] currentPositions;
        protected List<int> currentLexicons;
        private Titem currentLexiconItem;
        private Tcomparer comparer;

        public MergedLexiconEnumerator(ILexicon<Titem, Tcomparer>[] lexicons)
        {
            this.lexicons = lexicons;
            currentPositions = new int[lexicons.Length];
            for (int i = 0; i < lexicons.Length; ++i)
            {
                currentPositions[i] = 0;
            }
            currentLexicons = new List<int>();
            currentLexiconItem = default(Titem);
            comparer = new Tcomparer();
        }

        public void Reset()
        {
            for (int i = 0; i < lexicons.Length; ++i)
            {
                currentPositions[i] = 0;
            }
            currentLexicons.Clear();
            currentLexiconItem = default(Titem);
        }

        public bool MoveNext()
        {
            bool found = false;
            int minI = 0;
            foreach (int i in currentLexicons)
            {
                ++currentPositions[i];
            }
            currentLexicons.Clear();
            for(int i = 0; i < lexicons.Length; ++i)
            {
                if (currentPositions[i]<lexicons[i].Count)
                {
                    minI = i;
                    found = true;
                    break;
                }
            }
            if(found)
            {
                currentLexicons.Add(minI);
                currentLexiconItem = lexicons[minI][currentPositions[minI]];
                for(int i = minI + 1; i < lexicons.Length; ++i)
                {
                    if(currentPositions[i]<lexicons[i].Count)
                    {
                        int comparison =  comparer.Compare(currentLexiconItem,lexicons[i][currentPositions[i]]);
                        if(comparison > 0)
                        {
                            minI = i;
                            currentLexiconItem = lexicons[minI][currentPositions[minI]];
                            currentLexicons.Clear();
                            currentLexicons.Add(minI);
                        }
                        else if (comparison == 0)
                        {
                            currentLexicons.Add(i);
                        }
                    }
                }
            }
            return found;
        }

        public Titem Current
        {
            get
            {
                return currentLexiconItem;
            }
        }

        public KeyValuePair<int,KeyValuePair<int, Titem>>[] GetCurrentLexiconItemsInfo()
        {
            if (currentLexicons.Count == 0)
            {
                return null;
            }
            var currentLexiconItems = new KeyValuePair<int, KeyValuePair<int, Titem>>[currentLexicons.Count];
            int j = 0;
            foreach(int i in currentLexicons)
            {
                currentLexiconItems[j] = new KeyValuePair<int, KeyValuePair<int, Titem>>(i,new KeyValuePair<int,Titem>( currentPositions[i], lexicons[i][currentPositions[i]]));
                ++j;
            }
            return currentLexiconItems;
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get
            {
                return ((IEnumerator<Titem>)this).Current;
            }
        }
    }
}
