///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestNewStructExpr 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNewAlone() {
	        string epl = "select new { theString = 'x' || theString || 'x', intPrimitive = intPrimitive + 2} as val0 from SupportBean as sb";

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("val0"));
	        var fragType = stmt.EventType.GetFragmentType("val0");
	        Assert.IsFalse(fragType.IsIndexed);
	        Assert.IsFalse(fragType.IsNative);
	        Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("theString"));
	        Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("intPrimitive"));

	        string[] fieldsInner = "theString,intPrimitive".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", -5));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[] { "xE1x", -3 });
	    }

        [Test]
	    public void TestDefaultColumnsAndSODA()
	    {
	        string epl = "select " +
	                "case theString" +
	                " when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\"}" +
	                " when \"B\" then new{theString,intPrimitive=10,col2=theString||\"B\"} " +
	                "end as val0 from SupportBean as sb";

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        RunAssertionDefault(stmt);

	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        RunAssertionDefault(stmt);

	        // test to-expression string
	        epl = "select " +
	                "case theString" +
	                " when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\" }" +
	                " when \"B\" then new{theString,intPrimitive = 10,col2=theString||\"B\" } " +
	                "end from SupportBean as sb";

	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        Assert.AreEqual("case theString when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\"} when \"B\" then new{theString,intPrimitive=10,col2=theString||\"B\"} end", stmt.EventType.PropertyNames[0]);
	    }

	    private void RunAssertionDefault(EPStatement stmt)
        {
            Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("val0"));
	        FragmentEventType fragType = stmt.EventType.GetFragmentType("val0");
	        Assert.IsFalse(fragType.IsIndexed);
	        Assert.IsFalse(fragType.IsNative);
	        Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("theString"));
	        Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("intPrimitive"));
	        Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col2"));

	        string[] fieldsInner = "theString,intPrimitive,col2".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"Q", 2, "AA"});

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 3));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"B", 10, "BB"});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestNewWithCase()
	    {
	        string epl = "select " +
	                "case " +
	                "  when theString = 'A' then new { col1 = 'X', col2 = 10 } " +
	                "  when theString = 'B' then new { col1 = 'Y', col2 = 20 } " +
	                "  when theString = 'C' then new { col1 = null, col2 = null } " +
	                "  else new { col1 = 'Z', col2 = 30 } " +
	                "end as val0 from SupportBean sb";
	        RunAssertion(epl);

	        epl = "select " +
	                "case theString " +
	                "  when 'A' then new { col1 = 'X', col2 = 10 } " +
	                "  when 'B' then new { col1 = 'Y', col2 = 20 } " +
	                "  when 'C' then new { col1 = null, col2 = null } " +
	                "  else new{ col1 = 'Z', col2 = 30 } " +
	                "end as val0 from SupportBean sb";
	        RunAssertion(epl);
	    }

	    private void RunAssertion(string epl)
	    {
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("val0"));
	        FragmentEventType fragType = stmt.EventType.GetFragmentType("val0");
	        Assert.IsFalse(fragType.IsIndexed);
	        Assert.IsFalse(fragType.IsNative);
	        Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col1"));
	        Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("col2"));

	        string[] fieldsInner = "col1,col2".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"Z", 30});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"X", 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 3));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"Y", 20});

	        _epService.EPRuntime.SendEvent(new SupportBean("C", 4));
	        EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{null, null});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestInvalid() {
	        string epl;

	        epl = "select case when true then new { col1 = 'a' } else 1 end from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(44 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check the else-condition [select case when true then new { col1 = 'a' } else 1 end from SupportBean]");

	        epl = "select case when true then new { col1 = 'a' } when false then 1 end from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'case when true then new{col1=\"a\"} w...(55 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check when-condition number 1 [select case when true then new { col1 = 'a' } when false then 1 end from SupportBean]");

	        epl = "select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(54 chars)': Incompatible case-when return types by new-operator in case-when number 1: Type by name 'Case-when number 1' in property 'col1' expected System.String but receives " + typeof(int?) + " [select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean]");

	        epl = "select case when true then new { col1 = 'a' } else new { col2 = 'a' } end from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(56 chars)': Incompatible case-when return types by new-operator in case-when number 1: The property 'col1' is not provided but required [select case when true then new { col1 = 'a' } else new { col2 = 'a' } end from SupportBean]");

	        epl = "select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'case when true then new{col1=\"a\",co...(46 chars)': Failed to validate new-keyword property names, property 'col1' has already been declared [select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean]");
	    }

	    private void TryInvalid(string epl, string message) {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
