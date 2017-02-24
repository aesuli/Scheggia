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
    
    public class OffsetPostingEnumerator : IPostingEnumerator
    {
        private int offset;
        private IPostingEnumerator postingEnumerator;
        private ScoreFunction scoreFunction;

        public static IPostingEnumerator Build(int offset, IPostingEnumerator postingEnumerator)
        {
            if (offset == 0)
            {
                return postingEnumerator;
            }
            else
            {
                return new OffsetPostingEnumerator(offset, postingEnumerator);
            }
        }

        protected OffsetPostingEnumerator(int offset, IPostingEnumerator postingEnumerator)
        {
            this.offset = offset;
            this.postingEnumerator = postingEnumerator;
            scoreFunction = ScoreFunctions.CopyScore(postingEnumerator);
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
                postingEnumerator.Dispose();
            }
        }
        
        public ScoreFunction ScoreFunction
        {
            get
            {
                return scoreFunction;
            }
            set
            {
                scoreFunction = value;
            }
        }

        public bool MoveNext()
        {
            return postingEnumerator.MoveNext();
        }

        public bool MoveNext(int minPostingId)
        {
            return postingEnumerator.MoveNext(minPostingId - offset);
        }

        public int Count
        {
            get
            {
                return postingEnumerator.Count;
            }
        }

        public int Progress
        {
            get
            {
                return postingEnumerator.Progress;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return postingEnumerator.CurrentPostingId + offset;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                return postingEnumerator.CurrentHitCount;
            }
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            return postingEnumerator.GetCurrentHitEnumerator();
        }
    }
}
