// <copyright file="EntityManagerTest.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of OmegaTests
// OmegaTests is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// OmegaTests is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with OmegaTests. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-19</date>

using System;
using DBApi;
using OmegaTests.Entities;
using Xunit;

namespace OmegaTests.Integration
{
    public class EntityManagerTest
    {
        private string GetConnectionString()
        {
            var password = Environment.GetEnvironmentVariable("SA_PASSWORD");
            return "Server=127.0.0.1;Database=OmegaUnitTests;User Id=sa;Password=" + password;
        }

        [Fact]
        private void TestPersistSingleCategory()
        {
            var entityManager = new EntityManager(GetConnectionString());
            var category = new Category();
            category.Title = "Unit Test Generated Category";

            category = entityManager.Persist<Category>(category);
            
            Assert.NotNull(category);
            Assert.IsType<int>(category.CategoryId);
            
        }

        [Fact]
        private void TestFetchSingleCategoryFromDatabase()
        {
            var entityManager = new EntityManager(GetConnectionString());
            Category category = entityManager.FindById<Category>(1);
            Assert.NotNull(category);
            Assert.Equal(1, category.CategoryId);
        }

        [Fact]
        public void TestEntityManagerCreationWithNoConnectionString()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var entityManager = new EntityManager(string.Empty);
            });
        }

        [Fact]
        public void TestEntityManagerPersistenceWithNoObject()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var em = new EntityManager(GetConnectionString());
                Category category = null;
                var test = em.Persist<Category>(category);
            });
        }

        [Fact]
        public void TestEntityManagerPersistenceWithNoConnectionRetries()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var em = new EntityManager(GetConnectionString());
                Category category = new Category();
                category.Title = "Outsmart Category";
                var test = em.Persist(typeof(Category), category, -1);
            });
        }
    }
}