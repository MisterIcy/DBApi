// <copyright file="CreateDatabase.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of DBApi
// DBApi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// DBApi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with DBApi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-18</date>

using System;
using System.Text;

namespace DBApi.QueryBuilder
{
    public class CreateDatabase : Expression
    {
        private string DatabaseName { get; set; }
        private string? Collation { get; set; }
        public CreateDatabase(string databaseName, string? collation = null) : base(200)
        {
            DatabaseName = string.IsNullOrEmpty(databaseName)
                ? throw new ArgumentNullException(nameof(databaseName))
                : databaseName;
            Collation = collation;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var stringBuilder = new StringBuilder("CREATE DATABASE ");
            stringBuilder.Append(DatabaseName);
            if (!string.IsNullOrEmpty(Collation))
            {
                stringBuilder.Append(" COLLATE ").Append(Collation);
            }

            return stringBuilder.ToString();
        }
    }
}