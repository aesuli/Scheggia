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

namespace Esuli.Scheggia.Scoring
{
    using System;
    using Esuli.Scheggia.Core;

    public class ScoreFunctions
    {
        public static ScoreFunction ScoreZero()
        {
            return delegate()
            {
                return 0.0;
            };
        }

        public static ScoreFunction ScoreOne()
        {
            return delegate()
            {
                return 1.0;
            };
        }

        public static ScoreFunction MultiplyScore(double multiplier, ScoreFunction baseScoreFunction)
        {
            return delegate()
            {
                return baseScoreFunction() * multiplier;
            };
        }

        public static ScoreFunction AddScore(IPostingEnumeratorState[] enums)
        {
            return delegate()
            {
                double score = 0.0;
                foreach (IPostingEnumeratorState en in enums)
                {
                    score += en.ScoreFunction();
                }
                return score;
            };
        }

        public static ScoreFunction CopyScore(IPostingEnumeratorState es)
        {
            return delegate()
            {
                return es.ScoreFunction();
            };
        }

        public static ScoreFunction TF(IPostingEnumeratorState es)
        {
            return delegate()
            {
                return es.CurrentHitCount;
            };
        }

        public delegate int MaxHitCount(int id);

        public static ScoreFunction nTF(IPostingEnumeratorState es, double a, MaxHitCount maxHitCount)
        {
            return delegate()
            {
                return a + (1.0-a)*(TF(es)()/ maxHitCount(es.CurrentPostingId));
            };
        }

        public static ScoreFunction nTFIDF(IPostingEnumeratorState es, double a, MaxHitCount maxHitCount, long indexSize)
        {
            return delegate()
            {
                double idf = Math.Log((2.0 + indexSize) / (1.0 + Math.Min(es.Count,indexSize)));
                return nTF(es,a,maxHitCount)() * idf;
            };
        }
    }
}
