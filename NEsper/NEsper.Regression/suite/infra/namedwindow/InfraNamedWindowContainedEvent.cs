///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowContainedEvent : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            string epl;
            var path = new RegressionPath();
            env.CompileDeploy("create window BookWindow#time(30) as BookDesc", path);

            epl = "select * from SupportBean unIdirectional, BookWindow[reviews]";
            TryInvalidCompile(
                env,
                path,
                epl,
                "Failed to valIdate named window use in join, contained-event is only allowed for named windows when marked as unIdirectional");

            epl = "select *, (select * from BookWindow[reviews] where sb.TheString = comment) " +
                  "from SupportBean sb";
            TryInvalidCompile(
                env,
                path,
                epl,
                "Failed to plan subquery number 1 querying BookWindow: Failed to valIdate named window use in subquery, contained-event is only allowed for named windows when not correlated ");

            env.UndeployAll();
        }
    }
} // end of namespace