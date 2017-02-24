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
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    /// <summary>
    /// Comparer of posting lists by ascending order of number of postings.
    /// </summary>
    public class AscendingPostingEnumeratorComparer : IComparer<IPostingEnumerator>
    {
        public int Compare(IPostingEnumerator x, IPostingEnumerator y)
        {
            return x.Count.CompareTo(y.Count);
        }
    }
}
