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
    using Esuli.Scheggia.Scoring;

    /// <summary>
    /// AND operator that also supports negated enumerators.
    /// </summary>
    /// <typeparam name="Thit">Type of hits.</typeparam>
    public class AndPostingEnumerator<Thit> : AndPostingEnumerator, IPostingEnumerator<Thit> where Thit : IComparable<Thit>
    {
        private IPostingEnumerator<Thit>[] postingEnumerators;

        public static IPostingEnumerator<Thit> Build(IPostingEnumerator<Thit>[] postingListEnumerators, IPostingEnumerator[] notPostingEnumerators)
        {
            if (postingListEnumerators.Length == 0)
            {
                return new EmptyPostingEnumerator<Thit>();
            }

            if (postingListEnumerators.Length == 1 && notPostingEnumerators.Length == 0)
            {
                return postingListEnumerators[0];
            }

            return new AndPostingEnumerator<Thit>(postingListEnumerators, notPostingEnumerators);
        }

        private AndPostingEnumerator(IPostingEnumerator<Thit> [] postingEnumerators,IPostingEnumerator [] notPostingEnumerators)
            : base(postingEnumerators,notPostingEnumerators)
        {
            this.postingEnumerators = postingEnumerators;
        }

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            IHitEnumerator<Thit>[] hitEnumerators = new IHitEnumerator<Thit>[postingEnumerators.Length];
            for (int i = 0; i < postingEnumerators.Length; ++i)
            {
                hitEnumerators[i] = postingEnumerators[i].GetSpecializedCurrentHitEnumerator();
            }
            return new ArrayHitEnumeratorMerger<Thit>(hitEnumerators);
        }
    }
}
