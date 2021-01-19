// <copyright file="Category.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of OmegaTests
// OmegaTests is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// OmegaTests is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with OmegaTests. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-19</date>

using System;
using DBApi.Attributes;

namespace OmegaTests.Entities
{
    [Entity]
    [Table("Categories")]
    public class Category
    {
        [Identity] [Column("CategoryId", ColumnType.Int32, false, true)]
        private Int32 _categoryId;

        public int CategoryId
        {
            get => _categoryId;
            set => _categoryId = value;
        }

        [Column("Title")] 
        private string _title;

        public string Title
        {
            get => _title;
            set => _title = value;
        }
    }
}