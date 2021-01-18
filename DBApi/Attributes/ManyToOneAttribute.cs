// <copyright file="ManyToOneAttribute.cs">
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
    /// Defines a Many To One relationship
    /// </summary>
    [AttributeUsage(AttributeTargets.Field )]
    [PublicAPI]
    public sealed class ManyToOneAttribute : Attribute
    {
        /// <summary>
        /// Entity type that is linked to present entity
        /// </summary>
        public Type TargetEntity { get; }
        /// <summary>
        /// Identifier column
        /// </summary>
        public string IdentifierColumn { get; }
        /// <summary>
        /// Creates a new Many to One relationship
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="identifier"></param>
        public ManyToOneAttribute(Type targetEntity, string identifier)
        {
            this.TargetEntity = targetEntity ?? throw new ArgumentNullException(nameof(targetEntity));
            
            IdentifierColumn = string.IsNullOrEmpty(identifier) ?
                throw new ArgumentNullException(nameof(identifier)) :
                IdentifierColumn = identifier;
        }
    }
}