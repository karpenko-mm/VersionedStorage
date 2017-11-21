using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace VersionedStorage.Example
{
    /// <summary>
    /// Демонстрация примера использования библиотеки VersionedStorage
    /// для управления версионным хранением данных в БД
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Utils.SetupConsoleParameters();
            Utils.SetupSampleDatabase();

            Console.WriteLine("Пример использования класса для управления версионным хранением данных:");
            Console.WriteLine();

            using (SqlConnection connection = SampleDatabaseConnectionFactory.CreateConnection())
            {
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    //создание объекта управления версионным хранением данных.
                    //изменения будут производиться в транзакции transaction, автором изменений будет "testUser"
                    SqlVersionedStorage versionedStorage = new SqlVersionedStorage(transaction, "testUser");

                    //добавление новой записи
                    int firstRecordVersionId = versionedStorage.Insert(
                        "SAMPLE_TABLE",
                        new Dictionary<string, object>()
                        {
                            { "INVOICE_NO", "2017-0001" },
                            { "SUPPLIER", "ACME LLC." },
                            { "PRICE", 1200 },
                            { "WEIGHT", 850 }
                        });

                    Console.WriteLine("Добавлена новая запись:");
                    Utils.DumpSampleTable(versionedStorage);

                    //внесение изменений в существующую запись
                    //с сохранением истории изменений
                    int secondRecordVersionId = versionedStorage.Update(
                        "SAMPLE_TABLE",
                        firstRecordVersionId,
                        new Dictionary<string, object>()
                        {
                            { "INVOICE_NO", "2017-0001" },
                            { "SUPPLIER", "ACME LTD." },
                            { "PRICE", 12000 },
                            { "WEIGHT", 850 }
                        });

                    Console.WriteLine();
                    Console.WriteLine(String.Format("Запись с версией {0} обновлена на новую версию {1}:", firstRecordVersionId, secondRecordVersionId));
                    Utils.DumpSampleTable(versionedStorage);

                    //удаление записи
                    //с сохранением истории изменений
                    versionedStorage.Delete("SAMPLE_TABLE", secondRecordVersionId);

                    Console.WriteLine();
                    Console.WriteLine("Запись удалена:");
                    Utils.DumpSampleTable(versionedStorage);

                    transaction.Commit();
                }
            }

            Console.ReadKey();
        }


        
    } 
}
