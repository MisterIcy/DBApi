// <copyright file="ConnectionTest.cs">
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
using Xunit;

namespace OmegaTests
{
    public class ConnectionTest
    {
        [Fact]
        public void CreateConnection()
        {
            var password = Environment.GetEnvironmentVariable("SA_PASSWORD");
            var connectionString = "Server=127.0.0.1;Database=OmegaUnitTests;User Id=sa;Password=" + password;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    connection.Close();

                }
            }
            catch (Exception exception)
            {
                Assert.False(true);
            }
            Assert.True(true);
        }
    }
}