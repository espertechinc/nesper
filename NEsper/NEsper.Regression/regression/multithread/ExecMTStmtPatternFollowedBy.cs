///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTStmtPatternFollowedBy : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider epService) {
            RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY);
            RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE);
        }
    
        private void RunAssertionPatternFollowedBy(ConfigurationEngineDefaults.FilterServiceProfile profile) {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            string engineUri = this.GetType().Name + "_" + profile;
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, engineUri, config);
            epService.Initialize();
    
            string[] epls = {
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=0)->sb=S0(id=1)) or (sc=S0(id=1)->sd=S0(id=0))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=1)->sb=S0(id=2)) or (sc=S0(id=2)->sd=S0(id=1))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=2)->sb=S0(id=3)) or (sc=S0(id=3)->sd=S0(id=2))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=3)->sb=S0(id=4)) or (sc=S0(id=4)->sd=S0(id=3))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=4)->sb=S0(id=5)) or (sc=S0(id=5)->sd=S0(id=4))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=5)->sb=S0(id=6)) or (sc=S0(id=6)->sd=S0(id=5))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=6)->sb=S0(id=7)) or (sc=S0(id=7)->sd=S0(id=6))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=7)->sb=S0(id=8)) or (sc=S0(id=8)->sd=S0(id=7))]",
                    "select sa.id,sb.id,sc.id,sd.id from pattern [(sa=S0(id=8)->sb=S0(id=9)) or (sc=S0(id=9)->sd=S0(id=8))]"
            };
    
            for (int i = 0; i < 20; i++) {
                Log.Info("i=" + i);
                var listener = new SupportMTUpdateListener();
                var stmts = new EPStatement[epls.Length];
                for (int j = 0; j < epls.Length; j++) {
                    stmts[j] = epService.EPAdministrator.CreateEPL(epls[j]);
                    stmts[j].Events += listener.Update;
                }
    
                var threadOneValues = new int[]{0, 2, 4, 6, 8};
                var threadTwoValues = new int[]{1, 3, 5, 7, 9};
    
                var threadOne = new Thread(new SenderRunnable(epService.EPRuntime, threadOneValues).Run);
                var threadTwo = new Thread(new SenderRunnable(epService.EPRuntime, threadTwoValues).Run);
    
                threadOne.Start();
                threadTwo.Start();
                threadOne.Join();
                threadTwo.Join();
    
                EventBean[] events = listener.GetNewDataListFlattened();

#if INTERNAL_DEBUG
                for (int j = 0; j < events.Length; j++) {
                    EventBean @out = events[j];
                    Log.Info(" sa=" + GetNull(@out.Get("sa.id")) +
                                       " sb=" + GetNull(@out.Get("sb.id")) +
                                       " sc=" + GetNull(@out.Get("sc.id")) +
                                       " sd=" + GetNull(@out.Get("sd.id")));
                }
#endif
                Assert.AreEqual(9, events.Length);
    
                for (int j = 0; j < epls.Length; j++) {
                    stmts[j].Dispose();
                }
            }
    
            epService.Dispose();
        }
    
        public class SenderRunnable
        {
            private readonly EPRuntime _runtime;
            private readonly int[] _values;

            public int[] Values => _values;

            public SenderRunnable(EPRuntime runtime, int[] values) {
                this._runtime = runtime;
                this._values = values;
            }
    
            public void Run() {
                for (int i = 0; i < _values.Length; i++) {
                    _runtime.SendEvent(new SupportBean_S0(_values[i]));
                }
            }
        }
    
        private string GetNull(Object value) {
            if (value == null) {
                return "-";
            }
            return value.ToString();
        }
    }
} // end of namespace
