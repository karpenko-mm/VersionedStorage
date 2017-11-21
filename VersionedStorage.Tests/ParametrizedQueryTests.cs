using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VersionedStorage.Tests
{
    /// <summary>
    /// Тесты класса ParametrizedQuery
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ParametrizedQueryTests
    {
        [TestMethod]
        public void Constructor_Text_FieldsAreInitialized()
        {
            String queryText = "select * from test";
            ParametrizedQuery query = new ParametrizedQuery(queryText);
            Assert.AreEqual(queryText, query.Text);
        }


        [TestMethod]
        public void Constructor_TextAndParams_FieldsAreInitialized()
        {
            String queryText = "select * from test";
            Dictionary<String, object> queryParams = new Dictionary<string, object>();

            ParametrizedQuery query = new ParametrizedQuery(queryText, queryParams);
            Assert.AreEqual(queryText, query.Text);
            Assert.AreEqual(queryParams, query.Parameters);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_TextIsNullOrEmpty1_Exception()
        {
            ParametrizedQuery query = new ParametrizedQuery(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_TextIsNullOrEmpty2_Exception()
        {
            Dictionary<String, object> queryParams = new Dictionary<string, object>();

            ParametrizedQuery query = new ParametrizedQuery(null, queryParams);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ParamsIsNull_Exception()
        {
            String queryText = "select * from test";
            ParametrizedQuery query = new ParametrizedQuery(queryText, null);
        }   
    }
}
