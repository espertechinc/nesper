///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.regressionlib.suite.infra.namedwindow;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Named Window tests
    [TestFixture]
    public class TestSuiteInfraNamedWindowWConfig
    {
        private void RunAssertion(
            bool useDefault,
            bool? preserve,
            Locking? locking)
        {
            var session = RegressionRunner.Session();
            if (!useDefault) {
                session.Configuration.Runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder =
                    preserve.GetValueOrDefault();
                session.Configuration.Runtime.Threading.NamedWindowConsumerDispatchLocking =
                    locking.GetValueOrDefault(Locking.SPIN);
            }

            var exec = new InfraNamedWindowOnUpdateWMultiDispatch();
            RegressionRunner.Run(session, exec);
            session.Destroy();
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOnUpdateWMultiDispatch()
        {
            RunAssertion(true, null, null);
            RunAssertion(false, true, Locking.SPIN);
            RunAssertion(false, true, Locking.SUSPEND);
            RunAssertion(false, false, null);
        }
    }
} // end of namespace