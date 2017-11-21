using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Реализация операций версионного изменения данных в произвольном источнике данных
    /// </summary>
    public class GenericVersionedStorage : IVersionedStorage
    {
        /// <summary>
        /// Генератор запросов к источнику данных
        /// </summary>
        public IQueryGenerator QueryGenerator { get; private set; }

        /// <summary>
        /// Исполнитель запросов к источнику данных
        /// </summary>
        public IQueryExecutor QueryExecutor { get; private set; }

        /// <summary>
        /// ID пользователя, от имени котрого производится изменение данных
        /// </summary>
        public String UserId { get; private set; }


        private GenericVersionedStorage()
        {
        }


        /// <summary>
        /// Создание объекта для выполнения операций версионного изменения данных
        /// </summary>
        /// <param name="queryGenerator">Генератор запросов к источнику данных</param>
        /// <param name="queryExecutor">Исполнитель запросов к источнику данных</param>
        /// <param name="userId">ID пользователя, от имени котрого производится изменение данных</param>
        public GenericVersionedStorage(IQueryGenerator queryGenerator, IQueryExecutor queryExecutor, String userId)
        {
            if (queryGenerator == null)
                throw new ArgumentNullException("queryGenerator");
            if (queryExecutor == null)
                throw new ArgumentNullException("queryExecutor");
            if (String.IsNullOrEmpty(userId))
                throw Exceptions.ArgumentEmptyException("userId");

            QueryGenerator = queryGenerator;
            QueryExecutor = queryExecutor;
            UserId = userId;
        }


        /// <summary>
        /// Создание новой записи в источник данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="fieldValues">Значения полей добавляемой записи</param>
        /// <returns>ID первой версии созданной записи</returns>
        public int Insert(string tableName, Dictionary<string, object> fieldValues)
        {
            if (String.IsNullOrEmpty(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (fieldValues == null)
                throw new ArgumentNullException("fieldValues");
            if (fieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("fieldValues");

            //вставка новой записи
            Dictionary<string, object> fullFieldValues = new Dictionary<string, object>(fieldValues);
            fullFieldValues["CREATED_BY"] = UserId;
            fullFieldValues["CREATED_DATE"] = DateTime.Now;
            fullFieldValues["IS_ACTIVE"] = 1;
            IParametrizedQuery insertQuery = QueryGenerator.GenerateInsert(tableName, fullFieldValues);
            QueryExecutor.ExecuteNonQuery(insertQuery);

            //получение ID вставленной записи (RECORD_ID)
            IParametrizedQuery selectQuery = QueryGenerator.GenerateSelect(
                tableName,
                new String[] { "max(RECORD_ID)" },
                new Dictionary<string, object>()
                {
                    { "CREATED_BY", fullFieldValues["CREATED_BY"] },
                    { "CREATED_DATE", fullFieldValues["CREATED_DATE"] },
                    { "IS_ACTIVE", 1 }
                });

            object objLastRecordId = QueryExecutor.ExecuteScalar(selectQuery);
            return Convert.ToInt32(objLastRecordId);
        }


        /// <summary>
        /// Обновление существующей записи в источнике данных с поддержкой версионности
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="recordId">ID обновляемой версии записи</param>
        /// <param name="newFieldValues">Новые значения полей обновляемой записи</param>
        /// <returns>ID новой версии обновленной записи</returns>
        public int Update(string tableName, int recordId, Dictionary<string, object> newFieldValues)
        {
            if (String.IsNullOrEmpty(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (newFieldValues == null)
                throw new ArgumentNullException("newFieldValues");
            if (newFieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("newFieldValues");

            //пометка существующей записи как неактуальной (IS_ACTIVE=0)
            IParametrizedQuery updateQuery = QueryGenerator.GenerateUpdate(
                tableName,
                new Dictionary<string, object>()
                {
                    { "RECORD_ID", recordId },
                    { "IS_ACTIVE", 1 }
                },
                new Dictionary<string, object>()
                {
                    { "IS_ACTIVE", 0 }
                });

            int rowsAffected = QueryExecutor.ExecuteNonQuery(updateQuery);
            if (rowsAffected == 0)
                throw Exceptions.DbRecordNotExistsException(recordId);

            //вставка новой версии записи со ссылкой на предыдущую версию (PREV_RECORD_ID = recordId)
            Dictionary<string, object> fullFieldValues = new Dictionary<string, object>(newFieldValues);
            fullFieldValues["PREV_RECORD_ID"] = recordId;
            return Insert(tableName, fullFieldValues);
        }


        /// <summary>
        /// Удаление существующей записи в источнике данных с поддержкой версионности
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="recordId">ID удаляемой версии записи</param>
        public void Delete(string tableName, int recordId)
        {
            if (String.IsNullOrEmpty(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");

            IParametrizedQuery query = QueryGenerator.GenerateUpdate(
                tableName,
                new Dictionary<string, object>()
                {
                    { "RECORD_ID", recordId },
                    { "IS_ACTIVE", 1 }
                },
                new Dictionary<string, object>()
                {
                    { "IS_ACTIVE", 0 },
                    { "DELETED_BY", UserId },
                    { "DELETED_DATE", DateTime.Now }
                });

            int rowsAffected = QueryExecutor.ExecuteNonQuery(query);
            if (rowsAffected == 0)
                throw Exceptions.DbRecordNotExistsException(recordId);
        }        
    }
}
