// <copyright file="ColumnType.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of DBApi
// DBApi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// DBApi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with DBApi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-18</date>

using System;

namespace DBApi.Attributes
{
    /// <summary>
    /// Enumeration of data types we can use in Columns 
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// Binary Data
        /// </summary>
        Binary,
        /// <summary>
        /// Boolean data
        /// </summary>
        Boolean,
        /// <summary>
        /// Single Byte
        /// </summary>
        Byte,
        /// <summary>
        /// Bytes
        /// </summary>
        Bytes,
        /// <summary>
        /// Characters
        /// </summary>
        Chars,
        /// <summary>
        /// DateTime
        /// </summary>
        DateTime,
        /// <summary>
        /// Decimal point numbers
        /// </summary>
        Decimal,
        /// <summary>
        /// Double
        /// </summary>
        Double,
        /// <summary>
        /// GUID
        /// </summary>
        Guid,
        /// <summary>
        /// 16 bit integers
        /// </summary>
        Int16,
        /// <summary>
        /// 32 bit integers
        /// </summary>
        Int32,
        /// <summary>
        /// 64 bit integers
        /// </summary>
        Int64,
        /// <summary>
        /// Monetary values
        /// </summary>
        Money,
        /// <summary>
        /// Single
        /// </summary>
        Single,
        /// <summary>
        /// String
        /// </summary>
        String,
        /// <summary>
        /// XML Data
        /// </summary>
        Xml,
        
        /** OLD TYPES, USED FOR BC */
        
        /// <summary>
        /// nchar & nvarchar
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        STRING,
        /// <summary>
        /// Integer values
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        INTEGER,
        /// <summary>
        /// Floating point values
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        DOUBLE,
        /// <summary>
        /// true / false values
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        BOOLEAN,
        /// <summary>
        /// DateTime
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        DATETIME,
        /// <summary>
        /// Dates
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        DATE,
        /// <summary>
        /// Times
        /// </summary>
        [Obsolete("This type is obsolete and will be removed in a future version")]
        TIME
    }
}