﻿// <copyright file="TruncateTable.cs">
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

namespace DBApi.QueryBuilder
{
    public class TruncateTable : Expression
    {
        [PublicAPI]
        public string TableName { get; protected set; }
        public TruncateTable(string tableName) : base(-190)
        {
            TableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException(nameof(tableName))
                : tableName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "TRUNCATE TABLE " + TableName;
        }
    }
}