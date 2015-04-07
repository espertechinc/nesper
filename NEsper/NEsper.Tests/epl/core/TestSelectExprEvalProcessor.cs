///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
	public class TestSelectExprEvalProcessor 
	{
	    private SelectExprProcessorHelper methodOne;
	    private SelectExprProcessorHelper methodTwo;

        [SetUp]
	    public void SetUp()
	    {
	        IList<SelectClauseExprCompiledSpec> selectList = SupportSelectExprFactory.MakeNoAggregateSelectList();
	        EventAdapterService eventAdapterService = SupportEventAdapterService.Service;
	        SupportValueAddEventService vaeService = new SupportValueAddEventService();
	        SelectExprEventTypeRegistry selectExprEventTypeRegistry = new SelectExprEventTypeRegistry("abc", new StatementEventTypeRefImpl());
	        MethodResolutionService methodResolutionService = new MethodResolutionServiceImpl(new EngineImportServiceImpl(true, true, true, false, null), null);

	        methodOne = new SelectExprProcessorHelper(Collections.GetEmptyList<int>(), selectList, Collections.GetEmptyList<SelectExprStreamDesc>(), null, null, false, new SupportStreamTypeSvc1Stream(), eventAdapterService, vaeService, selectExprEventTypeRegistry, methodResolutionService, null, null, new Configuration(), null, new TableServiceImpl());

	        InsertIntoDesc insertIntoDesc = new InsertIntoDesc(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, "Hello");
	        insertIntoDesc.Add("a");
	        insertIntoDesc.Add("b");

	        methodTwo = new SelectExprProcessorHelper(Collections.GetEmptyList<int>(), selectList, Collections.GetEmptyList<SelectExprStreamDesc>(), insertIntoDesc, null, false, new SupportStreamTypeSvc1Stream(), eventAdapterService, vaeService, selectExprEventTypeRegistry, methodResolutionService, null, null, new Configuration(), null, new TableServiceImpl());
	    }

        [Test]
	    public void TestGetResultEventType()
	    {
	        EventType type = methodOne.Evaluator.ResultEventType;
	        EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new string[]{"resultOne", "resultTwo"});
	        Assert.AreEqual(typeof(double?), type.GetPropertyType("resultOne"));
	        Assert.AreEqual(typeof(int?), type.GetPropertyType("resultTwo"));

	        type = methodTwo.Evaluator.ResultEventType;
	        EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new string[]{"a", "b"});
	        Assert.AreEqual(typeof(double?), type.GetPropertyType("a"));
	        Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
	    }

        [Test]
	    public void TestProcess()
	    {
	        EventBean[] events = new EventBean[] {MakeEvent(8.8, 3, 4)};

	        EventBean result = methodOne.Evaluator.Process(events, true, false, null);
	        Assert.AreEqual(8.8d, result.Get("resultOne"));
	        Assert.AreEqual(12, result.Get("resultTwo"));

	        result = methodTwo.Evaluator.Process(events, true, false, null);
	        Assert.AreEqual(8.8d, result.Get("a"));
	        Assert.AreEqual(12, result.Get("b"));
	        Assert.AreSame(result.EventType, methodTwo.Evaluator.ResultEventType);
	    }

	    private EventBean MakeEvent(double doubleBoxed, int intPrimitive, int intBoxed)
	    {
	        SupportBean bean = new SupportBean();
	        bean.DoubleBoxed = doubleBoxed;
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        return SupportEventBeanFactory.CreateObject(bean);
	    }
	}
} // end of namespace
