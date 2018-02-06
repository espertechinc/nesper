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

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowConsumer : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                    "create schema IncomingEvent(id int);\n" +
                            "create schema RetainedEvent(id int);\n" +
                            "insert into RetainedEvent select * from IncomingEvent#expr_batch(current_count >= 10000);\n" +
                            "create window RetainedEventWindow#keepall as RetainedEvent;\n" +
                            "insert into RetainedEventWindow select * from RetainedEvent;\n");
    
            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("id", 1);
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(@event, "IncomingEvent");
            }
        }
    }
} // end of namespace
