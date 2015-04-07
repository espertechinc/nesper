///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.events;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestContainedEventExample 
	{
	    private EPServiceProvider _epService;
	    private XmlDocument _eventDocOne;
	    private XmlDocument _eventDocTwo;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}

	        var config = new ConfigurationEventTypeXMLDOM();
	        var schemaUri = ResourceManager.ResolveResourceURL("regression/mediaOrderSchema.xsd").ToString();
	        config.SchemaResource = schemaUri;
	        config.RootElementName = "mediaorder";

	        _epService.EPAdministrator.Configuration.AddEventType("MediaOrder", config);
	        _epService.EPAdministrator.Configuration.AddEventType("Cancel", config);

	        var xmlStreamOne = ResourceManager.GetResourceAsStream("regression/mediaOrderOne.xml");
	        _eventDocOne = SupportXML.GetDocument(xmlStreamOne);

	        var xmlStreamTwo = ResourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml");
	        _eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _eventDocOne = null;
	        _eventDocTwo = null;
	    }

        [Test]
	    public void TestExample()
	    {
	        var stmtTextOne = "select orderId, items.item[0].itemId from MediaOrder";
	        var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
	        var listenerOne = new SupportUpdateListener();
	        stmtOne.AddListener(listenerOne);

	        var stmtTextTwo = "select * from MediaOrder[books.book]";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
	        var listenerTwo = new SupportUpdateListener();
	        stmtTwo.AddListener(listenerTwo);

	        var stmtTextThree = "select * from MediaOrder(orderId='PO200901')[books.book]";
	        var stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
	        var listenerThree = new SupportUpdateListener();
	        stmtThree.AddListener(listenerThree);

	        var stmtTextFour = "select count(*) from MediaOrder[books.book].std:unique(bookId)";
	        var stmtFour = _epService.EPAdministrator.CreateEPL(stmtTextFour);
	        var listenerFour = new SupportUpdateListener();
	        stmtFour.AddListener(listenerFour);

	        var stmtTextFive = "select * from MediaOrder[books.book][review]";
	        var stmtFive = _epService.EPAdministrator.CreateEPL(stmtTextFive);
	        var listenerFive = new SupportUpdateListener();
	        stmtFive.AddListener(listenerFive);

	        var stmtTextSix = "select * from pattern [c=Cancel -> o=MediaOrder(orderId = c.orderId)[books.book]]";
	        var stmtSix = _epService.EPAdministrator.CreateEPL(stmtTextSix);
	        var listenerSix = new SupportUpdateListener();
	        stmtSix.AddListener(listenerSix);

	        var stmtTextSeven = "select * from MediaOrder[select orderId, bookId from books.book][select * from review]";
	        var stmtSeven = _epService.EPAdministrator.CreateEPL(stmtTextSeven);
	        var listenerSeven = new SupportUpdateListener();
	        stmtSeven.AddListener(listenerSeven);

	        var stmtTextEight = "select * from MediaOrder[select * from books.book][select reviewId, comment from review]";
	        var stmtEight = _epService.EPAdministrator.CreateEPL(stmtTextEight);
	        var listenerEight = new SupportUpdateListener();
	        stmtEight.AddListener(listenerEight);

	        var stmtTextNine = "select * from MediaOrder[books.book as book][select book.*, reviewId, comment from review]";
	        var stmtNine = _epService.EPAdministrator.CreateEPL(stmtTextNine);
	        var listenerNine = new SupportUpdateListener();
	        stmtNine.AddListener(listenerNine);

	        var stmtTextTen = "select * from MediaOrder[books.book as book][select mediaOrder.*, bookId, reviewId from review] as mediaOrder";
	        var stmtTen = _epService.EPAdministrator.CreateEPL(stmtTextTen);
	        var listenerTen = new SupportUpdateListener();
	        stmtTen.AddListener(listenerTen);

	        var stmtTextEleven_0 = "insert into ReviewStream select * from MediaOrder[books.book as book]\n" +
	                "    [select mediaOrder.* as mediaOrder, book.* as book, review.* as review from review as review] as mediaOrder";
	        _epService.EPAdministrator.CreateEPL(stmtTextEleven_0);
	        var stmtTextEleven_1 = "select mediaOrder.orderId, book.bookId, review.reviewId from ReviewStream";
	        var stmtEleven_1 = _epService.EPAdministrator.CreateEPL(stmtTextEleven_1);
	        var listenerEleven = new SupportUpdateListener();
	        stmtEleven_1.AddListener(listenerEleven);

	        var stmtTextTwelve = "select * from MediaOrder[books.book where author = 'Orson Scott Card'][review]";
	        var stmtTwelve = _epService.EPAdministrator.CreateEPL(stmtTextTwelve);
	        var listenerTwelve = new SupportUpdateListener();
	        stmtTwelve.AddListener(listenerTwelve);

	        _epService.EPRuntime.SendEvent(_eventDocOne);

	        EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "orderId,items.item[0].itemId".Split(','), new object[]{"PO200901", "100001"});
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.LastNewData, "bookId".Split(','), new object[][] { new object[] { "B001" }, new object[] { "B002" } });
            EPAssertionUtil.AssertPropsPerRow(listenerThree.LastNewData, "bookId".Split(','), new object[][] { new object[] { "B001" }, new object[] { "B002" } });
            EPAssertionUtil.AssertPropsPerRow(listenerFour.LastNewData, "count(*)".Split(','), new object[][] { new object[] { 2L } });
            EPAssertionUtil.AssertPropsPerRow(listenerFive.LastNewData, "reviewId".Split(','), new object[][] { new object[] { "1" } });
	        Assert.IsFalse(listenerSix.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(listenerSeven.LastNewData, "orderId,bookId,reviewId".Split(','), new object[][] { new object[] { "PO200901", "B001", "1" } });
            EPAssertionUtil.AssertPropsPerRow(listenerEight.LastNewData, "reviewId,bookId".Split(','), new object[][] { new object[] { "1", "B001" } });
            EPAssertionUtil.AssertPropsPerRow(listenerNine.LastNewData, "reviewId,bookId".Split(','), new object[][] { new object[] { "1", "B001" } });
            EPAssertionUtil.AssertPropsPerRow(listenerTen.LastNewData, "reviewId,bookId".Split(','), new object[][] { new object[] { "1", "B001" } });
            EPAssertionUtil.AssertPropsPerRow(listenerEleven.LastNewData, "mediaOrder.orderId,book.bookId,review.reviewId".Split(','), new object[][] { new object[] { "PO200901", "B001", "1" } });
            EPAssertionUtil.AssertPropsPerRow(listenerTwelve.LastNewData, "reviewId".Split(','), new object[][] { new object[] { "1" } });
	    }

        [Test]
	    public void TestJoinSelfJoin()
	    {
	        var stmtText = "select book.bookId,item.itemId from MediaOrder[books.book] as book, MediaOrder[items.item] as item where productId = bookId order by bookId, item.itemId asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var fields = "book.bookId,item.itemId".Split(',');
	        _epService.EPRuntime.SendEvent(_eventDocOne);
	        PrintRows(listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B001", "100001" } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B001", "100001" } });

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B005", "200002" }, new object[] { "B005", "200004" }, new object[] { "B006", "200001" } });

	        // count
	        stmt.Dispose();
	        fields = "count(*)".Split(',');
	        stmtText = "select count(*) from MediaOrder[books.book] as book, MediaOrder[items.item] as item where productId = bookId order by bookId asc";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 3L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 4L } });

	        // unidirectional count
	        stmt.Dispose();
	        stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional, MediaOrder[items.item] as item where productId = bookId order by bookId asc";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 3L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 1L } });
	    }

        [Test]
	    public void TestJoinSelfLeftOuterJoin()
	    {
	        var stmtText = "select book.bookId,item.itemId from MediaOrder[books.book] as book left outer join MediaOrder[items.item] as item on productId = bookId order by bookId, item.itemId asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var fields = "book.bookId,item.itemId".Split(',');
	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B005", "200002" }, new object[] { "B005", "200004" }, new object[] { "B006", "200001" }, new object[] { "B008", null } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
	        PrintRows(listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B001", "100001" }, new object[] { "B002", null } });

	        // count
	        stmt.Dispose();
	        fields = "count(*)".Split(',');
	        stmtText = "select count(*) from MediaOrder[books.book] as book left outer join MediaOrder[items.item] as item on productId = bookId";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 4L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 6L } });

	        // unidirectional count
	        stmt.Dispose();
	        stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional left outer join MediaOrder[items.item] as item on productId = bookId";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 4L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 2L } });
	    }

        [Test]
	    public void TestJoinSelfFullOuterJoin()
	    {
	        var stmtText = "select orderId, book.bookId,item.itemId from MediaOrder[books.book] as book full outer join MediaOrder[select orderId, * from items.item] as item on productId = bookId order by bookId, item.itemId asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var fields = "book.bookId,item.itemId".Split(',');
	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { null, "200003" }, new object[] { "B005", "200002" }, new object[] { "B005", "200004" }, new object[] { "B006", "200001" }, new object[] { "B008", null } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
	        PrintRows(listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "B001", "100001" }, new object[] { "B002", null } });

	        // count
	        stmt.Dispose();
	        fields = "count(*)".Split(',');
	        stmtText = "select count(*) from MediaOrder[books.book] as book full outer join MediaOrder[items.item] as item on productId = bookId";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 5L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 7L } });

	        // unidirectional count
	        stmt.Dispose();
	        stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional full outer join MediaOrder[items.item] as item on productId = bookId";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(_eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 4L } });

	        _epService.EPRuntime.SendEvent(_eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 2L } });
	    }

	    private void PrintRows(EventBean[] rows)
	    {
	        var renderer = _epService.EPRuntime.EventRenderer.GetJSONRenderer(rows[0].EventType);
	        for (var i = 0; i < rows.Length; i++)
	        {
	            // System.out.println(renderer.render("event#" + i, rows[i]));
	            renderer.Render("event#" + i, rows[i]);
	        }
	    }

        [Test]
	    public void TestSolutionPattern()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("ResponseEvent", typeof(ResponseEvent));

	        var fields = "category,subEventType,avgTime".Split(',');
	        var stmtText = "select category, subEventType, avg(responseTimeMillis) as avgTime from ResponseEvent[select category, * from subEvents].win:time(1 min) group by category, subEventType order by category, subEventType";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new ResponseEvent("svcOne", new SubEvent[] {new SubEvent(1000, "typeA"), new SubEvent(800, "typeB")}));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "svcOne", "typeA", 1000.0 }, new object[] { "svcOne", "typeB", 800.0 } });

	        _epService.EPRuntime.SendEvent(new ResponseEvent("svcOne", new SubEvent[] {new SubEvent(400, "typeB"), new SubEvent(500, "typeA")}));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "svcOne", "typeA", 750.0 }, new object[] { "svcOne", "typeB", 600.0 } });
	    }

	    public class ResponseEvent
	    {
	        public ResponseEvent(string category, SubEvent[] subEvents)
	        {
	            Category = category;
	            SubEvents = subEvents;
	        }

	        public string Category { get; private set; }

	        public SubEvent[] SubEvents { get; private set; }
	    }

	    public class SubEvent
	    {
	        public SubEvent(long responseTimeMillis, string subEventType)
	        {
	            ResponseTimeMillis = responseTimeMillis;
	            SubEventType = subEventType;
	        }

	        public long ResponseTimeMillis { get; private set; }

	        public string SubEventType { get; private set; }
	    }
	}
} // end of namespace
