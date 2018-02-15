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

namespace com.espertech.esper.regression.events.objectarray
{
    public class ExecEventObjectArrayInheritanceConfigRuntime : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
    
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
            configOps.AddEventType("RootEvent", new string[]{"base"}, new object[]{typeof(string)});
            configOps.AddEventType("Sub1Event", new string[]{"sub1"}, new object[]{typeof(string)}, new ConfigurationEventTypeObjectArray(Collections.SingletonSet("RootEvent")));
            configOps.AddEventType("Sub2Event", new string[]{"sub2"}, new object[]{typeof(string)}, new ConfigurationEventTypeObjectArray(Collections.SingletonSet("RootEvent")));
            configOps.AddEventType("SubAEvent", new string[]{"suba"}, new object[]{typeof(string)}, new ConfigurationEventTypeObjectArray(Collections.SingletonSet("Sub1Event")));
            configOps.AddEventType("SubBEvent", new string[]{"subb"}, new object[]{typeof(string)}, new ConfigurationEventTypeObjectArray(Collections.SingletonSet("SubAEvent")));
    
            ExecEventObjectArrayInheritanceConfigInit.RunObjectArrInheritanceAssertion(epService);
        }
    }
} // end of namespace
