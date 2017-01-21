///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;

using NUnit.Framework;

using SupportEventAdapterService = com.espertech.esper.support.events.SupportEventAdapterService;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
	public class TestSelectExprEvalProcessor 
	{
	    private SelectExprProcessorHelper _methodOne;
	    private SelectExprProcessorHelper _methodTwo;

        [SetUp]
	    public void SetUp()
	    {
	        var selectList = SupportSelectExprFactory.MakeNoAggregateSelectList();
	        var eventAdapterService = SupportEventAdapterService.Service;
	        var vaeService = new SupportValueAddEventService();
	        var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry("abc", new StatementEventTypeRefImpl());
            var engineImportService = SupportEngineImportServiceFactory.Make();

            _methodOne = new SelectExprProcessorHelper(
                Collections.GetEmptyList<int>(), selectList, Collections.GetEmptyList<SelectExprStreamDesc>(), null,
                null, false, new SupportStreamTypeSvc1Stream(), eventAdapterService, vaeService,
                selectExprEventTypeRegistry, engineImportService, 1, null, new Configuration(), null,
                new TableServiceImpl(), null);

	        var insertIntoDesc = new InsertIntoDesc(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, "Hello");
	        insertIntoDesc.Add("a");
	        insertIntoDesc.Add("b");

            _methodTwo = new SelectExprProcessorHelper(
                Collections.GetEmptyList<int>(), selectList, Collections.GetEmptyList<SelectExprStreamDesc>(),
                insertIntoDesc, null, false, new SupportStreamTypeSvc1Stream(), eventAdapterService, vaeService,
                selectExprEventTypeRegistry, engineImportService, 1, null, new Configuration(), null,
                new TableServiceImpl(), null);
	    }

        [Test]
	    public void TestGetResultEventType()
	    {
	        var type = _methodOne.Evaluator.ResultEventType;
	        EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new string[]{"resultOne", "resultTwo"});
	        Assert.AreEqual(typeof(double?), type.GetPropertyType("resultOne"));
	        Assert.AreEqual(typeof(int?), type.GetPropertyType("resultTwo"));

	        type = _methodTwo.Evaluator.ResultEventType;
	        EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new string[]{"a", "b"});
	        Assert.AreEqual(typeof(double?), type.GetPropertyType("a"));
	        Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
	    }

        [Test]
	    public void TestProcess()
	    {
	        var events = new EventBean[] {MakeEvent(8.8, 3, 4)};

	        var result = _methodOne.Evaluator.Process(events, true, false, null);
	        Assert.AreEqual(8.8d, result.Get("resultOne"));
	        Assert.AreEqual(12, result.Get("resultTwo"));

	        result = _methodTwo.Evaluator.Process(events, true, false, null);
	        Assert.AreEqual(8.8d, result.Get("a"));
	        Assert.AreEqual(12, result.Get("b"));
	        Assert.AreSame(result.EventType, _methodTwo.Evaluator.ResultEventType);
	    }

	    private EventBean MakeEvent(double doubleBoxed, int intPrimitive, int intBoxed)
	    {
	        var bean = new SupportBean();
	        bean.DoubleBoxed = doubleBoxed;
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        return SupportEventBeanFactory.CreateObject(bean);
	    }
	}
} // end of namespace
