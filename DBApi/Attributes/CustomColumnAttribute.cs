// <copyright file="CustomColumnAttribute.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of DBApi
// DBApi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// DBApi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with DBApi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-18</date>

using System;
using DBApi.Annotations;

namespace DBApi.Attributes
{
    /// <summary>
    /// Επισημαίνει πως το πεδίο συσχετίζεται με "παραμετρικό πεδίο" σε πίνακα εκτός οντότητας
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public sealed class CustomColumnAttribute : Attribute
    {
        public CustomColumnAttribute(string tableName, string identifierColumn, int columnFieldId, ColumnType columnType = ColumnType.String)
        {
            CustomTableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException(nameof(tableName))
                : tableName;
            
            IdentifierColumn = identifierColumn;
            CustomFieldId = columnFieldId;
            ColumnType = columnType;
        }
        /// <summary>
        /// Ο πίνακας όπου αποθηκεύονται τα παραμετρικά
        /// </summary>
        public string CustomTableName { get; }
        /// <summary>
        /// Η στήλη που "συσχετίζει" το παραμετρικό πεδίο με την οντότητα στην οποία ανήκει
        /// </summary>
        public string IdentifierColumn { get; }
        /// <summary>
        /// Το αναγνωριστικό του παραμετρικού πεδίου
        /// </summary>
        public int CustomFieldId { get; }
        /// <summary>
        /// Τύπος πεδίου
        /// </summary>
        public ColumnType ColumnType { get; }
    }
}