///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadStmtPatternFollowedBy
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(Configuration configuration)
        {
            RunAssertionPatternFollowedBy(FilterServiceProfile.READMOSTLY, configuration);
            RunAssertionPatternFollowedBy(FilterServiceProfile.READWRITE, configuration);
        }

        public void RunReadMostly(Configuration configuration)
        {
            RunAssertionPatternFollowedBy(FilterServiceProfile.READMOSTLY, configuration);
        }

        public void RunReadWrite(Configuration configuration)
        {
            RunAssertionPatternFollowedBy(FilterServiceProfile.READWRITE, configuration);
        }

        private static void RunAssertionPatternFollowedBy(
            FilterServiceProfile profile,
            Configuration config)
        {
            config.Common.AddEventType("S0", typeof(SupportBean_S0));
            var runtimeURI = typeof(MultithreadStmtPatternFollowedBy).Name + "_" + profile;
            var runtime = EPRuntimeProvider.GetRuntime(runtimeURI, config);
            runtime.Initialize();

            string[] epls = {
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=0)->sb=S0(Id=1)) or (sc=S0(Id=1)->sd=S0(Id=0))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=1)->sb=S0(Id=2)) or (sc=S0(Id=2)->sd=S0(Id=1))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=2)->sb=S0(Id=3)) or (sc=S0(Id=3)->sd=S0(Id=2))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=3)->sb=S0(Id=4)) or (sc=S0(Id=4)->sd=S0(Id=3))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=4)->sb=S0(Id=5)) or (sc=S0(Id=5)->sd=S0(Id=4))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=5)->sb=S0(Id=6)) or (sc=S0(Id=6)->sd=S0(Id=5))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=6)->sb=S0(Id=7)) or (sc=S0(Id=7)->sd=S0(Id=6))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=7)->sb=S0(Id=8)) or (sc=S0(Id=8)->sd=S0(Id=7))]",
                "select sa.Id,sb.Id,sc.Id,sd.Id from pattern [(sa=S0(Id=8)->sb=S0(Id=9)) or (sc=S0(Id=9)->sd=S0(Id=8))]"
            };

            for (var i = 0; i < 20; i++) {
                log.Info("i=" + i);
                var listener = new SupportMTUpdateListener();
                var stmts = new EPStatement[epls.Length];
                for (var j = 0; j < epls.Length; j++) {
                    var deployed = CompileDeploy(epls[j], runtime, config);
                    stmts[j] = deployed.Statements[0];
                    stmts[j].AddListener(listener);
                }

                int[] threadOneValues = {0, 2, 4, 6, 8};
                int[] threadTwoValues = {1, 3, 5, 7, 9};

                var threadOne = new Thread(new SenderRunnable(runtime.EventService, threadOneValues).Run);
                threadOne.Name = typeof(MultithreadStmtPatternFollowedBy).Name + "-one";

                var threadTwo = new Thread(new SenderRunnable(runtime.EventService, threadTwoValues).Run);
                threadTwo.Name = typeof(MultithreadStmtPatternFollowedBy).Name + "-two";

                threadOne.Start();
                threadTwo.Start();
                ThreadJoin(threadOne);
                ThreadJoin(threadTwo);

                var events = listener.GetNewDataListFlattened();
                /* Comment in to print events delivered.
                for (int j = 0; j < events.length; j++) {
                    EventBean out = events[j];
                    /*
                    System.out.println(" sa=" + getNull(out.get("sa.Id")) +
                                       " sb=" + getNull(out.get("sb.Id")) +
                                       " sc=" + getNull(out.get("sc.Id")) +
                                       " sd=" + getNull(out.get("sd.Id")));
                }
                 */
                Assert.AreEqual(9, events.Length);

                for (var j = 0; j < epls.Length; j++) {
                    try {
                        runtime.DeploymentService.Undeploy(stmts[j].DeploymentId);
                    }
                    catch (EPUndeployException e) {
                        throw new EPException(e);
                    }
                }
            }

            runtime.Destroy();
        }

        private string GetNull(object value)
        {
            if (value == null) {
                return "-";
            }

            return value.ToString();
        }

        public class SenderRunnable : IRunnable
        {
            private readonly EPEventService runtime;
            private readonly int[] values;

            public SenderRunnable(
                EPEventService runtime,
                int[] values)
            {
                this.runtime = runtime;
                this.values = values;
            }

            public void Run()
            {
                for (var i = 0; i < values.Length; i++) {
                    runtime.SendEventBean(new SupportBean_S0(values[i]), "S0");
                }
            }
        }
    }
} // end of namespace