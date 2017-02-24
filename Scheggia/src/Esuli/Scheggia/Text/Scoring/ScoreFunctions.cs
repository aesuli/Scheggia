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

namespace Esuli.Scheggia.Text.Scoring
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Text.Core;

    public class TextScoreFunctions
    {
        public static ScoreFunction LabelBasedMaxScore(IPostingEnumerator<LabeledPositionHit> es, IDictionary<byte,double> labelScores)
        {
            return delegate()
            {
                double maxScore = double.MinValue;
                using (var hitEnumerator = es.GetSpecializedCurrentHitEnumerator())
                {
                    while (hitEnumerator.MoveNext())
                    {
                        maxScore = Math.Max(maxScore, labelScores[hitEnumerator.Current.Label]);
                    }
                }
                return maxScore;
            };
        }

        public static ScoreFunction LabelBasedMinScore(IPostingEnumerator<LabeledPositionHit> es, IDictionary<byte, double> labelScores)
        {
            return delegate()
            {
                double minScore = double.MaxValue;
                using (var hitEnumerator = es.GetSpecializedCurrentHitEnumerator())
                {
                    while (hitEnumerator.MoveNext())
                    {
                        minScore = Math.Min(minScore, labelScores[hitEnumerator.Current.Label]);
                    }
                }
                return minScore;
            };
        }

        public static ScoreFunction LabelBasedAverageScore(IPostingEnumerator<LabeledPositionHit> es, IDictionary<byte, double> labelScores)
        {
            return delegate()
            {
                double accumulator = 0.0;
                int count = 0;
                using (var hitEnumerator = es.GetSpecializedCurrentHitEnumerator())
                {
                    while (hitEnumerator.MoveNext())
                    {
                        accumulator += labelScores[hitEnumerator.Current.Label];
                        ++count;
                    }
                }
                return accumulator/count;
            };
        }
    }
}
            
