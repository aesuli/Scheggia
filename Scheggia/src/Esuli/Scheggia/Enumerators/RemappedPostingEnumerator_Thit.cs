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
    
    public class RemappedPostingEnumerator<Thit> : RemappedPostingEnumerator, IPostingEnumerator<Thit>
    {
        private IPostingEnumerator<Thit> postingEnumerator;

        public static IPostingEnumerator<Thit> Build(Dictionary<int,int> mapping, IPostingEnumerator<Thit> postingEnumerator)
        {
            return new RemappedPostingEnumerator<Thit>(mapping, postingEnumerator);
        }

        private RemappedPostingEnumerator(Dictionary<int, int> mapping, IPostingEnumerator<Thit> postingEnumerator)
            : base(mapping, postingEnumerator)
        {
            this.postingEnumerator = postingEnumerator;
        }

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            return postingEnumerator.GetSpecializedCurrentHitEnumerator();
        }
    }
}
