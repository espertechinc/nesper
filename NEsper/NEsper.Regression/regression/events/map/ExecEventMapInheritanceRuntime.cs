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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapInheritanceRuntime : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            IDictionary<string, Object> root = ExecEventMap.MakeMap(new object[][]{new object[] {"base", typeof(string)}});
            IDictionary<string, Object> sub1 = ExecEventMap.MakeMap(new object[][]{new object[] {"sub1", typeof(string)}});
            IDictionary<string, Object> sub2 = ExecEventMap.MakeMap(new object[][]{new object[] {"sub2", typeof(string)}});
            IDictionary<string, Object> suba = ExecEventMap.MakeMap(new object[][]{new object[] {"suba", typeof(string)}});
            IDictionary<string, Object> subb = ExecEventMap.MakeMap(new object[][]{new object[] {"subb", typeof(string)}});
    
            epService.EPAdministrator.Configuration.AddEventType("RootEvent", root);
            epService.EPAdministrator.Configuration.AddEventType("Sub1Event", sub1, new string[]{"RootEvent"});
            epService.EPAdministrator.Configuration.AddEventType("Sub2Event", sub2, new string[]{"RootEvent"});
            epService.EPAdministrator.Configuration.AddEventType("SubAEvent", suba, new string[]{"Sub1Event"});
            epService.EPAdministrator.Configuration.AddEventType("SubBEvent", subb, new string[]{"Sub1Event", "Sub2Event"});
    
            ExecEventMapInheritanceInitTime.RunAssertionMapInheritance(epService);
        }
    }
} // end of namespace
