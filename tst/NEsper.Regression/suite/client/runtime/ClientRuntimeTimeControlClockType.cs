///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeTimeControlClockType
    {
        public void Run(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddEventType(typeof(SupportBean));

            var runtimeProvider = new EPRuntimeProvider();
            var runtime = runtimeProvider.GetRuntimeInstance(nameof(ClientRuntimeTimeControlClockType), configuration);

            runtime.EventService.AdvanceTime(0);
            ClassicAssert.AreEqual(0, runtime.EventService.CurrentTime);
            ClassicAssert.IsTrue(runtime.EventService.IsExternalClockingEnabled);

            runtime.EventService.ClockInternal();
            ClassicAssert.IsFalse(runtime.EventService.IsExternalClockingEnabled);
            var waitStart = DateTimeHelper.CurrentTimeMillis;
            var waitTarget = waitStart + 10000;

            long currMillis;
            while ((currMillis = DateTimeHelper.CurrentTimeMillis) < waitTarget) {
                if (runtime.EventService.CurrentTime > 0) {
                    break;
                }
            }

            currMillis = DateTimeHelper.CurrentTimeMillis;
            ClassicAssert.AreNotEqual(0, runtime.EventService.CurrentTime);
            Assert.That(currMillis, Is.GreaterThan(runtime.EventService.CurrentTime - 10000));

            runtime.EventService.ClockExternal();
            ClassicAssert.IsTrue(runtime.EventService.IsExternalClockingEnabled);
            runtime.EventService.AdvanceTime(0);
            ThreadSleep(500);
            ClassicAssert.AreEqual(0, runtime.EventService.CurrentTime);

            runtime.Destroy();
        }
    }
} // end of namespace