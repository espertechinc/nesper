///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestVariables 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private SupportUpdateListener _listenerSet;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ViewResources.IsIterableUnbound = true;
	        config.AddVariable("MYCONST_THREE", "boolean", true, true);
            config.EngineDefaults.Execution.IsAllowIsolatedService = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	        _listenerSet = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	        _listenerSet = null;
	    }

        [Test]
        public void TestDotVariableSeparateThread()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddVariable<MySimpleVariableService>("mySimpleVariableService", null);
            _epService.EPRuntime.SetVariableValue("mySimpleVariableService", new MySimpleVariableService());

            EPStatement epStatement = _epService.EPAdministrator.CreateEPL("select mySimpleVariableService.DoSomething() as c0 from SupportBean");

            var latch = new CountDownLatch(1);
            var values = new List<String>();
            epStatement.Subscriber = new Action<IDictionary<string, object>>(
                @event =>
                {
                    var value = (String) @event.Get("c0");
                    values.Add(value);
                    latch.CountDown();
                });

            var executorService = Executors.NewSingleThreadExecutor();
            executorService.Submit(() => _epService.EPRuntime.SendEvent(new SupportBean()));
            latch.Await();
            executorService.Shutdown();

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("hello", values[0]);
        }

        [Test]
	    public void TestInvokeMethod()
        {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddImport(typeof(MySimpleVariableServiceFactory));
	        _epService.EPAdministrator.Configuration.AddImport(typeof(MySimpleVariableService));

	        // declared via EPL
	        _epService.EPAdministrator.CreateEPL("create constant variable MySimpleVariableService myService = MySimpleVariableServiceFactory.MakeService()");

	        // added via runtime config
	        _epService.EPAdministrator.Configuration.AddVariable("myRuntimeInitService", typeof(MySimpleVariableService), MySimpleVariableServiceFactory.MakeService());

	        // exercise
	        _epService.EPAdministrator.CreateEPL("select " +
	                "myService.DoSomething() as c0, " +
	                "myRuntimeInitService.DoSomething() as c1 " +
	                "from SupportBean").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[] {"hello", "hello"});
	    }

        [Test]
	    public void TestConstantVariable()
        {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.CreateEPL("create const variable int MYCONST = 10");

            TryOperator("MYCONST = IntBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

            TryOperator("MYCONST > IntBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("MYCONST >= IntBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("MYCONST < IntBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("MYCONST <= IntBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("IntBoxed < MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed <= MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed > MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("IntBoxed >= MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("IntBoxed in (MYCONST)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("IntBoxed between MYCONST and MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("MYCONST != IntBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });
            TryOperator("IntBoxed != MYCONST", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });

            TryOperator("IntBoxed not in (MYCONST)", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed not between MYCONST and MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });

            TryOperator("MYCONST is IntBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });
            TryOperator("IntBoxed is MYCONST", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

            TryOperator("MYCONST is not IntBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });
            TryOperator("IntBoxed is not MYCONST", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });

	        // try coercion
            TryOperator("MYCONST = ShortBoxed", new object[][] { new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { null, false } });
            TryOperator("ShortBoxed = MYCONST", new object[][] { new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { null, false } });

            TryOperator("MYCONST > ShortBoxed", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, false }, new object[] { (short)9, true }, new object[] { (short)8, true } });
            TryOperator("ShortBoxed < MYCONST", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, false }, new object[] { (short)9, true }, new object[] { (short)8, true } });

            TryOperator("ShortBoxed in (MYCONST)", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { (short)8, false } });

	        // test SODA
	        var epl = "create constant variable int MYCONST = 10";
	        _epService.EPAdministrator.DestroyAllStatements();
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        var stmt = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(epl, stmt.Text);

	        // test invalid
	        TryInvalidSet("on SupportBean set MYCONST = 10",
	                "Error starting statement: Variable by name 'MYCONST' is declared constant and may not be set [on SupportBean set MYCONST = 10]");
	        TryInvalidSet("select * from SupportBean output when true then set MYCONST=1",
	                "Error starting statement: Error in the output rate limiting clause: Variable by name 'MYCONST' is declared constant and may not be set [select * from SupportBean output when true then set MYCONST=1]");

	        // assure no update via API
	        TryInvalidSetConstant("MYCONST", 1);

	        // add constant variable via runtime API
	        _epService.EPAdministrator.Configuration.AddVariable("MYCONST_TWO", "string", null, true);
	        TryInvalidSetConstant("MYCONST_TWO", "dummy");
	        TryInvalidSetConstant("MYCONST_THREE", false);

	        // try ESPER-653
	        var stmtDate = _epService.EPAdministrator.CreateEPL("create constant variable System.DateTime START_TIME = com.espertech.esper.compat.DateTimeHelper.GetCurrentTime()");
	        var value = stmtDate.First().Get("START_TIME");
	        Assert.IsNotNull(value);

	        // test array constant
	        _epService.EPAdministrator.DestroyAllStatements();

	        _epService.EPAdministrator.CreateEPL("create constant variable string[] var_strings = {'E1', 'E2'}");
	        var stmtArrayVar = _epService.EPAdministrator.CreateEPL("select var_strings from SupportBean");
	        Assert.AreEqual(typeof(string[]), stmtArrayVar.EventType.GetPropertyType("var_strings"));
	        RunAssertionArrayVar("var_strings");
	        _epService.EPAdministrator.Configuration.AddVariable("varcoll", "String[]", new string[] {"E1", "E2"}, true);

            TryOperator("IntBoxed in (10, 8)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, true } });

            _epService.EPAdministrator.CreateEPL("create constant variable int [ ] var_ints = {8, 10}");
            TryOperator("IntBoxed in (var_ints)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, true } });

	        _epService.EPAdministrator.CreateEPL("create constant variable int[]  var_intstwo = {9}");
            TryOperator("IntBoxed in (var_ints, var_intstwo)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });

	        TryInvalid("create constant variable SupportBean[] var_beans",
	                "Error starting statement: Cannot create variable: Cannot create variable 'var_beans', type 'SupportBean' cannot be declared as an array type [create constant variable SupportBean[] var_beans]");

	        // test array of primitives
	        var stmtArrayOne = _epService.EPAdministrator.CreateEPL("create variable byte[] myBytesBoxed");
            var expectedType = new object[][] { new object[] { "myBytesBoxed", typeof(byte?[]) } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtArrayOne.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
	        var stmtArrayTwo = _epService.EPAdministrator.CreateEPL("create variable byte[primitive] myBytesPrimitive");
            expectedType = new object[][] { new object[] { "myBytesPrimitive", typeof(byte[]) } };
	         SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtArrayTwo.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);

	        // test enum constant
	        _epService.EPAdministrator.DestroyAllStatements();
	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));
	        _epService.EPAdministrator.CreateEPL("create constant variable SupportEnum var_enumone = SupportEnum.ENUM_VALUE_2");
            TryOperator("var_enumone = enumValue", new object[][] { new object[] { SupportEnum.ENUM_VALUE_3, false }, new object[] { SupportEnum.ENUM_VALUE_2, true }, new object[] { SupportEnum.ENUM_VALUE_1, false } });

	        _epService.EPAdministrator.CreateEPL("create constant variable SupportEnum[] var_enumarr = {SupportEnum.ENUM_VALUE_2, SupportEnum.ENUM_VALUE_1}");
            TryOperator("enumValue in (var_enumarr, var_enumone)", new object[][] { new object[] { SupportEnum.ENUM_VALUE_3, false }, new object[] { SupportEnum.ENUM_VALUE_2, true }, new object[] { SupportEnum.ENUM_VALUE_1, true } });

            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));
	        _epService.EPAdministrator.CreateEPL("create variable SupportEnum var_enumtwo = SupportEnum.ENUM_VALUE_2");
	        _epService.EPAdministrator.CreateEPL("on SupportBean set var_enumtwo = enumValue");

            _epService.EPAdministrator.Configuration.AddVariable("supportEnum", Name.Of<SupportEnum>(), SupportEnum.ENUM_VALUE_1);
            _epService.EPAdministrator.Configuration.AddVariable("enumWithOverride", Name.Of<MyEnumWithOverride>(), MyEnumWithOverride.LONG);
	    }

	    private void RunAssertionArrayVar(string varName)
        {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString in (" + varName + "))");
	        stmt.AddListener(_listener);
	        SendBeanAssert("E1", true);
	        SendBeanAssert("E2", true);
	        SendBeanAssert("E3", false);
	    }

	    private void SendBeanAssert(string theString, bool expected)
        {
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
	        Assert.AreEqual(expected, _listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestVariableEPRuntime()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int), -1);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(string), "abc");
	        var runtimeSPI = (EPRuntimeSPI) _epService.EPRuntime;
	        var types = runtimeSPI.VariableTypeAll;
	        Assert.AreEqual(3, types.Count);
	        Assert.AreEqual(typeof(int?), types.Get("var1"));
	        Assert.AreEqual(typeof(string), types.Get("var2"));
	        Assert.AreEqual(typeof(int?), runtimeSPI.GetVariableType("var1"));
	        Assert.AreEqual(typeof(string), runtimeSPI.GetVariableType("var2"));

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive, var2 = TheString";
	        _epService.EPAdministrator.CreateEPL(stmtTextSet);

	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {-1, "abc"});
	        SendSupportBean(null, 99);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {99, null});

	        _epService.EPRuntime.SetVariableValue("var2", "def");
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {99, "def"});

	        _epService.EPRuntime.SetVariableValue("var1", 123);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {123, "def"});

	        IDictionary<string, object> newValues = new Dictionary<string, object>();
	        newValues.Put("var1", 20);
            _epService.EPRuntime.SetVariableValue(newValues);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {20, "def"});

	        newValues.Put("var1", (byte) 21);
	        newValues.Put("var2", "test");
            _epService.EPRuntime.SetVariableValue(newValues);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {21, "test"});

	        newValues.Put("var1", null);
	        newValues.Put("var2", null);
            _epService.EPRuntime.SetVariableValue(newValues);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {null, null});

	        // try variable not found
	        try
	        {
	            _epService.EPRuntime.SetVariableValue("dummy", null);
	            Assert.Fail();
	        }
	        catch (VariableNotFoundException ex)
	        {
	            // expected
	            Assert.AreEqual("Variable by name 'dummy' has not been declared", ex.Message);
	        }

	        // try variable not found
	        try
	        {
	            newValues.Put("dummy2", 20);
	            _epService.EPRuntime.SetVariableValue(newValues);
	            Assert.Fail();
	        }
	        catch (VariableNotFoundException ex)
	        {
	            // expected
	            Assert.AreEqual("Variable by name 'dummy2' has not been declared", ex.Message);
	        }

	        // create new variable on the fly
	        _epService.EPAdministrator.CreateEPL("create variable int dummy = 20 + 20");
	        Assert.AreEqual(40, _epService.EPRuntime.GetVariableValue("dummy"));

	        // try type coercion
	        try
	        {
	            _epService.EPRuntime.SetVariableValue("dummy", "abc");
	            Assert.Fail();
	        }
	        catch (VariableValueException ex)
	        {
	            // expected
                Assert.AreEqual("Variable 'dummy' of declared type " + typeof(int?).FullName + " cannot be assigned a value of type " + typeof(string).FullName + "", ex.Message);
	        }
	        try
	        {
	            _epService.EPRuntime.SetVariableValue("dummy", 100L);
                Assert.Fail();
	        }
	        catch (VariableValueException ex)
	        {
	            // expected
                Assert.AreEqual("Variable 'dummy' of declared type " + typeof(int?).FullName + " cannot be assigned a value of type " + typeof(long).FullName + "", ex.Message);
	        }
	        try
	        {
	            _epService.EPRuntime.SetVariableValue("var2", 0);
                Assert.Fail();
	        }
	        catch (VariableValueException ex)
	        {
	            // expected
                Assert.AreEqual("Variable 'var2' of declared type " + typeof(string).FullName + " cannot be assigned a value of type " + typeof(int).FullName + "", ex.Message);
	        }

	        // coercion
	        _epService.EPRuntime.SetVariableValue("var1", (short) -1);
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {-1, null});

	        // rollback for coercion failed
	        newValues = new LinkedHashMap<string, object>();    // preserve order
	        newValues.Put("var2", "xyz");
	        newValues.Put("var1", 4.4d);
	        try
	        {
	            _epService.EPRuntime.SetVariableValue(newValues);
                Assert.Fail();
	        }
	        catch (VariableValueException)
	        {
	            // expected
	        }
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {-1, null});

	        // rollback for variable not found
	        newValues = new LinkedHashMap<string, object>();    // preserve order
	        newValues.Put("var2", "xyz");
	        newValues.Put("var1", 1);
	        newValues.Put("notfoundvariable", null);
	        try
	        {
	            _epService.EPRuntime.SetVariableValue(newValues);
                Assert.Fail();
	        }
	        catch (VariableNotFoundException)
	        {
	            // expected
	        }
	        AssertVariableValues(new string[] {"var1", "var2"}, new object[] {-1, null});
	    }

        [Test]
	    public void TestSetSubquery()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(string), "a");
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(string), "b");

	        var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " as s0str set var1 = (select p10 from S1#lastevent), var2 = (select p11||s0str.p01 from S1#lastevent)";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "a", "b" } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null, null } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "x", "y"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "1", "2"));
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "x", "y2" } });
	    }

        [Test]
	    public void TestVariableInFilterBoolean()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(string), null);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(string), null);

	        var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " set var1 = p00, var2 = p01";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null, null } });

	        var stmtTextSelect = "select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "(TheString = var1 or TheString = var2)";
	        var fieldsSelect = new string[] {"TheString", "IntPrimitive"};
	        var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
	        stmtSelect.AddListener(_listener);

	        SendSupportBean(null, 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBeanS0NewThread(100, "a", "b");
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"a", "b"});

	        SendSupportBean("a", 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"a", 2});

	        SendSupportBean(null, 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBean("b", 3);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"b", 3});

	        SendSupportBean("c", 4);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBeanS0NewThread(100, "e", "c");
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"e", "c"});

	        SendSupportBean("c", 5);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"c", 5});

	        SendSupportBean("e", 6);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"e", 6});

	        stmtSet.Dispose();
	    }

        [Test]
	    public void TestVariableInFilter()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(string), null);

	        var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " set var1 = p00";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null } });

	        var stmtTextSelect = "select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "(TheString = var1)";
	        var fieldsSelect = new string[] {"TheString", "IntPrimitive"};
	        var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
	        stmtSelect.AddListener(_listener);

	        SendSupportBean(null, 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBeanS0NewThread(100, "a", "b");
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"a"});

	        SendSupportBean("a", 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"a", 2});

	        SendSupportBean(null, 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBeanS0NewThread(100, "e", "c");
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"e"});

	        SendSupportBean("c", 5);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendSupportBean("e", 6);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"e", 6});

            stmtSet.Dispose();
	    }

        [Test]
	    public void TestAssignmentOrderNoDup()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int?), "12");
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(int?), "2");
	        _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(int?), null);

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive, var2 = var1 + 1, var3 = var1 + var2";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2", "var3"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 12, 2, null } });

	        SendSupportBean("S1", 3);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3, 4, 7});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 3, 4, 7 } });

	        SendSupportBean("S1", -1);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1, 0, -1});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { -1, 0, -1 } });

	        SendSupportBean("S1", 90);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{90, 91, 181});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 90, 91, 181 } });

            stmtSet.Dispose();
	    }

        [Test]
	    public void TestAssignmentOrderDup()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int?), 0);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(int?), 1);
	        _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(int?), 2);

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive, var2 = var2, var1 = IntBoxed, var3 = var3 + 1";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2", "var3"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 0, 1, 2 } });

	        SendSupportBean("S1", -1, 10);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{10, 1, 3});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 10, 1, 3 } });

	        SendSupportBean("S2", -2, 20);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{20, 1, 4});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 20, 1, 4 } });

	        SendSupportBeanNewThread("S3", -3, 30);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{30, 1, 5});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 30, 1, 5 } });

	        SendSupportBeanNewThread("S4", -4, 40);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{40, 1, 6});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 40, 1, 6 } });

            stmtSet.Dispose();
	    }

        [Test]
	    public void TestObjectModel()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(double), 10d);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(long?), 11L);

	        var model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create("var1", "var2", "id");
	        model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_A).FullName));

	        var stmtSelect = _epService.EPAdministrator.Create(model);
	        var stmtText = "select var1, var2, id from " + typeof(SupportBean_A).FullName;
	        Assert.AreEqual(stmtText, model.ToEPL());
	        stmtSelect.AddListener(_listener);

	        var fieldsSelect = new string[] {"var1", "var2", "id"};
	        SendSupportBean_A("E1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10d, 11L, "E1"});

	        model = new EPStatementObjectModel();
	        model.OnExpr = OnClause.CreateOnSet(Expressions.Eq(Expressions.Property("var1"), Expressions.Property("IntPrimitive"))).AddAssignment(Expressions.Eq(Expressions.Property("var2"), Expressions.Property("IntBoxed")));
	        model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1=IntPrimitive, var2=IntBoxed";
	        var stmtSet = _epService.EPAdministrator.Create(model);
	        stmtSet.AddListener(_listenerSet);
	        Assert.AreEqual(stmtTextSet, model.ToEPL());

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1"));
	        Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        var fieldsVar = new string[] {"var1", "var2"};
	        EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{10d, 11L}});
	        SendSupportBean("S1", 3, 4);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3d, 4L});
	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{3d, 4L}});

	        SendSupportBean_A("E2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3d, 4L, "E2"});

	        stmtSet.Dispose();
	        stmtSelect.Dispose();
	    }

        [Test]
	    public void TestCompile()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(double), 10d);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(long?), 11L);

	        var stmtText = "select var1, var2, id from " + typeof(SupportBean_A).FullName;
	        var model = _epService.EPAdministrator.CompileEPL(stmtText);
	        var stmtSelect = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(stmtText, model.ToEPL());
	        stmtSelect.AddListener(_listener);

	        var fieldsSelect = new string[] {"var1", "var2", "id"};
	        SendSupportBean_A("E1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10d, 11L, "E1"});

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1=IntPrimitive, var2=IntBoxed";
	        model = _epService.EPAdministrator.CompileEPL(stmtTextSet);
	        var stmtSet = _epService.EPAdministrator.Create(model);
	        stmtSet.AddListener(_listenerSet);
	        Assert.AreEqual(stmtTextSet, model.ToEPL());

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1"));
	        Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        var fieldsVar = new string[] {"var1", "var2"};
	        EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);

	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{10d, 11L}});
	        SendSupportBean("S1", 3, 4);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3d, 4L});
	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{3d, 4L}});

	        SendSupportBean_A("E2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3d, 4L, "E2"});

	        stmtSet.Dispose();
	        stmtSelect.Dispose();

	        // test prepared statement
	        _epService.EPAdministrator.Configuration.AddVariable("var_a", typeof(A), new A());
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(B));
	        var prepared = _epService.EPAdministrator.PrepareEPL("select var_a.value from B");
	        var statement = _epService.EPAdministrator.Create(prepared);
	        statement.Subscriber = new Action<string>(value => { });
	        _epService.EPRuntime.SendEvent(new B());
	    }

        [Test]
	    public void TestRuntimeConfig()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int?), 10);

	        var stmtText = "select var1, TheString from " + typeof(SupportBean).FullName + "(TheString like 'E%')";
	        var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmtSelect.AddListener(_listener);

	        var fieldsSelect = new string[] {"var1", "TheString"};
	        SendSupportBean("E1", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10, "E1"});

	        SendSupportBean("E2", 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10, "E2"});

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%') set var1 = IntPrimitive";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, new string[] {"var1"}));

	        var fieldsVar = new string[] {"var1"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 10 } });

	        SendSupportBean("S1", 3);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 3 } });

	        SendSupportBean("E3", 4);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3, "E3"});

	        SendSupportBean("S2", -1);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { -1 } });

	        SendSupportBean("E4", 5);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{-1, "E4"});

	        try
	        {
	            _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int?), 10);
	        }
	        catch (ConfigurationException ex)
	        {
	            Assert.AreEqual("Error creating variable: Variable by name 'var1' has already been created", ex.Message);
	        }

            stmtSet.Dispose();
            stmtSelect.Dispose();
	    }

        [Test]
	    public void TestRuntimeOrderMultiple()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int?), null);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(int?), 1);

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%' or TheString like 'B%') set var1 = IntPrimitive, var2 = IntBoxed";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null, 1 } });

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1"));
	        Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var2"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"var1", "var2"}, typeSet.PropertyNames);

	        SendSupportBean("S1", 3, null);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 3, null } });

	        SendSupportBean("S1", -1, -2);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1, -2});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { -1, -2 } });

	        var stmtText = "select var1, var2, TheString from " + typeof(SupportBean).FullName + "(TheString like 'E%' or TheString like 'B%')";
	        var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmtSelect.AddListener(_listener);
	        var fieldsSelect = new string[] {"var1", "var2", "TheString"};
	        EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, null);

	        SendSupportBean("E1", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{-1, -2, "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { -1, -2 } });
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][] { new object[] { -1, -2, "E1" } });

	        SendSupportBean("S1", 11, 12);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{11, 12});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { 11, 12 } });
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][] { new object[] { 11, 12, "E1" } });

	        SendSupportBean("E2", 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{11, 12, "E2"});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][] { new object[] { 11, 12, "E2" } });

            stmtSelect.Dispose();
            stmtSet.Dispose();
	    }

        [Test]
	    public void TestEngineConfigAPI()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddVariable("p_1", "begin");
	        config.AddVariable("p_2", true);
	        config.AddVariable("p_3", "value");

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%') set p_1 = 'end', p_2 = false, p_3 = null";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"p_1", "p_2", "p_3"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "begin", true, "value" } });

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(string), typeSet.GetPropertyType("p_1"));
	        Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("p_2"));
	        Assert.AreEqual(typeof(string), typeSet.GetPropertyType("p_3"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        Collections.SortInPlace(typeSet.PropertyNames);
	        Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, fieldsVar));

	        SendSupportBean("S1", 3);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"end", false, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "end", false, null } });

	        SendSupportBean("S2", 4);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"end", false, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "end", false, null } });

            stmtSet.Dispose();
	    }

        [Test]
	    public void TestEngineConfigXML()
	    {
	        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                "<esper-configuration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"../esper-configuration-2-0.xsd\">" +
	                "<variable name=\"p_1\" type=\"string\" />" +
	                "<variable name=\"p_2\" type=\"bool\" initialization-value=\"true\"/>" +
	                "<variable name=\"p_3\" type=\"long\" initialization-value=\"10\"/>" +
	                "<variable name=\"p_4\" type=\"double\" initialization-value=\"11.1d\"/>" +
	                "</esper-configuration>";

            var configDoc = new XmlDocument();
            configDoc.LoadXml(xml);

	        var config = SupportConfigFactory.GetConfiguration();
	        config.Configure(configDoc);
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set p_1 = TheString, p_2 = BoolBoxed, p_3 = IntBoxed, p_4 = IntBoxed";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"p_1", "p_2", "p_3", "p_4"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null, true, 10L, 11.1d } });

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(string), typeSet.GetPropertyType("p_1"));
	        Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("p_2"));
	        Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("p_3"));
	        Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("p_4"));
	        Collections.SortInPlace(typeSet.PropertyNames);
	        Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, fieldsVar));

	        var bean = new SupportBean();
	        bean.TheString = "text";
	        bean.BoolBoxed = false;
	        bean.IntBoxed = 200;
	        _epService.EPRuntime.SendEvent(bean);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"text", false, 200L, 200d});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { "text", false, 200L, 200d } });

	        bean = new SupportBean();   // leave all fields null
	        _epService.EPRuntime.SendEvent(bean);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{null, null, null, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][] { new object[] { null, null, null, null } });

            stmtSet.Dispose();
	    }

        [Test]
	    public void TestCoercion()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(float?), null);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(double?), null);
	        _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(long?), null);

	        var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive, var2 = IntPrimitive, var3=IntBoxed";
	        var stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
	        stmtSet.AddListener(_listenerSet);
	        var fieldsVar = new string[] {"var1", "var2", "var3"};
	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{null, null, null}});

	        var stmtText = "select irstream var1, var2, var3, id from " + typeof(SupportBean_A).FullName + "#length(2)";
	        var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmtSelect.AddListener(_listener);
	        var fieldsSelect = new string[] {"var1", "var2", "var3", "id"};
	        EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, null);

	        var typeSet = stmtSet.EventType;
	        Assert.AreEqual(typeof(float?), typeSet.GetPropertyType("var1"));
	        Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var2"));
	        Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var3"));
	        Assert.AreEqual(typeof(IDictionary<string, object>), typeSet.UnderlyingType);
	        EPAssertionUtil.AssertEqualsAnyOrder(typeSet.PropertyNames, fieldsVar);

	        SendSupportBean_A("A1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{null, null, null, "A1"});
	        EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][]{new object[]{null, null, null, "A1"}});

	        SendSupportBean("S1", 1, 2);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{1f, 1d, 2L});
	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{1f, 1d, 2L}});

	        SendSupportBean_A("A2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{1f, 1d, 2L, "A2"});
	        EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][]{new object[]{1f, 1d, 2L, "A1"}, new object[]{1f, 1d, 2L, "A2"}});

	        SendSupportBean("S1", 10, 20);
	        EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{10f, 10d, 20L});
	        EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new object[][]{new object[]{10f, 10d, 20L}});

	        SendSupportBean_A("A3");
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fieldsSelect, new object[]{10f, 10d, 20L, "A3"});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fieldsSelect, new object[]{10f, 10d, 20L, "A1"});
	        EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new object[][]{new object[]{10f, 10d, 20L, "A2"}, new object[]{10f, 10d, 20L, "A3"}});

	        stmtSelect.Dispose();
	        stmtSet.Dispose();
	    }

        [Test]
	    public void TestInvalidSet()
	    {
	        _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(string), null);
	        _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(bool?), false);
	        _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(int), 1);

	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set dummy = 100",
	                      "Error starting statement: Variable by name 'dummy' has not been created or configured [on " + Name.Of<SupportBean>() + " set dummy = 100]");

	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var1 = 1",
                          "Error starting statement: Variable 'var1' of declared type System.String cannot be assigned a value of type " + Name.Of<int>() + " [on " + Name.Of<SupportBean>() + " set var1 = 1]");

	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var3 = 'abc'",
                          "Error starting statement: Variable 'var3' of declared type " + Name.Of<int>() + " cannot be assigned a value of type " + Name.Of<string>() + " [on " + Name.Of<SupportBean>() + " set var3 = 'abc']");

	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var3 = DoublePrimitive",
                          "Error starting statement: Variable 'var3' of declared type " + Name.Of<int>() + " cannot be assigned a value of type " + Name.Of<double>(false) + " [on " + Name.Of<SupportBean>() + " set var3 = DoublePrimitive]");

	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var2 = 'false'", null);
	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var3 = 1.1", null);
	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var3 = 22222222222222", null);
	        TryInvalidSet("on " + typeof(SupportBean).FullName + " set var3", "Error starting statement: Missing variable assignment expression in assignment number 0 [on " + Name.Of<SupportBean>() + " set var3]");
	    }

	    private void TryInvalidSet(string stmtText, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(stmtText);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            if (message != null)
	            {
	                Assert.AreEqual(message, ex.Message);
	            }
	        }
	    }

        [Test]
	    public void TestInvalidInitialization()
	    {
	        TryInvalid(typeof(int?), "abcdef",
                    "Error creating variable: Variable 'var1' of declared type " + typeof(int?).FullName + " cannot be initialized by value 'abcdef': System.FormatException: Input string was not in a correct format.");

	        TryInvalid(typeof(int?), 11.1D,
                    "Error creating variable: Variable 'var1' of declared type " + typeof(int?).FullName + " cannot be initialized by a value of type " + typeof(double).FullName + "");

	        TryInvalid(typeof(int), 11.1D, null);
	        TryInvalid(typeof(string), true, null);
	    }

	    private void TryInvalid(Type type, object value, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.Configuration.AddVariable("var1", type, value);
                Assert.Fail();
	        }
	        catch (ConfigurationException ex)
	        {
	            if (message != null)
	            {
	                Assert.AreEqual(message, ex.Message);
	            }
	        }
	    }

	    private void TryInvalid(string epl, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private SupportBean_A SendSupportBean_A(string id)
	    {
	        var bean = new SupportBean_A(id);
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private SupportBean SendSupportBean(string theString, int intPrimitive)
	    {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private SupportBean SendSupportBean(string theString, int intPrimitive, int? intBoxed)
	    {
	        var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private void SendSupportBeanNewThread(string theString, int intPrimitive, int? intBoxed)
	    {
	        var t = new Thread(() =>
            {
	            var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
	            _epService.EPRuntime.SendEvent(bean);
	        });
	        t.Start();
	        t.Join();
	    }

	    private void SendSupportBeanS0NewThread(int id, string p00, string p01)
	    {
	        var t = new Thread(() => _epService.EPRuntime.SendEvent(new SupportBean_S0(id, p00, p01)));
	        t.Start();
	        t.Join();
	    }

	    private SupportBean MakeSupportBean(string theString, int intPrimitive, int? intBoxed)
	    {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        return bean;
	    }

	    private void AssertVariableValues(string[] names, object[] values)
	    {
	        Assert.AreEqual(names.Length, values.Length);

	        // assert one-by-one
	        for (var i = 0; i < names.Length; i++)
	        {
	            Assert.AreEqual(values[i], _epService.EPRuntime.GetVariableValue(names[i]));
	        }

	        // get and assert all
	        var all = _epService.EPRuntime.VariableValueAll;
	        for (var i = 0; i < names.Length; i++)
	        {
	            Assert.AreEqual(values[i], all.Get(names[i]));
	        }

	        // get by request
	        ISet<string> nameSet = new HashSet<string>();
	        nameSet.AddAll(names);
	        var valueSet = _epService.EPRuntime.GetVariableValue(nameSet);
	        for (var i = 0; i < names.Length; i++)
	        {
	            Assert.AreEqual(values[i], valueSet.Get(names[i]));
	        }
	    }

        [Serializable]
	    public class A {
	        public string GetValue() {
	            return "";
	        }
	    }

	    public class B {
	    }

	    private void TryInvalidSetConstant(string variableName, object newValue) {
	        try {
	            _epService.EPRuntime.SetVariableValue(variableName, newValue);
	            Assert.Fail();
	        }
	        catch (VariableConstantValueException ex) {
	            Assert.AreEqual(ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
	        }
	        try {
	            _epService.EPRuntime.SetVariableValue(Collections.SingletonDataMap(variableName, newValue));
	            Assert.Fail();
	        }
	        catch (VariableConstantValueException ex) {
	            Assert.AreEqual(ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
	        }
	    }

	    private void TryOperator(string @operator, object[][] testdata)
        {
	        var spi = (EPServiceProviderSPI) _epService;
	        var filterSpi = (FilterServiceSPI) spi.FilterService;

	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select TheString as c0,IntPrimitive as c1 from SupportBean(" + @operator + ")");
	        stmt.AddListener(_listener);

	        // initiate
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S01"));

	        for (var i = 0; i < testdata.Length; i++) {
	            var bean = new SupportBean();
	            var testValue = testdata[i][0];
	            if (testValue is int) {
	                bean.IntBoxed = (int?) testValue;
	            }
	            else if (testValue is SupportEnum) {
	                bean.EnumValue = (SupportEnum) testValue;
	            }
	            else {
	                bean.ShortBoxed = (short?) testValue;
	            }
	            var expected = testdata[i][1].AsBoolean();

	            _epService.EPRuntime.SendEvent(bean);
                Assert.AreEqual(expected, _listener.GetAndClearIsInvoked(), "Failed at " + i);
	        }

	        // assert type of expression
	        if (filterSpi.IsSupportsTakeApply)
	        {
	            var set = filterSpi.Take(Collections.SingletonList(stmt.StatementId));
	            Assert.AreEqual(1, set.Filters.Count);
	            var valueSet = set.Filters[0].FilterValueSet;
	            Assert.AreEqual(1, valueSet.Parameters.Length);
	            var para = valueSet.Parameters[0][0];
	            Assert.IsTrue(para.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION);
	        }

	        stmt.Dispose();
	    }

	    public class MySimpleVariableServiceFactory
        {
	        public static MySimpleVariableService MakeService() {
	            return new MySimpleVariableService();
	        }
	    }

	    public class MySimpleVariableService
        {
	        public string DoSomething() {
	            return "hello";
	        }
	    }

        public enum MyEnumWithOverride
        {
            LONG = 1,
            SHORT = -1
        }
	}
} // end of namespace
