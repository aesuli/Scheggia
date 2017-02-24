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

namespace Esuli.Scheggia.Enumerators
{
    using System;
    using Esuli.Scheggia.Core;

    public class OrPostingEnumerator<Thit> : OrPostingEnumerator, IPostingEnumerator<Thit> where Thit : IComparable<Thit>
    {
        private IPostingEnumerator<Thit>[] postingEnumerators;

        public static IPostingEnumerator<Thit> Build(IPostingEnumerator<Thit>[] postingEnumerators)
        {
            if (postingEnumerators.Length == 0)
            {
                return new EmptyPostingEnumerator<Thit>();
            }

            if (postingEnumerators.Length == 1)
            {
                return postingEnumerators[0];
            }

            return new OrPostingEnumerator<Thit>(postingEnumerators);
        }

        protected OrPostingEnumerator(IPostingEnumerator<Thit>[] postingEnumerators)
            : base(postingEnumerators)
        {
            this.postingEnumerators = postingEnumerators;
        }
       
        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            if (currentHitEnumerators.Count == 0)
            {
                return null;
            }

            IHitEnumerator<Thit>[] hitEnumerators = new IHitEnumerator<Thit>[currentHitEnumerators.Count];
            int j = 0;
            foreach(int i in currentHitEnumerators)
            {
                hitEnumerators[j] = postingEnumerators[i].GetSpecializedCurrentHitEnumerator();
                ++j;
            }
            return new ArrayHitEnumeratorMerger<Thit>(hitEnumerators);
        }
    }
}
