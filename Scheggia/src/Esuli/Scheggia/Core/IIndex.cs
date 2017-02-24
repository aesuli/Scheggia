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

    /// <summary>
    /// Interface for a Scheggia index.
    /// An index has a name and it is composed of a set of <see cref="IField"/>.
    /// </summary>
    public interface IIndex : IDisposable
    {
        /// <summary>
        /// Name of the index.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Number of fields it contains.
        /// </summary>
        int FieldCount
        {
            get;
        }

        /// <summary>
        /// The maximum posting id value returned by the enumerators.
        /// Required by the merge functions.
        /// </summary>
        int MaxId
        {
            get;
        }

        /// <summary>
        /// Enumerator of the names of the fields.
        /// </summary>
        /// <returns>Enumerator of the names of the fields.</returns>
        IEnumerator<string> GetFieldNameEnumerator();

        /// <summary>
        /// Gets a <see cref="IField"/> object from its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The IField object, or null if the name is not associated to a field.</returns>
        IField GetField(string fieldName);

        /// <summary>
        /// Gets a specialized <see cref="IField"/> object from its name and type parameters.
        /// </summary>
        /// <typeparam name="Titem">Type of the objects in the <see cref="ILexicon"/>.</typeparam>
        /// <typeparam name="Tcomparer">Type of the comparer of objects in the <see cref="ILexicon"/>.</typeparam>
        /// <typeparam name="Thit">Type of information associated to each hit in the posting lists.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The <see cref="IField"/> object, or null if the name is not associated to a field.</returns>
        IField<Titem, Tcomparer, Thit> GetSpecializedField<Titem, Tcomparer, Thit>(string fieldName)
            where Tcomparer : IComparer<Titem>;
    }
}
