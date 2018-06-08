///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    /// <summary>
    /// Test for populating an empty type:
    /// - an empty insert-into property list is allowed, i.e. "insert into EmptySchema()"
    /// - an empty select-clause is not allowed, i.e. "select from xxx" fails
    /// - we require "select null from" (unnamed null column) for populating an empty type
    /// </summary>
    public class ExecInsertIntoEmptyPropType : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionNamedWindowModelAfter(epService);
            RunAssertionCreateSchemaInsertInto(epService);
        }
    
        private void RunAssertionNamedWindowModelAfter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
    
            epService.EPAdministrator.CreateEPL("create schema EmptyPropSchema()");
            EPStatement stmtCreateWindow = epService.EPAdministrator.CreateEPL("create window EmptyPropWin#keepall as EmptyPropSchema");
            epService.EPAdministrator.CreateEPL("insert into EmptyPropWin() select null from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean());
    
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator());
            Assert.AreEqual(1, events.Length);
            Assert.AreEqual("EmptyPropWin", events[0].EventType.Name);
    
            // try fire-and-forget query
            epService.EPRuntime.ExecuteQuery("insert into EmptyPropWin select null");
            Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);
            epService.EPRuntime.ExecuteQuery("delete from EmptyPropWin"); // empty window
    
            // try on-merge
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 merge EmptyPropWin " +
                    "when not matched then insert select null");
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);
    
            // try on-insert
            epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into EmptyPropWin select null");
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreateSchemaInsertInto(EPServiceProvider epService) {
            TryAssertionInsertMap(epService, true);
            TryAssertionInsertMap(epService, false);
            TryAssertionInsertOA(epService);
            TryAssertionInsertBean(epService);
        }
    
        private void TryAssertionInsertBean(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create schema MyBeanWithoutProps as " + typeof(MyBeanWithoutProps).MaskTypeName());
            epService.EPAdministrator.CreateEPL("insert into MyBeanWithoutProps select null from SupportBean");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyBeanWithoutProps");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.AssertOneGetNewAndReset().Underlying is MyBeanWithoutProps);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionInsertMap(EPServiceProvider epService, bool soda) {
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create map schema EmptyMapSchema as ()");
            epService.EPAdministrator.CreateEPL("insert into EmptyMapSchema() select null from SupportBean");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from EmptyMapSchema");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            Assert.IsTrue(((IDictionary<string, object>) @event.Underlying).IsEmpty());
            Assert.AreEqual(0, @event.EventType.PropertyDescriptors.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionInsertOA(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema EmptyOASchema()");
            epService.EPAdministrator.CreateEPL("insert into EmptyOASchema select null from SupportBean");
    
            var supportSubscriber = new SupportSubscriber();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from EmptyOASchema");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            stmt.Subscriber = supportSubscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(0, ((object[]) listener.AssertOneGetNewAndReset().Underlying).Length);
    
            object[] lastNewSubscriberData = supportSubscriber.LastNewData;
            Assert.AreEqual(1, lastNewSubscriberData.Length);
            Assert.AreEqual(0, ((object[]) lastNewSubscriberData[0]).Length);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private class MyBeanWithoutProps {
        }
    }
} // end of namespace
