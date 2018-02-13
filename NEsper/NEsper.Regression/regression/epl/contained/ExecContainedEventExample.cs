///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Xml;
using Avro.IO;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regression.events;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertFalse;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.contained
{
    public class ExecContainedEventExample : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var config = new ConfigurationEventTypeXMLDOM();
            string schemaUri = ResourceManager.ResolveResourceURL("regression/mediaOrderSchema.xsd").ToString();
            config.SchemaResource = schemaUri;
            config.RootElementName = "mediaorder";
    
            epService.EPAdministrator.Configuration.AddEventType("MediaOrder", config);
            epService.EPAdministrator.Configuration.AddEventType("Cancel", config);
    
            var xmlStreamOne = ResourceManager.GetResourceAsStream("regression/mediaOrderOne.xml");
            var eventDocOne = SupportXML.GetDocument(xmlStreamOne);
    
            var xmlStreamTwo = ResourceManager.GetResourceAsStream("regression/mediaOrderTwo.xml");
            var eventDocTwo = SupportXML.GetDocument(xmlStreamTwo);
    
            RunAssertionExample(epService, eventDocOne);
            RunAssertionJoinSelfJoin(epService, eventDocOne, eventDocTwo);
            RunAssertionJoinSelfLeftOuterJoin(epService, eventDocOne, eventDocTwo);
            RunAssertionJoinSelfFullOuterJoin(epService, eventDocOne, eventDocTwo);
            RunAssertionSolutionPattern(epService);
        }
    
        private void RunAssertionExample(EPServiceProvider epService, XmlDocument eventDocOne) {
            string stmtTextOne = "select orderId, items.item[0].itemId from MediaOrder";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.AddListener(listenerOne);
    
            string stmtTextTwo = "select * from MediaOrder[books.book]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.AddListener(listenerTwo);
    
            string stmtTextThree = "select * from MediaOrder(orderId='PO200901')[books.book]";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            var listenerThree = new SupportUpdateListener();
            stmtThree.AddListener(listenerThree);
    
            string stmtTextFour = "select count(*) from MediaOrder[books.book]#unique(bookId)";
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL(stmtTextFour);
            var listenerFour = new SupportUpdateListener();
            stmtFour.AddListener(listenerFour);
    
            string stmtTextFive = "select * from MediaOrder[books.book][review]";
            EPStatement stmtFive = epService.EPAdministrator.CreateEPL(stmtTextFive);
            var listenerFive = new SupportUpdateListener();
            stmtFive.AddListener(listenerFive);
    
            string stmtTextSix = "select * from pattern [c=Cancel -> o=MediaOrder(orderId = c.orderId)[books.book]]";
            EPStatement stmtSix = epService.EPAdministrator.CreateEPL(stmtTextSix);
            var listenerSix = new SupportUpdateListener();
            stmtSix.AddListener(listenerSix);
    
            string stmtTextSeven = "select * from MediaOrder[select orderId, bookId from books.book][select * from review]";
            EPStatement stmtSeven = epService.EPAdministrator.CreateEPL(stmtTextSeven);
            var listenerSeven = new SupportUpdateListener();
            stmtSeven.AddListener(listenerSeven);
    
            string stmtTextEight = "select * from MediaOrder[select * from books.book][select reviewId, comment from review]";
            EPStatement stmtEight = epService.EPAdministrator.CreateEPL(stmtTextEight);
            var listenerEight = new SupportUpdateListener();
            stmtEight.AddListener(listenerEight);
    
            string stmtTextNine = "select * from MediaOrder[books.book as book][select book.*, reviewId, comment from review]";
            EPStatement stmtNine = epService.EPAdministrator.CreateEPL(stmtTextNine);
            var listenerNine = new SupportUpdateListener();
            stmtNine.AddListener(listenerNine);
    
            string stmtTextTen = "select * from MediaOrder[books.book as book][select mediaOrder.*, bookId, reviewId from review] as mediaOrder";
            EPStatement stmtTen = epService.EPAdministrator.CreateEPL(stmtTextTen);
            var listenerTen = new SupportUpdateListener();
            stmtTen.AddListener(listenerTen);
    
            string stmtTextElevenZero = "insert into ReviewStream select * from MediaOrder[books.book as book]\n" +
                    "    [select mediaOrder.* as mediaOrder, book.* as book, review.* as review from review as review] as mediaOrder";
            epService.EPAdministrator.CreateEPL(stmtTextElevenZero);
            string stmtTextElevenOne = "select mediaOrder.orderId, book.bookId, review.reviewId from ReviewStream";
            EPStatement stmtElevenOne = epService.EPAdministrator.CreateEPL(stmtTextElevenOne);
            var listenerEleven = new SupportUpdateListener();
            stmtElevenOne.AddListener(listenerEleven);
    
            string stmtTextTwelve = "select * from MediaOrder[books.book where author = 'Orson Scott Card'][review]";
            EPStatement stmtTwelve = epService.EPAdministrator.CreateEPL(stmtTextTwelve);
            var listenerTwelve = new SupportUpdateListener();
            stmtTwelve.AddListener(listenerTwelve);
    
            epService.EPRuntime.SendEvent(eventDocOne);
    
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "orderId,items.item[0].itemId".Split(','), new Object[]{"PO200901", "100001"});
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.LastNewData, "bookId".Split(','), new Object[][]{new object[] {"B001"}, new object[] {"B002"}});
            EPAssertionUtil.AssertPropsPerRow(listenerThree.LastNewData, "bookId".Split(','), new Object[][]{new object[] {"B001"}, new object[] {"B002"}});
            EPAssertionUtil.AssertPropsPerRow(listenerFour.LastNewData, "count(*)".Split(','), new Object[][]{new object[] {2L}});
            EPAssertionUtil.AssertPropsPerRow(listenerFive.LastNewData, "reviewId".Split(','), new Object[][]{new object[] {"1"}});
            Assert.IsFalse(listenerSix.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(listenerSeven.LastNewData, "orderId,bookId,reviewId".Split(','), new Object[][]{new object[] {"PO200901", "B001", "1"}});
            EPAssertionUtil.AssertPropsPerRow(listenerEight.LastNewData, "reviewId,bookId".Split(','), new Object[][]{new object[] {"1", "B001"}});
            EPAssertionUtil.AssertPropsPerRow(listenerNine.LastNewData, "reviewId,bookId".Split(','), new Object[][]{new object[] {"1", "B001"}});
            EPAssertionUtil.AssertPropsPerRow(listenerTen.LastNewData, "reviewId,bookId".Split(','), new Object[][]{new object[] {"1", "B001"}});
            EPAssertionUtil.AssertPropsPerRow(listenerEleven.LastNewData, "mediaOrder.orderId,book.bookId,review.reviewId".Split(','), new Object[][]{new object[] {"PO200901", "B001", "1"}});
            EPAssertionUtil.AssertPropsPerRow(listenerTwelve.LastNewData, "reviewId".Split(','), new Object[][]{new object[] {"1"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinSelfJoin(EPServiceProvider epService, XmlDocument eventDocOne, XmlDocument eventDocTwo) {
            string stmtText = "select book.bookId,item.itemId from MediaOrder[books.book] as book, MediaOrder[items.item] as item where productId = bookId order by bookId, item.itemId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            string[] fields = "book.bookId,item.itemId".Split(',');
            epService.EPRuntime.SendEvent(eventDocOne);
            PrintRows(epService, listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B001", "100001"}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B001", "100001"}});
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B005", "200002"}, new object[] {"B005", "200004"}, new object[] {"B006", "200001"}});
    
            // count
            stmt.Dispose();
            fields = "count(*)".Split(',');
            stmtText = "select count(*) from MediaOrder[books.book] as book, MediaOrder[items.item] as item where productId = bookId order by bookId asc";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {3L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {4L}});
    
            // unidirectional count
            stmt.Dispose();
            stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional, MediaOrder[items.item] as item where productId = bookId order by bookId asc";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {3L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {1L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelfLeftOuterJoin(EPServiceProvider epService, XmlDocument eventDocOne, XmlDocument eventDocTwo) {
            string stmtText = "select book.bookId,item.itemId from MediaOrder[books.book] as book left outer join MediaOrder[items.item] as item on productId = bookId order by bookId, item.itemId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            string[] fields = "book.bookId,item.itemId".Split(',');
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B005", "200002"}, new object[] {"B005", "200004"}, new object[] {"B006", "200001"}, new object[] {"B008", null}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            PrintRows(epService, listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B001", "100001"}, new object[] {"B002", null}});
    
            // count
            stmt.Dispose();
            fields = "count(*)".Split(',');
            stmtText = "select count(*) from MediaOrder[books.book] as book left outer join MediaOrder[items.item] as item on productId = bookId";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {4L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {6L}});
    
            // unidirectional count
            stmt.Dispose();
            stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional left outer join MediaOrder[items.item] as item on productId = bookId";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {4L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {2L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelfFullOuterJoin(EPServiceProvider epService, XmlDocument eventDocOne, XmlDocument eventDocTwo) {
            string stmtText = "select orderId, book.bookId,item.itemId from MediaOrder[books.book] as book full outer join MediaOrder[select orderId, * from items.item] as item on productId = bookId order by bookId, item.itemId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            string[] fields = "book.bookId,item.itemId".Split(',');
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {null, "200003"}, new object[] {"B005", "200002"}, new object[] {"B005", "200004"}, new object[] {"B006", "200001"}, new object[] {"B008", null}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            PrintRows(epService, listener.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"B001", "100001"}, new object[] {"B002", null}});
    
            // count
            stmt.Dispose();
            fields = "count(*)".Split(',');
            stmtText = "select count(*) from MediaOrder[books.book] as book full outer join MediaOrder[items.item] as item on productId = bookId";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {5L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {7L}});
    
            // unidirectional count
            stmt.Dispose();
            stmtText = "select count(*) from MediaOrder[books.book] as book unidirectional full outer join MediaOrder[items.item] as item on productId = bookId";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(eventDocTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {4L}});
    
            epService.EPRuntime.SendEvent(eventDocOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {2L}});
    
            stmt.Dispose();
        }
    
        private void PrintRows(EPServiceProvider epService, EventBean[] rows) {
            JSONEventRenderer renderer = epService.EPRuntime.EventRenderer.GetJSONRenderer(rows[0].EventType);
            for (int i = 0; i < rows.Length; i++) {
                // Log.Info(renderer.Render("event#" + i, rows[i]));
                renderer.Render("event#" + i, rows[i]);
            }
        }
    
        private void RunAssertionSolutionPattern(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ResponseEvent", typeof(ResponseEvent));
    
            string[] fields = "category,subEventType,avgTime".Split(',');
            string stmtText = "select category, subEventType, avg(responseTimeMillis) as avgTime from ResponseEvent[select category, * from subEvents]#Time(1 min) group by category, subEventType order by category, subEventType";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new ResponseEvent("svcOne", new SubEvent[]{new SubEvent(1000, "typeA"), new SubEvent(800, "typeB")}));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"svcOne", "typeA", 1000.0}, new object[] {"svcOne", "typeB", 800.0}});
    
            epService.EPRuntime.SendEvent(new ResponseEvent("svcOne", new SubEvent[]{new SubEvent(400, "typeB"), new SubEvent(500, "typeA")}));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new object[] {"svcOne", "typeA", 750.0}, new object[] {"svcOne", "typeB", 600.0}});
    
            stmt.Dispose();
        }
    
        [Serializable]
        public class ResponseEvent  {
            private string category;
            private SubEvent[] subEvents;
    
            public ResponseEvent(string category, SubEvent[] subEvents) {
                this.category = category;
                this.subEvents = subEvents;
            }

            public string Category
            {
                get { return category; }
            }

            public SubEvent[] SubEvents
            {
                get { return subEvents; }
            }
        }
    
        [Serializable]
        public class SubEvent  {
            private long responseTimeMillis;
            private string subEventType;
    
            public SubEvent(long responseTimeMillis, string subEventType) {
                this.responseTimeMillis = responseTimeMillis;
                this.subEventType = subEventType;
            }

            public long ResponseTimeMillis
            {
                get { return responseTimeMillis; }
            }

            public string SubEventType
            {
                get { return subEventType; }
            }
        }
    }
} // end of namespace
