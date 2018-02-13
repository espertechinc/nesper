///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternStartLoop : RegressionExecution {
        /// <summary>
        /// Starting this statement fires an event and the listener starts a new statement (same expression) again,
        /// causing a loop. This listener limits to 10 - this is a smoke test.
        /// </summary>
        public override void Run(EPServiceProvider epService) {
            string patternExpr = "not " + typeof(SupportBean).FullName;
            EPStatement patternStmt = epService.EPAdministrator.CreatePattern(patternExpr);
            patternStmt.AddListener(new PatternUpdateListener(epService));
            patternStmt.Stop();
            patternStmt.Start();
        }
    
        class PatternUpdateListener : UpdateListener {
            private readonly EPServiceProvider epService;
    
            public PatternUpdateListener(EPServiceProvider epService) {
                this.epService = epService;
            }
    
            private int count = 0;
    
            public void Update(EventBean[] newEvents, EventBean[] oldEvents) {
                Log.Warn(".update");
    
                if (count < 10) {
                    count++;
                    string patternExpr = "not " + typeof(SupportBean).FullName;
                    EPStatement patternStmt = epService.EPAdministrator.CreatePattern(patternExpr);
                    patternStmt.AddListener(this);
                    patternStmt.Stop();
                    patternStmt.Start();
                }
            }
    
            public int GetCount() {
                return count;
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
