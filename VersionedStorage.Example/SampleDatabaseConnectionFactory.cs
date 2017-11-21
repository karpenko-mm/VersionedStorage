using System;
using System.Configuration;
using System.Data.SqlClient;

namespace VersionedStorage.Example
{
    /// <summary>
    /// Реализация подключения к тестовой БД
    /// </summary>
    class SampleDatabaseConnectionFactory
    {
        /// <summary>
        /// Создание и открытие подключения к тестовой БД
        /// </summary>
        /// <returns></returns>
        public static SqlConnection CreateConnection()
        {
            String connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
