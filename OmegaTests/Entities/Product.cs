// <copyright file="Product.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of OmegaTests
// OmegaTests is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// OmegaTests is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with OmegaTests. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-19</date>

using System;
using System.Runtime.CompilerServices;
using DBApi.Annotations;
using DBApi.Attributes;

namespace OmegaTests.Entities
{
    [Entity]
    [Table("Products")]
    [CacheControl()]
    public class Product
    {
        [Identity] [Column("ProductId", ColumnType.Int32, false, true)]
        private Int32 _productId;

        public int ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        [Column("Title", ColumnType.String, false)]
        private string _title;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        [Column("Slug", ColumnType.String, false, true)]
        private string _slug;

        public string Slug
        {
            get => _slug;
            set => _slug = value;
        }

        [Column("Description", ColumnType.String, false, false)]
        private string _description;

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        [Column("SKU", ColumnType.String, true, true)] [CanBeNull]
        private string _sku;

        [CanBeNull]
        public string Sku
        {
            get => _sku;
            set => _sku = value;
        }

        [Column("ProductCode", ColumnType.String, false, true)]
        private string _productCode;

        public string ProductCode
        {
            get => _productCode;
            set => _productCode = value;
        }

        [Column("CategoryId", ColumnType.Int32, true, false)] [ManyToOne(typeof(Category), "CategoryId")] [CanBeNull]
        private Category _category;

        [CanBeNull]
        public Category Category
        {
            get => _category;
            set => _category = value;
        }
    }
}