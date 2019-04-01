///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprDotExpressionDuckTyping : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Expression.IsDuckTyping = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanDuckType", typeof(SupportBeanDuckType));
    
            string epl = "select " +
                    "(dt).MakeString() as strval, " +
                    "(dt).MakeInteger() as intval, " +
                    "(dt).MakeCommon().MakeString() as commonstrval, " +
                    "(dt).MakeCommon().MakeInteger() as commonintval, " +
                    "(dt).ReturnDouble() as commondoubleval " +
                    "from SupportBeanDuckType dt ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var rows = new object[][]{
                    new object[] {"strval", typeof(Object)},
                    new object[] {"intval", typeof(Object)},
                    new object[] {"commonstrval", typeof(Object)},
                    new object[] {"commonintval", typeof(Object)},
                    new object[] {"commondoubleval", typeof(double)}   // this one is strongly typed
            };
            for (int i = 0; i < rows.Length; i++) {
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            string[] fields = "strval,intval,commonstrval,commonintval,commondoubleval".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBeanDuckTypeOne("x"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"x", null, null, -1, 12.9876d});
    
            epService.EPRuntime.SendEvent(new SupportBeanDuckTypeTwo(-10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, -10, "mytext", null, 11.1234d});
        }
    }
} // end of namespace
