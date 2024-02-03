///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

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
            env.CompileDeploy("@public create window BookWindow#time(30) as BookDesc", path);

            epl = "select * from SupportBean unidirectional, BookWindow[Reviews]";
            env.TryInvalidCompile(
                path,
                epl,
                "Failed to validate named window use in join, contained-event is only allowed for named windows when marked as unidirectional");

            epl = "select *, (select * from BookWindow[Reviews] where sb.TheString = Comment) " +
                  "from SupportBean sb";
            env.TryInvalidCompile(
                path,
                epl,
                "Failed to plan subquery number 1 querying BookWindow: Failed to validate named window use in subquery, contained-event is only allowed for named windows when not correlated ");

            env.UndeployAll();
        }
    }
} // end of namespace