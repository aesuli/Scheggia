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
    
    public class RangePostingEnumerator : IPostingEnumerator
    {
        private int currentPostingId;
        private int firstPostingId;
        private int lastPostingId;
        private int count;
        private ScoreFunction scoreFunction;

        public static IPostingEnumerator Build(int firstPostingId, int lastPostingId)
        {
            if (firstPostingId > lastPostingId)
            {
                return new EmptyPostingEnumerator();
            }
            return new RangePostingEnumerator(firstPostingId, lastPostingId);
        }

        protected RangePostingEnumerator(int firstPostingId, int lastPostingId)
        {
            this.currentPostingId = firstPostingId - 1;
            this.firstPostingId = firstPostingId;
            this.lastPostingId = lastPostingId;
            count = lastPostingId - firstPostingId + 1;
            scoreFunction = ScoreFunctions.ScoreZero();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            ++currentPostingId;
            if (currentPostingId > lastPostingId)
            {
                return false;
            }
            return true;
        }

        public bool MoveNext(int minPostingId)
        {
            currentPostingId = Math.Max(currentPostingId,Math.Max(firstPostingId,minPostingId));
            if (currentPostingId > lastPostingId)
            {
                return false;
            }
            return true;
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

        public int Count
        {
            get
            {
                return count;
            }
        }

        public int Progress
        {
            get
            {
                return currentPostingId - firstPostingId + 1;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return currentPostingId;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                return 0;
            }
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            return new EmptyHitEnumerator();
        }
    }
}
