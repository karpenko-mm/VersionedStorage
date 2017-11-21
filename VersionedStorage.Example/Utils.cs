using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace VersionedStorage.Example
{
    /// <summary>
    /// Служебные функции
    /// </summary>
    static class Utils
    {
        /// <summary>
        /// Создание необходимых объектов в тестовой БД
        /// </summary>
        public static void SetupSampleDatabase()
        {
            using (SqlConnection connection = SampleDatabaseConnectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"IF OBJECT_ID('dbo.SAMPLE_TABLE', 'U') IS NOT NULL DROP TABLE dbo.SAMPLE_TABLE";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"CREATE TABLE SAMPLE_TABLE (
                      RECORD_ID INT IDENTITY
                     ,PREV_RECORD_ID INT NULL
                     ,IS_ACTIVE INT NOT NULL
                     ,CREATED_BY NVARCHAR(255) NOT NULL
                     ,CREATED_DATE DATETIME NOT NULL
                     ,DELETED_BY NVARCHAR(255) NULL
                     ,DELETED_DATE DATETIME NULL
                     ,INVOICE_NO NVARCHAR(255) NOT NULL
                     ,SUPPLIER NVARCHAR(255) NOT NULL
                     ,PRICE DECIMAL(18, 2) NOT NULL
                     ,WEIGHT DECIMAL(18, 3) NOT NULL
                     ,PRIMARY KEY (RECORD_ID)
                    )";

                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Установка параметров консоли
        /// </summary>
        public static void SetupConsoleParameters()
        {
            Console.BufferWidth = Math.Max(Console.BufferWidth, 136);
            Console.WindowWidth = Console.BufferWidth + 2;
        }


        /// <summary>
        /// Вывод содержимого таблицы SAMPLE_TABLE тестовой БД в консоль
        /// </summary>
        /// <param name="storage"></param>
        public static void DumpSampleTable(SqlVersionedStorage storage)
        {
            ParametrizedQuery query = new ParametrizedQuery("select * from SAMPLE_TABLE order by RECORD_ID");
            DataTable table = storage.QueryExecutor.ExecuteSelect(query);

            int[] colWidths = new int[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                colWidths[i] = table.Columns[i].ColumnName.Length;
                foreach (DataRow row in table.Rows)
                {
                    colWidths[i] = Math.Max(colWidths[i], row[i].ToString().Length);
                }
            }

            int tableWidth = colWidths.Sum() + colWidths.Length;
            String dividerLine = new string('-', tableWidth);            

            Console.WriteLine(dividerLine);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                int colWidth = colWidths[i];
                Console.Write(table.Columns[i].ColumnName.PadRight(colWidth));
                Console.Write("|");
            }

            Console.WriteLine();
            Console.WriteLine(dividerLine);

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    int colWidth = colWidths[i];
                    String value = row[i].ToString();
                    Console.Write(value.PadRight(colWidth));
                    Console.Write("|");
                }
                Console.WriteLine();
            }

            Console.WriteLine(dividerLine);
        }        
    }
}
