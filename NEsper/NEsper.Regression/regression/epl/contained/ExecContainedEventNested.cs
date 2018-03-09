///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.contained
{
    public class ExecContainedEventNested : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionNamedWindowFilter(epService);
            RunAssertionNamedWindowSubquery(epService);
            RunAssertionNamedWindowOnTrigger(epService);
            RunAssertionSimple(epService);
            RunAssertionWhere(epService);
            RunAssertionColumnSelect(epService);
            RunAssertionPatternSelect(epService);
            RunAssertionSubSelect(epService);
            RunAssertionUnderlyingSelect(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionNamedWindowFilter(EPServiceProvider epService) {
            string[] fields = "reviewId".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            epService.EPAdministrator.CreateEPL("create window OrderWindowNWF#lastevent as OrderEvent");
            epService.EPAdministrator.CreateEPL("insert into OrderWindowNWF select * from OrderEvent");
    
            string stmtText = "select reviewId from OrderWindowNWF[books][reviews] bookReviews order by reviewId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {1}, new object[] {2}, new object[] {10}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {201}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionNamedWindowSubquery(EPServiceProvider epService) {
            string[] fields = "TheString,totalPrice".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("create window OrderWindowNWS#lastevent as OrderEvent");
            epService.EPAdministrator.CreateEPL("insert into OrderWindowNWS select * from OrderEvent");
    
            string stmtText = "select *, (select sum(price) from OrderWindowNWS[books]) as totalPrice from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 24d + 35d + 27d}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 15d + 13d}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionNamedWindowOnTrigger(EPServiceProvider epService) {
            string[] fields = "TheString,IntPrimitive".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("create window SupportBeanWindow#lastevent as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into SupportBeanWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create window OrderWindowNWOT#lastevent as OrderEvent");
            epService.EPAdministrator.CreateEPL("insert into OrderWindowNWOT select * from OrderEvent");
    
            string stmtText = "on OrderWindowNWOT[books] owb select sbw.* from SupportBeanWindow sbw where TheString = title";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("Foundation 2", 2));
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"Foundation 2", 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSimple(EPServiceProvider epService) {
            string[] fields = "reviewId".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            string stmtText = "select reviewId from OrderEvent[books][reviews] bookReviews order by reviewId asc";
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            Assert.IsTrue(stmt.StatementContext.IsStatelessSelect);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {1}, new object[] {2}, new object[] {10}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {201}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhere(EPServiceProvider epService) {
            string[] fields = "reviewId".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            // try where in root
            string stmtText = "select reviewId from OrderEvent[books where title = 'Enders Game'][reviews] bookReviews order by reviewId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {1}, new object[] {2}});
            listener.Reset();
    
            // try where in different levels
            stmt.Dispose();
            stmtText = "select reviewId from OrderEvent[books where title = 'Enders Game'][reviews where reviewId in (1, 10)] bookReviews order by reviewId asc";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {1}});
            listener.Reset();
    
            // try where in combination
            stmt.Dispose();
            stmtText = "select reviewId from OrderEvent[books as bc][reviews as rw where rw.reviewId in (1, 10) and bc.title = 'Enders Game'] bookReviews order by reviewId asc";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {1}});
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionColumnSelect(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            // columns supplied
            string stmtText = "select * from OrderEvent[select bookId, orderdetail.orderId as orderId from books][select reviewId from reviews] bookReviews order by reviewId asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // stream wildcards identify fragments
            stmtText = "select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderEvent[books as book][select myorder.* as orderFrag, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // one event type dedicated as underlying
            stmtText = "select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderEvent[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // wildcard unnamed as underlying
            stmtText = "select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
                    "from OrderEvent[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // wildcard named as underlying
            stmtText = "select orderFrag.orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    "from OrderEvent[select * from books as bookFrag][select myorder.* as orderFrag, review.* as reviewFrag from reviews as review] as myorder";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // object model
            stmtText = "select orderFrag.orderdetail.orderId as orderId, bookId, reviewId " +
                    "from OrderEvent[select * from books][select myorder.* as orderFrag, reviewId from reviews as review] as myorder";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model, stmtText);
            stmt.Events += listener.Update;
            TryAssertionColumnSelect(epService, listener);
            stmt.Dispose();
    
            // with where-clause
            stmtText = "select * from AccountEvent[select * from wallets where currency=\"USD\"]";
            model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
        }
    
        private void RunAssertionPatternSelect(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern [" +
                    "every r=OrderEvent[books][reviews] -> SupportBean(IntPrimitive = r[0].reviewId)]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubSelect(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString from SupportBean s0 where " +
                    "exists (select * from OrderEvent[books][reviews]#unique(reviewId) where reviewId = s0.IntPrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnderlyingSelect(EPServiceProvider epService) {
            string[] fields = "orderId,bookId,reviewId".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            string stmtText = "select orderdetail.orderId as orderId, bookFrag.bookId as bookId, reviewFrag.reviewId as reviewId " +
                    //string stmtText = "select * " +
                    "from OrderEvent[books as book][select myorder.*, book.* as bookFrag, review.* as reviewFrag from reviews as review] as myorder";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"PO200901", "10020", 1},
                new object[] {"PO200901", "10020", 2},
                new object[] {"PO200901", "10021", 10}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"PO200904", "10031", 201}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            TryInvalid(epService, "select bookId from OrderEvent[select count(*) from books]",
                    "Expression in a property-selection may not utilize an aggregation function [select bookId from OrderEvent[select count(*) from books]]");
    
            TryInvalid(epService, "select bookId from OrderEvent[select bookId, (select abc from review#lastevent) from books]",
                    "Expression in a property-selection may not utilize a subselect [select bookId from OrderEvent[select bookId, (select abc from review#lastevent) from books]]");
    
            TryInvalid(epService, "select bookId from OrderEvent[select prev(1, bookId) from books]",
                    "Failed to validate contained-event expression 'prev(1,bookId)': Previous function cannot be used in this context [select bookId from OrderEvent[select prev(1, bookId) from books]]");
    
            TryInvalid(epService, "select bookId from OrderEvent[select * from books][select * from reviews]",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [select bookId from OrderEvent[select * from books][select * from reviews]]");
    
            TryInvalid(epService, "select bookId from OrderEvent[select abc from books][reviews]",
                    "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select bookId from OrderEvent[select abc from books][reviews]]");
    
            TryInvalid(epService, "select bookId from OrderEvent[books][reviews]",
                    "Error starting statement: Failed to validate select-clause expression 'bookId': Property named 'bookId' is not valid in any stream [select bookId from OrderEvent[books][reviews]]");
    
            TryInvalid(epService, "select orderId from OrderEvent[books]",
                    "Error starting statement: Failed to validate select-clause expression 'orderId': Property named 'orderId' is not valid in any stream [select orderId from OrderEvent[books]]");
    
            TryInvalid(epService, "select * from OrderEvent[books where abc=1]",
                    "Failed to validate contained-event expression 'abc=1': Property named 'abc' is not valid in any stream [select * from OrderEvent[books where abc=1]]");
    
            TryInvalid(epService, "select * from OrderEvent[abc]",
                    "Failed to validate contained-event expression 'abc': Property named 'abc' is not valid in any stream [select * from OrderEvent[abc]]");
        }
    
        private void TryAssertionColumnSelect(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "orderId,bookId,reviewId".Split(',');
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"PO200901", "10020", 1},
                new object[] {"PO200901", "10020", 2},
                new object[] {"PO200901", "10021", 10}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"PO200904", "10031", 201}});
            listener.Reset();
        }
    }
} // end of namespace
