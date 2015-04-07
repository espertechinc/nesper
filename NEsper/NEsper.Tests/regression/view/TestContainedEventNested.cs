///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.bookexample;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestContainedEventNested 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNamedWindowFilter()
	    {
	        string[] fields = "reviewId".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        _epService.EPAdministrator.CreateEPL("create window OrderWindow.std:lastevent() as OrderEvent");
	        _epService.EPAdministrator.CreateEPL("insert into OrderWindow select * from OrderEvent");

	        string stmtText = "select reviewId from OrderWindow[books][reviews] bookReviews order by reviewId asc";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 1 }, new object[] { 2 }, new object[] { 10 } });
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 201 } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestNamedWindowSubquery()
	    {
	        string[] fields = "theString,totalPrice".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

	        _epService.EPAdministrator.CreateEPL("create window OrderWindow.std:lastevent() as OrderEvent");
	        _epService.EPAdministrator.CreateEPL("insert into OrderWindow select * from OrderEvent");

	        string stmtText = "select *, (select sum(price) from OrderWindow[books]) as totalPrice from SupportBean";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { "E1", 24d + 35d + 27d } });
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { "E2", 15d + 13d } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestNamedWindowOnTrigger()
	    {
	        string[] fields = "theString,intPrimitive".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

	        _epService.EPAdministrator.CreateEPL("create window SupportBeanWindow.std:lastevent() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into SupportBeanWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("create window OrderWindow.std:lastevent() as OrderEvent");
	        _epService.EPAdministrator.CreateEPL("insert into OrderWindow select * from OrderEvent");

	        string stmtText = "on OrderWindow[books] owb select sbw.* from SupportBeanWindow sbw where theString = title";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("Foundation 2", 2));
	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { "Foundation 2", 2 } });
	    }

        [Test]
	    public void TestSimple()
	    {
	        string[] fields = "reviewId".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        string stmtText = "select reviewId from OrderEvent[books][reviews] bookReviews order by reviewId asc";
	        EPStatementSPI stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsTrue(stmt.StatementContext.IsStatelessSelect);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 1 }, new object[] { 2 }, new object[] { 10 } });
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 201 } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestWhere()
	    {
	        string[] fields = "reviewId".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        // try where in root
	        string stmtText = "select reviewId from OrderEvent[books where title = 'Enders Game'][reviews] bookReviews order by reviewId asc";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 1 }, new object[] { 2 } });
	        _listener.Reset();

	        // try where in different levels
	        stmt.Dispose();
	        stmtText = "select reviewId from OrderEvent[books where title = 'Enders Game'][reviews where reviewId in (1, 10)] bookReviews order by reviewId asc";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 1 } });
	        _listener.Reset();

	        // try where in combination
	        stmt.Dispose();
	        stmtText = "select reviewId from OrderEvent[books as bc][reviews as rw where rw.reviewId in (1, 10) and bc.title = 'Enders Game'] bookReviews order by reviewId asc";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { 1 } });
	        _listener.Reset();
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestColumnSelect()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        // columns supplied
	        string stmtText = "select * from OrderEvent[select bookId, orderdetail.orderId as orderId from books][select reviewId from reviews] bookReviews order by reviewId asc";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // stream wildcards identify fragments
	        stmtText = "select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
	          "from OrderEvent[books as book][select myorder.* as orderFrag, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // one event type dedicated as underlying
	        stmtText = "select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
	          "from OrderEvent[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // wildcard unnamed as underlying
	        stmtText = "select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
	          "from OrderEvent[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // wildcard named as underlying
	        stmtText = "select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
	          "from OrderEvent[select * from books as bookFrag][select myorder.* as orderFrag, review.* as reviewFrag from reviews as review] as myorder";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // object model
	        stmtText = "select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
	          "from OrderEvent[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
	        Assert.AreEqual(stmtText, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model, stmtText);
	        stmt.AddListener(_listener);
	        RunAssertion();
	        stmt.Dispose();

	        // with where-clause
	        stmtText = "select * from AccountEvent[select * from wallets where currency=\"USD\"]";
	        model = _epService.EPAdministrator.CompileEPL(stmtText);
	        Assert.AreEqual(stmtText, model.ToEPL());
	    }

        [Test]
	    public void TestPatternSelect()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from pattern [" +
	                "every r=OrderEvent[books][reviews] -> SupportBean(intPrimitive = r[0].reviewId)]");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestSubSelect()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("select theString from SupportBean s0 where " +
	                "exists (select * from OrderEvent[books][reviews].std:unique(reviewId) where reviewId = s0.intPrimitive)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestUnderlyingSelect()
	    {
	        string[] fields = "orderId,bookId,reviewId".Split(',');
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        string stmtText = "select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
	        //String stmtText = "select * " +
	          "from OrderEvent[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{
	                new object[] {"PO200901", "10020", 1}, new object[] {"PO200901", "10020", 2}, new object[] {"PO200901", "10021", 10}});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { "PO200904", "10031", 201 } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestInvalid()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));

	        TryInvalid("select bookId from OrderEvent[select count(*) from books]",
	                   "Expression in a property-selection may not utilize an aggregation function [select bookId from OrderEvent[select count(*) from books]]");

	        TryInvalid("select bookId from OrderEvent[select bookId, (select abc from review.std:lastevent()) from books]",
	                   "Expression in a property-selection may not utilize a subselect [select bookId from OrderEvent[select bookId, (select abc from review.std:lastevent()) from books]]");

	        TryInvalid("select bookId from OrderEvent[select prev(1, bookId) from books]",
	                   "Failed to validate contained-event expression 'prev(1,bookId)': Previous function cannot be used in this context [select bookId from OrderEvent[select prev(1, bookId) from books]]");

	        TryInvalid("select bookId from OrderEvent[select * from books][select * from reviews]",
	                   "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [select bookId from OrderEvent[select * from books][select * from reviews]]");

	        TryInvalid("select bookId from OrderEvent[select abc from books][reviews]",
	                   "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select bookId from OrderEvent[select abc from books][reviews]]");

	        TryInvalid("select bookId from OrderEvent[books][reviews]",
	                   "Error starting statement: Failed to validate select-clause expression 'bookId': Property named 'bookId' is not valid in any stream [select bookId from OrderEvent[books][reviews]]");

	        TryInvalid("select orderId from OrderEvent[books]",
	                   "Error starting statement: Failed to validate select-clause expression 'orderId': Property named 'orderId' is not valid in any stream [select orderId from OrderEvent[books]]");

	        TryInvalid("select * from OrderEvent[books where abc=1]",
	                   "Failed to validate contained-event expression 'abc=1': Property named 'abc' is not valid in any stream [select * from OrderEvent[books where abc=1]]");

	        TryInvalid("select * from OrderEvent[abc]",
	                   "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select * from OrderEvent[abc]]");
	    }

	    private void RunAssertion()
	    {
	        string[] fields = "orderId,bookId,reviewId".Split(',');

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{
	                new object[] {"PO200901", "10020", 1}, new object[] {"PO200901", "10020", 2}, new object[] {"PO200901", "10021", 10}});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][] { new object[] { "PO200904", "10031", 201 } });
	        _listener.Reset();
	    }

	    private void TryInvalid(string text, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(text);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }
	}
} // end of namespace
