// <copyright file="ColumnAttribute.cs">
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
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Gets a value that defines the table column associated with this field
        /// </summary>
        public string ColumnName { get; }
        /// <summary>
        /// Shortcut for <see cref="ColumnName"/>
        /// </summary>
        public string Name => ColumnName;
        /// <summary>
        /// Gets a value that defines the type of the column
        /// <see cref="DBApi.Attributes.ColumnType"/> 
        /// </summary>
        public ColumnType ColumnType { get; }
        /// <summary>
        /// Gets a value that defines if the column can take NULL values
        /// </summary>
        public bool Nullable { get; }
        /// <summary>
        /// Gets a value that defines if the column can only take unique values
        /// </summary>
        public bool Unique { get; }
        /// <summary>
        /// Column Attribute Constructor
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <param name="columnType">Type of the column</param>
        /// <param name="nullable">True if the column can take NULL values, otherwise false</param>
        /// <param name="unique">True if the column can only take unique values, otherwise false</param>
        public ColumnAttribute(
            string name,
            ColumnType columnType = ColumnType.String,
            bool nullable = true,
            bool unique = false
        )
        {
            ColumnName = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
            ColumnType = columnType;
            Nullable = nullable;
            Unique = unique;
        }
    }
}