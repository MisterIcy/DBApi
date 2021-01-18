// <copyright file="Update.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of DBApi
// DBApi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// DBApi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with DBApi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-18</date>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBApi.Reflection;

namespace DBApi.QueryBuilder
{
    public class Update : Expression
    {
        private readonly List<string> Fields;
        private readonly string TableName;
        private readonly string Alias;

        public Update(string tableName, List<string> fields, string @alias = "t") : base(90)
        {
            TableName = tableName;
            Alias = @alias;
            Fields = fields;
        }

        public Update(Type entityType) : base(90)
        {
            var metadata = new ClassMetadata(entityType);
            TableName = metadata.TableName;
            Alias = "t";
            Fields = new List<string>();
            foreach (var column
                in metadata.Columns.Where
                (
                    column =>
                        !column.Value.IsCustomColumn && column.Value.RelationshipType != RelationshipType.OneToMany
                )
            )
            {
                Fields.Add(column.Value.ColumnName);
            }
        }

        public Update(ClassMetadata metadata) : base(90)
        {
            TableName = metadata.TableName;
            Alias = "t";
            Fields = new List<string>();
            foreach (var column in metadata.Columns.Select(c => c.Value).ToList())
            {
                if (column.IsIdentifier ||
                    column.IsCustomColumn ||
                    (column.IsRelationship && column.RelationshipType == RelationshipType.OneToMany)
                    )
                {
                    continue;
                }
                Fields.Add(column.ColumnName);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var strb = new StringBuilder("UPDATE " + TableName + " SET ");
            foreach (var field in Fields)
            {
                strb.Append(field + " = @" + field + Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(" ");
            return strb.ToString();
        }
    }
}