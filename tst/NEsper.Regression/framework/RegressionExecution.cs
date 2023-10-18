///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.framework
{
    public interface RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return null;
        }

        string Name()
        {
            return GetType().Name;
        }

        public static string[] MilestoneStats()
        {
            return null;
        }

        void Run(RegressionEnvironment env);
    }
}