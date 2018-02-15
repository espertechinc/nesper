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
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapNestedEscapeDot : RegressionExecution {
        public override void Configure(Configuration configuration) {
            IDictionary<string, Object> definition = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"a.b", typeof(int)},
                    new object[] {"a.b.c", typeof(int)},
                    new object[] {"nes.", typeof(int)},
                    new object[] {"nes.nes2", ExecEventMap.MakeMap(new object[][]{new object[] {"x.y", typeof(int)}})}
            });
            configuration.AddEventType("DotMap", definition);
        }
    
        public override void Run(EPServiceProvider epService) {
            string statementText = "select a\\.b, a\\.b\\.c, nes\\., nes\\.nes2.x\\.y from DotMap";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            IDictionary<string, Object> data = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"a.b", 10},
                    new object[] {"a.b.c", 20},
                    new object[] {"nes.", 30},
                    new object[] {"nes.nes2", ExecEventMap.MakeMap(new object[][]{new object[] {"x.y", 40}})}
            });
            epService.EPRuntime.SendEvent(data, "DotMap");
    
            string[] fields = "a.b,a.b.c,nes.,nes.nes2.x.y".Split(',');
            EventBean received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{10, 20, 30, 40});
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
