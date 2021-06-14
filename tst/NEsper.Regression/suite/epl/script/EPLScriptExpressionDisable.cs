///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.script
{
    public class EPLScriptExpressionDisable : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TryInvalidCompile(
                env,
                "expression js:abc [ bla; ] select abc() from SupportBean",
                "Failed to validate select-clause expression 'abc()': Script compilation has been disabled by configuration");
        }
    }
} // end of namespace