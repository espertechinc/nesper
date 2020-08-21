///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitParameterizedByContext : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec("2002-05-01T09:00:00.000"));
            var epl = "@Name('ctx') create context MyCtx start SupportScheduleSimpleEvent as sse;\n" +
                      "@Name('s0') context MyCtx\n" +
                      "select count(*) as c \n" +
                      "from SupportBean_S0\n" +
                      "output last at(context.sse.Atminute, context.sse.Athour, *, *, *, *) and when terminated\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportScheduleSimpleEvent(10, 15));
            env.SendEventBean(new SupportBean_S0(0));

            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec("2002-05-01T10:14:59.000"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec("2002-05-01T10:15:00.000"));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }
    }
} // end of namespace