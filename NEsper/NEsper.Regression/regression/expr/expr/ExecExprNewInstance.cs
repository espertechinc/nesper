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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    using Map = IDictionary<string, object>;

    public class ExecExprNewInstance : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionNewInstance(epService, false);
            RunAssertionNewInstance(epService, true);
    
            RunAssertionStreamAlias(epService);
    
            // try variable
            epService.EPAdministrator.CreateEPL("create constant variable com.espertech.esper.compat.AtomicLong cnt = new com.espertech.esper.compat.AtomicLong(1)");
    
            // try shallow invalid cases
            SupportMessageAssertUtil.TryInvalid(epService, "select new Dummy() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'new Dummy()': Failed to resolve new-operator class name 'Dummy'");
    
            epService.EPAdministrator.Configuration.AddImport(typeof(MyClassNoCtor));
            SupportMessageAssertUtil.TryInvalid(epService, "select new MyClassNoCtor() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'new MyClassNoCtor()': Failed to find a suitable constructor for type ");
        }
    
        private void RunAssertionStreamAlias(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyClassObjectCtor));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select " +
                    "new MyClassObjectCtor(sb) as c0 " +
                    "from SupportBean as sb").Events += listener.Update;
    
            var sb = new SupportBean();
            epService.EPRuntime.SendEvent(sb);
            var @event = listener.AssertOneGetNewAndReset();
            Assert.AreSame(sb, ((MyClassObjectCtor) @event.Get("c0")).Value);
        }
    
        private void RunAssertionNewInstance(EPServiceProvider epService, bool soda) {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean));

            var container = epService.Container;

            var epl = "select " +
                    "new SupportBean(\"A\",IntPrimitive) as c0, " +
                    "new SupportBean(\"B\",IntPrimitive+10), " +
                    "new SupportBean() as c2, " +
                    "new SupportBean(\"ABC\",0).get_TheString() as c3 " +
                    "from SupportBean";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var expectedAggType = new object[][]{new object[] {"c0", typeof(SupportBean)}, new object[] {
                "new SupportBean(\"B\",IntPrimitive+10)", typeof(SupportBean)
            }};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmt.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            var fields = "TheString,IntPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            var @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertPropsPono(container, @event.Get("c0"), fields, new object[]{"A", 10});
            EPAssertionUtil.AssertPropsPono(container, ((Map) @event.Underlying).Get("new SupportBean(\"B\",IntPrimitive+10)"), fields, new object[]{"B", 20});
            EPAssertionUtil.AssertPropsPono(container, @event.Get("c2"), fields, new object[]{null, 0});
            Assert.AreEqual("ABC", @event.Get("c3"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }

        public class MyClassNoCtor
        {
            private MyClassNoCtor()
            {
            }
        }

        public class MyClassObjectCtor
        {
            public object Value { get; }
            public MyClassObjectCtor(Object value)
            {
                this.Value = value;
            }
        }
    }
} // end of namespace
