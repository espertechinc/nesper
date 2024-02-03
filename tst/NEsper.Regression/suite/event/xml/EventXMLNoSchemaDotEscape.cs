///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaDotEscape
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithPreconfig(execs);
            With(CreateSchema)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaDotEscapeCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaDotEscapePreconfig());
            return execs;
        }

        public class EventXMLNoSchemaDotEscapePreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "AEvent", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaDotEscapeCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='myroot')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            var stmt = "@name('s0') select a\\.b.c\\.d as val from " + eventTypeName;
            env.CompileDeploy(stmt, path).AddListener("s0");

            SendXMLEvent(env, "<myroot><a.b><c.d>value</c.d></a.b></myroot>", eventTypeName);

            env.AssertEqualsNew("s0", "val", "value");
            env.UndeployAll();
        }
    }
} // end of namespace