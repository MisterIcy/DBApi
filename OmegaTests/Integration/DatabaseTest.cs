// <copyright file="DatabaseTest.cs">
// Copyright 2020 Alexandros Koutroulis <icyd3mon@gmail.com>
// This file is part of OmegaTests
// OmegaTests is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// OmegaTests is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with OmegaTests. If not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Alexandros Koutroulis</author>
// <date>2021-01-18</date>

using System;
using System.Data.SqlClient;
using DBApi;
using DBApi.QueryBuilder;
using Xunit;

namespace OmegaTests.Integration
{
    public class DatabaseTest
    {
        private const string TestDbName = "OmegaTestDb";
        [Fact]
        public void CreateNewDatabase()
        {;
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();
            var expression = new CreateDatabase(TestDbName);
            using var statement = new Statement(expression.ToString(), connection);
            var result = statement.Execute();
            connection.Close();
            Assert.Equal(-1, result);
        }

        private string GetConnectionString()
        {
            var password = Environment.GetEnvironmentVariable("SA_PASSWORD");
            return "Server=127.0.0.1;Database=OmegaUnitTests;User Id=sa;Password=" + password;
        }
    }
}