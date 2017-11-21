using System;
using System.Data;

namespace VersionedStorage
{
    /// <summary>
    /// Создание типовых исключений
    /// </summary>
    internal static class Exceptions
    {
        /// <summary>
        /// Создание исключения ArgumentException с текстом "Значение аргумента ... не должно быть пустым"
        /// </summary>
        /// <param name="argumentName"></param>
        /// <returns>ArgumentException</returns>
        public static ArgumentException ArgumentEmptyException(String argumentName)
        {
            String msg = String.Format(Properties.Resources.ArgumentIsEmptyExceptionMessage, argumentName);
            return new ArgumentException(msg);
        }


        /// <summary>
        /// Создание исключения ArgumentException с текстом "Коллекция ... не должна быть пуста"
        /// </summary>
        /// <param name="argumentName"></param>
        /// <returns>ArgumentException</returns>
        public static ArgumentException CollectionEmptyException(String argumentName)
        {
            String msg = String.Format(Properties.Resources.CollectionIsEmptyExceptionMessage, argumentName);
            return new ArgumentException(msg);
        }


        /// <summary>
        /// Создание исключения  с текстом "Актуальная версия записи RECORD_ID=... не сущесвует"
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns>DBConcurrencyException</returns>
        public static DBConcurrencyException DbRecordNotExistsException(int recordId)
        {
            String msg = String.Format(Properties.Resources.RecordNotExistsExceptionMessage, recordId);
            return new DBConcurrencyException(msg);
        }
    }
}
