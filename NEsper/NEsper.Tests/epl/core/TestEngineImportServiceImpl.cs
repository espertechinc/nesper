///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestEngineImportServiceImpl 
    {
        EngineImportServiceImpl _engineImportService;
    
        [SetUp]
        public void SetUp()
        {
            _engineImportService = new EngineImportServiceImpl(true, true, true, false, null);
        }
    
        [Test]
        public void TestResolveMethodNoArgTypes()
        {
            var method = _engineImportService.ResolveMethod("System.Math", "Sqrt");
            Assert.AreEqual(typeof(Math).GetMethod("Sqrt", new Type[] {typeof(double)}), method);
    
            try
            {
                _engineImportService.ResolveMethod("System.Math", "Abs");
                Assert.Fail();
            }
            catch (EngineImportException ex)
            {
                Assert.AreEqual("Ambiguous method name: method by name 'Abs' is overloaded in class 'System.Math'", ex.Message);
            }
        }
    
        [Test]
        public void TestAddAggregation()
        {
            _engineImportService.AddAggregation("abc", new ConfigurationPlugInAggregationFunction("abc", "abcdef.G"));
            _engineImportService.AddAggregation("abcDefGhk", new ConfigurationPlugInAggregationFunction("abcDefGhk", "ab"));
            _engineImportService.AddAggregation("a", new ConfigurationPlugInAggregationFunction("a", "Yh"));
    
            TryInvalidAddAggregation("g h", "");
            TryInvalidAddAggregation("gh", "j j");
            TryInvalidAddAggregation("abc", "hhh");
        }
    
        [Test]
        public void TestResolveAggregationMethod()
        {
            _engineImportService.AddAggregation("abc", new ConfigurationPlugInAggregationFunction("abc", typeof(SupportPluginAggregationMethodOneFactory).FullName));
            Assert.IsTrue(_engineImportService.ResolveAggregationFactory("abc") is SupportPluginAggregationMethodOneFactory);
        }
    
        [Test]
        public void TestInvalidResolveAggregation()
        {
            try
            {
                _engineImportService.ResolveAggregationFactory("abc");
            }
            catch (EngineImportUndefinedException)
            {
                // expected
            }
            
            _engineImportService.AddAggregation("abc", new ConfigurationPlugInAggregationFunction("abc", "abcdef.G"));
            try
            {
                _engineImportService.ResolveAggregationFactory("abc");
            }
            catch (EngineImportException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestResolveClass()
        {
            String className = "System.Math";
            Type expected = typeof(Math);
            Assert.AreEqual(expected, _engineImportService.ResolveTypeInternal(className, false));

            _engineImportService.AddImport("System.Math");
            Assert.AreEqual(expected, _engineImportService.ResolveTypeInternal(className, false));
    
            _engineImportService.AddImport("System");
            className = "String";
            expected = typeof(String);
            Assert.AreEqual(expected, _engineImportService.ResolveTypeInternal(className, false));
        }
    
        [Test]
        public void TestResolveClassInvalid()
        {
            String className = "Math";
            try
            {
                _engineImportService.ResolveTypeInternal(className, false);
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestAddImport()
        {
            _engineImportService.AddImport("System.Math");
            Assert.AreEqual(1, _engineImportService.Imports.Length);
            Assert.AreEqual(new AutoImportDesc("System.Math"), _engineImportService.Imports[0]);
    
            _engineImportService.AddImport("System");
            Assert.AreEqual(2, _engineImportService.Imports.Length);
            Assert.AreEqual(new AutoImportDesc("System.Math"), _engineImportService.Imports[0]);
            Assert.AreEqual(new AutoImportDesc("System"), _engineImportService.Imports[1]);
        }
    
        [Test]
        public void TestAddImportInvalid()
        {
            try
            {
                _engineImportService.AddImport("System.*");
                Assert.Fail();
            }
            catch (EngineImportException)
            {
                // Expected
            }
    
            try
            {
                _engineImportService.AddImport("System..Math");
                Assert.Fail();
            }
            catch (EngineImportException)
            {
                // Expected
            }
        }
    
        private void TryInvalidAddAggregation(String funcName, String className)
        {
            try
            {
                _engineImportService.AddAggregation(funcName, new ConfigurationPlugInAggregationFunction(funcName, className));
            }
            catch (EngineImportException)
            {
                // expected
            }
        }
    }
}
