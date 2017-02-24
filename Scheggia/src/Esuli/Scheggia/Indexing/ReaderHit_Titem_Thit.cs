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
namespace Esuli.Scheggia.Indexing
{
    public struct ReaderHit<Titem, Thit>
    {
        private int id;
        private Titem item;
        private Thit hit;

        public ReaderHit(int id,Titem item, Thit hit)
        {
            this.id = id;
            this.item = item;
            this.hit = hit;
        }

        public int Id 
        { 
            get 
            { 
                return id; 
            }
        }

        public Titem Item
        {
            get
            {
                return item;
            }
        }

        public Thit Hit
        {
            get
            {
                return hit;
            }
        }
    }
}
