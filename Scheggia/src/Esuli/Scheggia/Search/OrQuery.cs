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

namespace Esuli.Scheggia.Search
{
    using System.Text;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;

    public class OrQuery : IQuery
    {
        private IQuery [] arguments;

        public OrQuery(IQuery[] arguments)
        {
            this.arguments = arguments;
        }

        public IPostingEnumerator Apply(IIndex index)
        {
            IPostingEnumerator [] enumerators = new IPostingEnumerator[arguments.Length];
            for (int i = 0; i < arguments.Length; ++i)
            {
                enumerators[i] = arguments[i].Apply(index);
            }
           return OrPostingEnumerator.Build(enumerators);
        }

        public string Describe()
        {
            StringBuilder description = new StringBuilder("OR ( ");
            foreach (IQuery argument in arguments)
            {
                description.Append(argument.Describe());
                description.Append(" , ");
            }
            description.Length -= 3;
            description.Append(" ) ");
            return description.ToString();
        }
    }
}
