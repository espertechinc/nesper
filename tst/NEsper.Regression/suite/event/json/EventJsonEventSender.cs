///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonEventSender
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withd(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonEventSenderParseAndSend());
            return execs;
        }

        internal class EventJsonEventSenderParseAndSend : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype @JsonSchema create json schema MyEvent(p1 string);\n" +
                    "@name('s0') select * from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var sender = (EventSenderJson)env.Runtime.EventService.GetEventSender("MyEvent");
                var underlying = (JsonEventObject)sender.Parse("{\"p1\": \"abc\"}");

                sender.SendEvent(underlying);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }
    }
} // end of namespace