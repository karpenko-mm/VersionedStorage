using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using System.Data;

namespace VersionedStorage.Tests
{
    /// <summary>
    /// Тесты класса SqlVersionedStorage
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SqlVersionedStorageTests
    {
        #region Инициализация тестов

        private Mock<IDbTransaction> TransactionMock;
        private String UserId;

        public SqlVersionedStorageTests()
        {
            TransactionMock = new Mock<IDbTransaction>();
            UserId = "testUser";
        }

        #endregion


        #region Тесты конструктора

        [TestMethod]
        public void Constructor_TransactionAndUserId_FieldsInitialized()
        {
            SqlVersionedStorage storage = new SqlVersionedStorage(TransactionMock.Object, UserId);

            Assert.AreEqual(UserId, storage.UserId);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TransactionIsNull_Exception()
        {
            SqlVersionedStorage storage = new SqlVersionedStorage(null, UserId);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_UserIdIsNullOrEmpry_Exception()
        {
            SqlVersionedStorage storage = new SqlVersionedStorage(TransactionMock.Object, null);
        }

        #endregion
    }
}
