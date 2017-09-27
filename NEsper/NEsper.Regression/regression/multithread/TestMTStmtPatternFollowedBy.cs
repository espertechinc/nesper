///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    [TestFixture]
    public class TestMTStmtPatternFollowedBy
    {
        [SetUp]
        public void Setup()
        {
            EPServiceProviderManager.PurgeAllProviders();
        }

        [Test]
        public void TestPatternFollowedBy()
        {
            RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY);
            RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE);
        }

        private void RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile profile)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            String engineURI = GetType().Name + "_" + profile;
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(engineURI, config);
            epService.Initialize();

            String[] epls =
            {
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

            for (int i = 0; i < 20; i++)
            {
                var listener = new SupportMTUpdateListener();
                var stmts = new EPStatement[epls.Length];
                for (int j = 0; j < epls.Length; j++)
                {
                    stmts[j] = epService.EPAdministrator.CreateEPL(epls[j]);
                    stmts[j].Events += listener.Update;
                }

                var threadOneValues = new int[] { 0, 2, 4, 6, 8 };
                var threadTwoValues = new int[] { 1, 3, 5, 7, 9 };
                var threadArray = new[] {threadOneValues, threadTwoValues};

                Parallel.ForEach(
                    threadArray,
                    threadValues => ExecuteSender(epService.EPRuntime, threadValues));

                EventBean[] events = listener.GetNewDataListFlattened();
                Assert.AreEqual(9, events.Length);

                for (int j = 0; j < epls.Length; j++)
                {
                    stmts[j].Dispose();
                }
            }

            epService.Dispose();
        }

        private static void ExecuteSender(EPRuntime runtime, IEnumerable<int> values)
        {
            foreach (int value in values)
            {
                runtime.SendEvent(new SupportBean_S0(value));
            }
        }

        private String GetNull(Object value)
        {
            if (value == null)
            {
                return "-";
            }
            return value.ToString();
        }
    }
}
