using System;
using System.Collections.Generic;
using System.Data;

namespace VersionedStorage
{
    /// <summary>
    /// Исполнитель запросов к источнику данных, поддерживающему транзакции и синтаксис SQL
    /// </summary>
    public class SqlQueryExecutor : IQueryExecutor
    {
        /// <summary>
        /// Транзакция в источнике данных
        /// </summary>
        public IDbTransaction Transaction { get; private set; }


        private SqlQueryExecutor()
        {
        }


        /// <summary>
        /// Создание исполнителя запросов с заданной транзакцией
        /// </summary>
        /// <param name="transaction">Транзакция в источнике данных</param>
        public SqlQueryExecutor(IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            Transaction = transaction;
        }


        /// <summary>
        /// Выполнение инструкции источника данных
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <returns>Количество задействованных в запросе строк источника данных</returns>
        public virtual int ExecuteNonQuery(IParametrizedQuery query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (String.IsNullOrEmpty(query.Text))
                throw Exceptions.ArgumentEmptyException("query.Text");

            IDbConnection connection = Transaction.Connection;
            using (IDbCommand cmd = PrepareDbCommand(query.Text, query.Parameters))
            {
                int result = cmd.ExecuteNonQuery();
                return result;
            }
        }


        /// <summary>
        /// Выполнение запроса к источнику данных, возвращающего одно значение
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <returns>Значение, возвращенное источником данных</returns>
        public virtual object ExecuteScalar(IParametrizedQuery query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (String.IsNullOrEmpty(query.Text))
                throw Exceptions.ArgumentEmptyException("query.Text");

            IDbConnection connection = Transaction.Connection;
            using (IDbCommand cmd = PrepareDbCommand(query.Text, query.Parameters))
            {
                object result = cmd.ExecuteScalar();
                return result;
            }
        }


        /// <summary>
        /// Выполнение запроса к источнику данных, возвращающего набор записей
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <param name="rowLimit">Ограничение на число записей, получаемое из источника. Записи сверх указанного количества не будут получены из источника</param>
        /// <returns>Таблица с набором записей</returns>
        public virtual DataTable ExecuteSelect(IParametrizedQuery query, int rowLimit = Int32.MaxValue)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (String.IsNullOrEmpty(query.Text))
                throw Exceptions.ArgumentEmptyException("query.Text");
            if (rowLimit <= 0)
                throw new ArgumentOutOfRangeException("rowLimit");

            IDbConnection connection = Transaction.Connection;
            using (IDbCommand cmd = PrepareDbCommand(query.Text, query.Parameters))
            {
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    CreateDataTableColumns(dataReader, dataTable);
                    int fieldCount = dataReader.FieldCount;

                    while (dataTable.Rows.Count < rowLimit
                        && dataReader.Read())
                    {
                        DataRow row = dataTable.NewRow();

                        for (int i = 0; i < fieldCount; i++)
                        {
                            String fieldName = dataReader.GetName(i);
                            object fieldValue = dataReader.GetValue(i);                            
                            row[fieldName] = fieldValue;
                        }

                        dataTable.Rows.Add(row);
                    }

                    return dataTable;
                }
            }
        }


        /// <summary>
        /// Формирование столбцов в таблице в соответствии с источником данных
        /// </summary>
        /// <param name="dataReader">Объект DataReader, готовый для получения записей из источника данных</param>
        /// <param name="dataTable">Таблица данных</param>
        /// <returns></returns>
        private DataTable CreateDataTableColumns(IDataReader dataReader, DataTable dataTable)
        {
            int fieldCount = dataReader.FieldCount;

            dataTable.Rows.Clear();
            dataTable.Columns.Clear();

            for (int i = 0; i < fieldCount; i++)
            {
                String fieldName = dataReader.GetName(i);
                Type fieldType = dataReader.GetFieldType(i);
                dataTable.Columns.Add(fieldName, fieldType);
            }

            return dataTable;
        }


        /// <summary>
        /// Создание и инициализация объекта IDbCommand для выполнения инструкции к источнику данных
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Набор параметров запроса</param>
        /// <returns></returns>
        private IDbCommand PrepareDbCommand(string query, Dictionary<string, object> parameters)
        {
            IDbConnection connection = Transaction.Connection;
            IDbCommand command = connection.CreateCommand();
            command.Transaction = Transaction;
            command.CommandText = query;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    IDbDataParameter dbParameter = PrepareDbDataParameter(command, parameter.Key, parameter.Value);
                    command.Parameters.Add(dbParameter);
                }
            }

            return command;
        }


        /// <summary>
        /// Создание и инициализация параметра IDbParameter для передачи в инструкцию к источнику данных
        /// </summary>
        /// <param name="command">Инструкция к источнику данных</param>
        /// <param name="parameterName">Имя параметра</param>
        /// <param name="parameterValue">Значение параметра</param>
        /// <returns></returns>
        private IDbDataParameter PrepareDbDataParameter(IDbCommand command, String parameterName, object parameterValue)
        {
            IDbDataParameter dbParameter = command.CreateParameter();
            dbParameter.ParameterName = parameterName;
            dbParameter.Direction = ParameterDirection.Input;

            if (parameterValue == null
                || (parameterValue is DateTime && Convert.ToDateTime(parameterValue) == DateTime.MinValue)
                || (parameterValue is String && String.IsNullOrEmpty(parameterValue as String))
                )
            {
                dbParameter.Value = System.DBNull.Value;
            }
            else
            {
                dbParameter.Value = parameterValue;
            }

            return dbParameter;
        }
    }
}
