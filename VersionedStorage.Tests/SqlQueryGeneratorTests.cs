using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VersionedStorage.Tests
{
    /// <summary>
    /// Тесты класса SqlQueryGenerator
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SqlQueryGeneratorTests
    {
        #region Тесты GenerateInsert()

        [TestMethod]
        public void GenerateInsert_WithFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateInsert("TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 },
                    { "DOUBLE_FIELD", 1.25 },
                    { "DATE_FIELD", new DateTime(2015, 01, 01) },
                    { "STRING_FIELD", "test" }
                });

            Assert.AreEqual("insert into TEST_TABLE (INT_FIELD, DOUBLE_FIELD, DATE_FIELD, STRING_FIELD) values (@param0, @param1, @param2, @param3)", query.Text);
            Assert.AreEqual(4, query.Parameters.Count);
            Assert.AreEqual(1, query.Parameters["param0"]);
            Assert.AreEqual(1.25, query.Parameters["param1"]);
            Assert.AreEqual(new DateTime(2015, 01, 01), query.Parameters["param2"]);
            Assert.AreEqual("test", query.Parameters["param3"]);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateInsert_TableNameIsNullOrEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateInsert(
                null,
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateInsert_FieldsEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateInsert(
                "TEST_TABLE",
                new Dictionary<string, object>());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateInsert_FieldsIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateInsert(
                "TEST_TABLE",
                null);
        }

        #endregion


        #region Тесты GenerateUpdate()

        [TestMethod]
        public void GenerateUpdate_WithKeyAndValueFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 },
                    { "DOUBLE_FIELD", 1.25 },
                    { "DATE_FIELD", new DateTime(2015, 01, 01) },
                    { "STRING_FIELD", "test" }
                },
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 10 },
                    { "DOUBLE_FIELD", 10.25 },
                    { "DATE_FIELD", new DateTime(2017, 09, 09) },
                    { "STRING_FIELD", "updated" }
                });

            Assert.AreEqual("update TEST_TABLE set INT_FIELD=@param4, DOUBLE_FIELD=@param5, DATE_FIELD=@param6, STRING_FIELD=@param7 where INT_FIELD=@param0 and DOUBLE_FIELD=@param1 and DATE_FIELD=@param2 and STRING_FIELD=@param3", query.Text);
            Assert.AreEqual(8, query.Parameters.Count);
            Assert.AreEqual(1, query.Parameters["param0"]);
            Assert.AreEqual(1.25, query.Parameters["param1"]);
            Assert.AreEqual(new DateTime(2015, 01, 01), query.Parameters["param2"]);
            Assert.AreEqual("test", query.Parameters["param3"]);
            Assert.AreEqual(10, query.Parameters["param4"]);
            Assert.AreEqual(10.25, query.Parameters["param5"]);
            Assert.AreEqual(new DateTime(2017, 09, 09), query.Parameters["param6"]);
            Assert.AreEqual("updated", query.Parameters["param7"]);
        }


        [TestMethod]
        public void GenerateUpdate_WithNullableKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "NULL_FIELD", null }
                },
                new Dictionary<string, object>()
                {
                    { "NULL_FIELD", "new_value" }
                });

            Assert.AreEqual("update TEST_TABLE set NULL_FIELD=@param1 where NULL_FIELD is NULL", query.Text);
            Assert.AreEqual(1, query.Parameters.Count);
            Assert.AreEqual("new_value", query.Parameters["param1"]);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateUpdate_TableNameIsNullOrEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                null,
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 }
                },
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 2 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateUpdate_KeyFieldsEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                new Dictionary<string, object>(),
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateUpdate_KeyFieldsIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                null,
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateUpdate_ValueFieldsEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "KEY_FIELD", 1 }
                },
                new Dictionary<string, object>());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateUpdate_ValueFieldsIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateUpdate(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "KEY_FIELD", 1 }
                },
                null);
        }

        #endregion


        #region Тесты GenerateDelete()

        [TestMethod]
        public void GenerateDelete_WithKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateDelete(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 },
                    { "DOUBLE_FIELD", 1.25 },
                    { "DATE_FIELD", new DateTime(2015, 01, 01) },
                    { "STRING_FIELD", "test" }
                });

            Assert.AreEqual("delete from TEST_TABLE where INT_FIELD=@param0 and DOUBLE_FIELD=@param1 and DATE_FIELD=@param2 and STRING_FIELD=@param3", query.Text);
            Assert.AreEqual(4, query.Parameters.Count);
            Assert.AreEqual(1, query.Parameters["param0"]);
            Assert.AreEqual(1.25, query.Parameters["param1"]);
            Assert.AreEqual(new DateTime(2015, 01, 01), query.Parameters["param2"]);
            Assert.AreEqual("test", query.Parameters["param3"]);
        }


        [TestMethod]
        public void GenerateDelete_WithNullableKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateDelete(
                "TEST_TABLE",
                new Dictionary<string, object>()
                {
                    { "NULL_FIELD", null }
                });

            Assert.AreEqual("delete from TEST_TABLE where NULL_FIELD is NULL", query.Text);
            Assert.AreEqual(0, query.Parameters.Count);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateDelete_TableNameIsNullOrEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateDelete(
                null,
                new Dictionary<string, object>()
                {
                    { "FIELD_1", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateDelete_KeyFieldValuesEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateDelete(
                "TEST_TABLE",
                new Dictionary<string, object>());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateDelete_KeyFieldValuesIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateDelete(
                "TEST_TABLE",
                null);
        }

        #endregion


        #region Тесты GenerateSelect()

        [TestMethod]
        public void GenerateSelect_WithKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateSelect("TEST_TABLE",
                new String[] { "FIELD1", "FIELD2", "FIELD3" },
                new Dictionary<string, object>()
                {
                    { "INT_FIELD", 1 },
                    { "DOUBLE_FIELD", 1.25 },
                    { "DATE_FIELD", new DateTime(2015, 01, 01) },
                    { "STRING_FIELD", "test" }
                });

            Assert.AreEqual("select FIELD1, FIELD2, FIELD3 from TEST_TABLE where INT_FIELD=@param0 and DOUBLE_FIELD=@param1 and DATE_FIELD=@param2 and STRING_FIELD=@param3", query.Text);
            Assert.AreEqual(4, query.Parameters.Count);
            Assert.AreEqual(1, query.Parameters["param0"]);
            Assert.AreEqual(1.25, query.Parameters["param1"]);
            Assert.AreEqual(new DateTime(2015, 01, 01), query.Parameters["param2"]);
            Assert.AreEqual("test", query.Parameters["param3"]);
        }


        [TestMethod]
        public void GenerateSelect_WithNullableKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateSelect("TEST_TABLE",
                new String[] { "FIELD1", "FIELD2", "FIELD3" },
                new Dictionary<string, object>()
                {
                    { "NULL_FIELD", null }
                });

            Assert.AreEqual("select FIELD1, FIELD2, FIELD3 from TEST_TABLE where NULL_FIELD is NULL", query.Text);
            Assert.AreEqual(0, query.Parameters.Count);
        }


        [TestMethod]
        public void GenerateSelect_NoKeyFields_QueryAndParamsConfigured()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query = sqlGen.GenerateSelect("TEST_TABLE",
                new String[] { "FIELD1", "FIELD2", "FIELD3" },
                new Dictionary<string, object>());

            Assert.AreEqual("select FIELD1, FIELD2, FIELD3 from TEST_TABLE", query.Text);
            Assert.AreEqual(0, query.Parameters.Count);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateSelect_TableNameIsNullOrEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query1 = sqlGen.GenerateSelect(
                null,
                new string[] { "FIELD_1", "FIELD_2" },
                new Dictionary<string, object>()
                {
                    { "KEY_1", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateSelect_SelectFieldsEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query1 = sqlGen.GenerateSelect(
                "TEST_TABLE",
                new string[0],
                new Dictionary<string, object>()
                {
                    { "KEY_1", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateSelect_SelectFieldsIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query1 = sqlGen.GenerateSelect(
                "TEST_TABLE",
                null,
                new Dictionary<string, object>()
                {
                    { "KEY_1", 1 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateSelect_KeyFieldsIsNull_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            IParametrizedQuery query1 = sqlGen.GenerateSelect(
                "TEST_TABLE",
                new string[] { "FIELD_1", "FIELD_2" },
                null);
        }

        #endregion


        #region Тесты GenerateParameter()

        [TestMethod]
        public void GenerateParameter_ParamNameIsNotEmpty_NameWithPrefixReturned()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();
            String paramName = "paramName";
            String paramNameExpected = String.Format("@{0}", paramName);

            String paramNameGenerated = sqlGen.GenerateParameter(paramName);

            Assert.AreEqual(paramNameExpected, paramNameGenerated);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateParameter_ParamNameIsNullOrEmpty_Exception()
        {
            SqlQueryGenerator sqlGen = new SqlQueryGenerator();

            sqlGen.GenerateParameter(null);
        }

        #endregion
    }
}
