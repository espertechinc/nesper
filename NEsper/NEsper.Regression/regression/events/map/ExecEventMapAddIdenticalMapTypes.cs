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
    public class ExecEventMapAddIdenticalMapTypes : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            IDictionary<string, Object> levelOne1 = ExecEventMap.MakeMap(new object[][]{new object[] {"simpleOne", typeof(int?)}});
            IDictionary<string, Object> levelOne2 = ExecEventMap.MakeMap(new object[][]{new object[] {"simpleOne", typeof(long)}});
            IDictionary<string, Object> levelZero1 = ExecEventMap.MakeMap(new object[][]{new object[] {"map", levelOne1}});
            IDictionary<string, Object> levelZero2 = ExecEventMap.MakeMap(new object[][]{new object[] {"map", levelOne2}});
    
            // can add the same nested type twice
            epService.EPAdministrator.Configuration.AddEventType("ABC", levelZero1);
            epService.EPAdministrator.Configuration.AddEventType("ABC", levelZero1);
            try {
                // changing the definition however stops the compatibility
                epService.EPAdministrator.Configuration.AddEventType("ABC", levelZero2);
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
        }
    }
} // end of namespace
