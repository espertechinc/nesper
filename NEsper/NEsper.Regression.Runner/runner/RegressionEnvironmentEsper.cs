///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

namespace com.espertech.esper.regressionrun.Runner
{
    public class RegressionEnvironmentEsper : RegressionEnvironmentBase
    {
        public RegressionEnvironmentEsper(Configuration configuration, EPRuntime runtime) 
            : base(configuration, runtime)
        {
        }

        public override RegressionEnvironment Milestone(long num)
        {
            return this;
        }

        public override RegressionEnvironment MilestoneInc(AtomicLong counter)
        {
            Milestone(counter.GetAndIncrement());
            return this;
        }

        public override RegressionEnvironment AddListener(string statementName)
        {
            EPStatement stmt = GetAssertStatement(statementName);
            stmt.AddListener(new SupportUpdateListener());
            return this;
        }

        public override bool IsHA
        {
            get => false;
        }

        public override bool IsHA_Releasing
        {
            get => false;
        }

        public override SupportListener ListenerNew()
        {
            return new SupportUpdateListener();
        }
    }
} // end of namespace