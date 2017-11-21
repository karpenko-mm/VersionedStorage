using System;
using System.Data;

namespace VersionedStorage
{
    /// <summary>
    /// Реализация операций версионного изменения данных в источнике, поддерживающем транзакции и синтаксис SQL
    /// </summary>
    public class SqlVersionedStorage: GenericVersionedStorage
    {
        /// <summary>
        /// Создание объекта для выполнения операций версионного изменения данных
        /// </summary>
        /// <param name="transaction">Транзакция в источнике данных</param>
        /// <param name="userId">ID пользователя, от имени котрого производится изменение данных</param>
        public SqlVersionedStorage(IDbTransaction transaction, String userId)
            : base(new SqlQueryGenerator(),
                  new SqlQueryExecutor(transaction),
                  userId)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");
            if (String.IsNullOrEmpty(userId))
                throw Exceptions.ArgumentEmptyException("userId");
        }
    }
}
