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
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Scoring;
    
    public class RemappedPostingEnumerator : IPostingEnumerator
    {
        private Dictionary<int, int> mapping;
        private IPostingEnumerator postingEnumerator;
        private int progress;
        private ScoreFunction scoreFunction;
        private int currentMappedPostingId;
        private int count;

        public static IPostingEnumerator Build(Dictionary<int, int> mapping, IPostingEnumerator postingEnumerator)
        {
            return new RemappedPostingEnumerator(mapping, postingEnumerator);
        }

        protected RemappedPostingEnumerator(Dictionary<int, int> mapping, IPostingEnumerator postingEnumerator)
        {
            this.mapping = mapping;
            this.postingEnumerator = postingEnumerator;
            count = postingEnumerator.Count;
            scoreFunction = ScoreFunctions.CopyScore(postingEnumerator);
            progress = -1;
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
            while (postingEnumerator.MoveNext())
            {
                int postingId = postingEnumerator.CurrentPostingId;
                if (mapping.TryGetValue(postingId, out currentMappedPostingId))
                {
                    ++progress;
                    return true;
                }
            }
            count = progress;
            return false;
        }

        public bool MoveNext(int minPostingId)
        {
            if (CurrentPostingId >= minPostingId)
                return true;
            while(MoveNext())
            {
                if (CurrentPostingId >= minPostingId)
                    return true;
            }
            return false;
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
                return progress;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return currentMappedPostingId;
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
