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

    public class ProximityPostingEnumerator<Thit> : IPostingEnumerator<Thit>
        where Thit : IPositionalHit<Thit>, IComparable<Thit>
    {
        private IPostingEnumerator<Thit>[] postingEnumerators;
        private IPostingEnumerator<Thit> andPostingEnumerator;
        private int progress;
        private int count;
        private int currentPostingId;
        private int currentHitCount;
        private ScoreFunction scoreFunction;
        private int maxDistance;

        public static IPostingEnumerator<Thit> Build(IPostingEnumerator<Thit>[] postingEnumerators, int maxDistance)
        {
            if (postingEnumerators.Length == 0)
            {
                return new EmptyPostingEnumerator<Thit>();
            }

            if (postingEnumerators.Length == 1)
            {
                return postingEnumerators[0];
            }

            return new ProximityPostingEnumerator<Thit>(postingEnumerators, maxDistance);
        }

        private ProximityPostingEnumerator(IPostingEnumerator<Thit>[] postingEnumerators, int maxDistance)
        {
            int length = postingEnumerators.Length;
            this.maxDistance = maxDistance;
            this.postingEnumerators = new IPostingEnumerator<Thit>[length];
            for (int i = 0; i < length; ++i)
            {
                this.postingEnumerators[i] = postingEnumerators[i];
            }

            andPostingEnumerator = AndPostingEnumerator<Thit>.Build(postingEnumerators, new IPostingEnumerator<Thit>[0]);

            progress = 0;
            currentPostingId = -1;
            currentHitCount = -1;
            count = andPostingEnumerator.Count / Math.Max(1, length);
            scoreFunction = ScoreFunctions.MultiplyScore(Math.Log(length, 2.0), ScoreFunctions.AddScore(postingEnumerators));
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
                andPostingEnumerator.Dispose();
                foreach (var hitListEnumerator in postingEnumerators)
                {
                    hitListEnumerator.Dispose();
                }
            }
        }

        public bool MoveNext()
        {
            return MoveNextHit(true);
        }

        public bool MoveNextHit(bool advance)
        {
            if (advance)
            {
                if (!andPostingEnumerator.MoveNext())
                {
                    count = progress;
                    currentPostingId = int.MaxValue;
                    return false;
                }
            }
            while (true)
            {
                var hitEnumerators = new IHitEnumerator<Thit>[postingEnumerators.Length];

                for (int i = 0; i < hitEnumerators.Length; ++i)
                {
                    hitEnumerators[i] = postingEnumerators[i].GetSpecializedCurrentHitEnumerator();
                }

                using (ProximityHitEnumerator<Thit> hitEnumerator = new ProximityHitEnumerator<Thit>(hitEnumerators, maxDistance))
                {
                    if (hitEnumerator.MoveNext())
                    {
                        ++progress;
                        currentPostingId = postingEnumerators[0].CurrentPostingId;
                        currentHitCount = -1;
                        count = -1;
                        return true;
                    }
                }
                for (int i = 0; i < hitEnumerators.Length; ++i)
                {
                    hitEnumerators[i].Dispose();
                }

                if (!andPostingEnumerator.MoveNext())
                {
                    count = progress;
                    currentPostingId = int.MaxValue;
                    return false;
                }
            }
        }

        public bool MoveNext(int minPostingId)
        {
            if (currentPostingId >= minPostingId && currentPostingId != int.MaxValue)
            {
                return true;
            }

            currentHitCount = -1;

            int andPostingEnumeratorProgress = andPostingEnumerator.Progress;
            if (andPostingEnumerator.MoveNext(minPostingId))
            {
                double progressRatio = progress / (double)Math.Max(1, andPostingEnumeratorProgress);
                int estDeltaProgress = (int)(progressRatio * (andPostingEnumerator.Progress - andPostingEnumeratorProgress));
                progress += Math.Max(0, estDeltaProgress - 1);
                return MoveNextHit(false);
            }

            currentPostingId = int.MaxValue;
            return false;
        }

        public int Count
        {
            get
            {
                if (count < 0)
                {
                    count = progress
                        + (int)((andPostingEnumerator.Count - andPostingEnumerator.Progress)
                        * ((double)progress / Math.Max(1, andPostingEnumerator.Progress)));
                }
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
                return currentPostingId;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                //TODO improve estimate
                if (currentHitCount < 0)
                {
                    currentHitCount = 0;
                    foreach (IPostingEnumerator postingEnumerator in postingEnumerators)
                    {
                        currentHitCount += postingEnumerator.CurrentHitCount;
                    }
                }
                return currentHitCount;
            }
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            return GetSpecializedCurrentHitEnumerator();
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

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            IHitEnumerator<Thit>[] hitEnumerators = new IHitEnumerator<Thit>[postingEnumerators.Length];
            for (int i = 0; i < postingEnumerators.Length; ++i)
            {
                hitEnumerators[i] = postingEnumerators[i].GetSpecializedCurrentHitEnumerator();
            }
            return new ProximityHitEnumerator<Thit>(hitEnumerators, maxDistance);
        }
    }
}
