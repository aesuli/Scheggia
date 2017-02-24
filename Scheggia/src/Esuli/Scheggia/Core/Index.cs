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

namespace Esuli.Scheggia.Core
{
    using System;
    using System.Collections.Generic;

    public class Index : IIndex
    {
        private string name;
        private Dictionary<string, IField> fields;
        private int maxId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// This constructor usually will be not invoked by user applications, that will instead get an Index from a <see cref="IIndexer"/> or from a <see cref="IIndexReader"/>.
        /// </summary>
        /// <param name="name">The name of the index.</param>   
        /// <param name="maxId">The maximum hit id value returned by the enumerators.</param>
        /// <param name="fields">The fields dictionary.</param>
        public Index(string name, int maxId, Dictionary<string, IField> fields)
        {
            this.name = name;
            this.fields = fields;
            this.maxId = maxId;
        }

        /// <summary>
        /// Name of the index.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Number of fields it contains.
        /// </summary>
        public int FieldCount
        {
            get
            {
                return fields.Count;
            }
        }

        /// <summary>
        /// The maximum posting id value returned by the enumerators.
        /// Required by the merge functions.
        /// </summary>
        public int MaxId
        {
            get
            {
                return maxId;
            }
        }

        /// <summary>
        /// Enumerator of the names of the fields.
        /// </summary>
        /// <returns>
        /// Enumerator of the names of the fields.
        /// </returns>
        public IEnumerator<string> GetFieldNameEnumerator()
        {
            return fields.Keys.GetEnumerator();
        }

        /// <summary>
        /// Gets a <see cref="IField" /> object from its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// The IField object, or null if the name is not associated to a field.
        /// </returns>
        public IField GetField(string fieldName)
        {
            return fields[fieldName];
        }

        /// <summary>
        /// Gets a specialized <see cref="IField" /> object from its name and type parameters.
        /// </summary>
        /// <typeparam name="Titem">Type of the objects in the <see cref="ILexicon" />.</typeparam>
        /// <typeparam name="Tcomparer">Type of the comparer of objects in the <see cref="ILexicon" />.</typeparam>
        /// <typeparam name="Thit">Type of information associated to each hit in the posting lists.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// The <see cref="IField" /> object, or null if the name is not associated to a field.
        /// </returns>
        public IField<Titem, Tcomparer, Thit> GetSpecializedField<Titem, Tcomparer, Thit>(string fieldName)
            where Tcomparer : IComparer<Titem>
        {
            return fields[fieldName] as IField<Titem, Tcomparer, Thit>;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var field in fields)
                {
                    field.Value.Dispose();
                }
            }
        }
    }
}
