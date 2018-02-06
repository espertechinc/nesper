///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestJsonUtil
    {
        private ExprValidationContext _exprValidationContext;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _exprValidationContext = SupportExprValidationContextFactory.MakeEmpty(_container);
        }
    
        [TearDown]
        public void TearDown()
        {
            _exprValidationContext = null;
        }
    
        [Test]
        public void TestUnmarshal()
        {
            String json;
            Container result;
    
            json = "{'name':'c0', 'def': {'DefString':'a', 'DefBoolPrimitive':true, 'Defintprimitive':10, 'Defintboxed':20}}";
            result = (Container)JsonUtil.ParsePopulate(json, typeof(Container), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.AreEqual("c0", result.Name);
            Assert.AreEqual((int?) 20, result.Def.DefIntBoxed);
            Assert.AreEqual("a", result.Def.DefString);
            Assert.AreEqual(10, result.Def.DefIntPrimitive);
            Assert.AreEqual(true, result.Def.DefBoolPrimitive);
    
            json = "{\"name\":\"c1\",\"abc\":{'class':'TestJsonUtil+BImpl', \"bIdOne\":\"bidentone\",\"bIdTwo\":\"bidenttwo\"}}";
            result = (Container)JsonUtil.ParsePopulate(json, typeof(Container), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.AreEqual("c1", result.Name);
            var bimpl = (BImpl) result.Abc;
            Assert.AreEqual("bidentone", bimpl.BIdOne);
            Assert.AreEqual("bidenttwo", bimpl.BIdTwo);
    
            json = "{\"name\":\"c2\",\"abc\":{'class':'com.espertech.esper.util.TestJsonUtil+AImpl'}}";
            result = (Container)JsonUtil.ParsePopulate(json, typeof(Container), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.AreEqual("c2", result.Name);
            Assert.IsTrue(result.Abc is AImpl);
    
            json = "{'booleanArray': [true, false, true], 'integerArray': [1], 'objectArray': [1, 'abc']}";
            var defOne = (DEF)JsonUtil.ParsePopulate(json, typeof(DEF), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            EPAssertionUtil.AssertEqualsExactOrder(new bool[] {true, false, true}, defOne.BooleanArray);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] {1}, defOne.IntegerArray);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] {1, "abc"}, defOne.ObjectArray);
    
            json = "{defString:'a'}";
            var defTwo = (DEF)JsonUtil.ParsePopulate(json, typeof(DEF), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.IsNull(defTwo.ObjectArray);
    
            json = "{'objectArray':[]}";
            var defThree = (DEF)JsonUtil.ParsePopulate(json, typeof(DEF), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.AreEqual(0, defThree.ObjectArray.Length);
    
            // note: notation for "field: value" does not require quotes around the field name
            json = "{objectArray:[ [1,2] ]}";
            defThree = (DEF)JsonUtil.ParsePopulate(json, typeof(DEF), ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
            Assert.AreEqual(1, defThree.ObjectArray.Length);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{1, 2}, (ICollection<object>) defThree.ObjectArray[0]);
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid(typeof(Container), "'name'",
                    "Failed to map value to object of type com.espertech.esper.util.TestJsonUtil+Container, expected Json Map/Object format, received String");
    
            TryInvalid(typeof(Container), "null",
                    "Failed to map value to object of type com.espertech.esper.util.TestJsonUtil+Container, expected Json Map/Object format, received null");
    
            TryInvalid(typeof(NoCtor), "{a:1}",
                    "Exception instantiating class com.espertech.esper.util.TestJsonUtil+NoCtor, please make sure the class has a public no-arg constructor (and for inner classes is declared static)");
    
            TryInvalid(typeof(ExceptionCtor), "{a:1}",
                    "Exception instantiating class com.espertech.esper.util.TestJsonUtil+ExceptionCtor: Test exception");
    
            TryInvalid(typeof(DEF), "{'dummy': 'def'}",
                    "Failed to find writable property 'dummy' for class com.espertech.esper.util.TestJsonUtil+DEF");
    
            TryInvalid(typeof(DEF), "{'defString': 1}",
                    "Property 'defString' of class com.espertech.esper.util.TestJsonUtil+DEF expects an System.String but receives a value of type System.Int32");

            TryInvalid(typeof (DEF), "{'booleanArray': 1}",
                    "Property 'booleanArray' of class com.espertech.esper.util.TestJsonUtil+DEF expects an array but receives a value of type System.Int32");
    
            TryInvalid(typeof(DEF), "{'booleanArray': [1, 2]}",
                    "Property 'booleanArray (array element)' of class System.Boolean[] expects an System.Boolean but receives a value of type System.Int32");
    
            TryInvalid(typeof(DEF), "{'defString': [1, 2]}",
                    "Property 'defString' of class com.espertech.esper.util.TestJsonUtil+DEF expects an System.String but receives a value of type " + TypeHelper.GetCleanName<List<object>>());
    
            TryInvalid(typeof(Container), "{'abc': 'def'}",
                    "Property 'abc' of class com.espertech.esper.util.TestJsonUtil+Container expects an com.espertech.esper.util.TestJsonUtil+ABC but receives a value of type System.String");
    
            TryInvalid(typeof(Container), "{'abc': {a:1}}",
                    "Failed to find implementation for interface com.espertech.esper.util.TestJsonUtil+ABC, for interfaces please specified the 'class' field that provides the class name either as a simple class name or fully qualified");
    
            TryInvalid(typeof(Container), "{'abc': {'class' : 'x.y.z'}}",
                    "Failed to find implementation for interface com.espertech.esper.util.TestJsonUtil+ABC, could not find class by name 'x.y.z'");
    
            TryInvalid(typeof(Container), "{'abc': {'class' : 'xyz'}}",
                    "Failed to find implementation for interface com.espertech.esper.util.TestJsonUtil+ABC, could not find class by name 'com.espertech.esper.util.xyz'");
    
            TryInvalid(typeof(Container), "{'abc': {'class' : 'System.String'}}",
                    "Failed to find implementation for interface com.espertech.esper.util.TestJsonUtil+ABC, class System.String does not implement the interface");
        }
    
        private void TryInvalid(Type container, String json, String expected)
        {
            try
            {
                JsonUtil.ParsePopulate(json, container, ExprNodeOrigin.SCRIPTPARAMS, _exprValidationContext);
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        public class Container
        {
            public Container()
            {
            }
    
            public Container(String name, ABC abc, DEF def)
            {
                Name = name;
                Abc = abc;
                Def = def;
            }

            public string Name { get; set; }

            public ABC Abc { get; set; }

            public DEF Def { get; set; }
        }
    
        public class DEF
        {
            public DEF()
            {
            }
    
            public DEF(String defString, bool defBoolPrimitive, int defIntPrimitive, int? defIntBoxed)
            {
                DefString = defString;
                DefBoolPrimitive = defBoolPrimitive;
                DefIntPrimitive = defIntPrimitive;
                DefIntBoxed = defIntBoxed;
            }

            public bool[] BooleanArray { get; set; }

            public object[] ObjectArray { get; set; }

            public int[] IntegerArray { get; set; }

            public string DefString { get; set; }

            public bool DefBoolPrimitive { get; set; }

            public int DefIntPrimitive { get; set; }

            public int? DefIntBoxed { get; set; }
        }
    
        public interface ABC {
        }
    
        public class AImpl : ABC {
            public AImpl() {
            }
    
            public AImpl(String aid) {
                Aid = aid;
            }

            public string Aid { get; set; }
        }
    
        public class BImpl : ABC {
            public BImpl() {
            }
    
            public BImpl(String bIdOne, String bIdTwo) {
                BIdOne = bIdOne;
                BIdTwo = bIdTwo;
            }

            public string BIdOne { get; set; }

            public string BIdTwo { get; set; }
        }
    
        public class NoCtor {
            public NoCtor(String dummy) {
            }
        }
    
        public class ExceptionCtor {
            public ExceptionCtor() {
                throw new Exception("Test exception");
            }
        }
    }
}
