using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Интерфейс операций версионного изменения данных
    /// </summary>
    public interface IVersionedStorage
    {
        /// <summary>
        /// Создание новой записи в источник данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="fieldValues">Значения полей добавляемой записи</param>
        /// <returns>ID первой версии созданной записи</returns>
        int Insert(String tableName, Dictionary<String, object> fieldValues);

        /// <summary>
        /// Обновление существующей записи в источнике данных с поддержкой версионности
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="recordId">ID обновляемой версии записи</param>
        /// <param name="newFieldValues">Новые значения полей обновляемой записи</param>
        /// <returns>ID новой версии обновленной записи</returns>
        int Update(String tableName, int recordId, Dictionary<String, object> newFieldValues);

        /// <summary>
        /// Удаление существующей записи в источнике данных с поддержкой версионности
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="recordId">ID удаляемой версии записи</param>
        void Delete(String tableName, int recordId);
    }
}
