///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonCreateSchema
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSpecialName(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonCreateSchemaInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSpecialName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonCreateSchemaSpecialName());
            return execs;
        }

        private class EventJsonCreateSchemaSpecialName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@public @buseventtype create json schema JsonEvent(`p q` string, ABC string, abc string, AbC string);\n" +
                        "@name('s0') select * from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson(
                    new JObject(
                        new JProperty("p q", "v1"),
                        new JProperty("ABC", "v2"),
                        new JProperty("abc", "v3"),
                        new JProperty("AbC", "v4")).ToString(),
                    "JsonEvent");
                env.AssertEventNew("s0", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("s0", iterator => AssertEvent(iterator.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    "p q,ABC,abc,AbC".SplitCsv(),
                    new object[] { "v1", "v2", "v3", "v4" });
            }
        }

        private class EventJsonCreateSchemaInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "create objectarray schema InnerEvent();\n create json schema JsonEvent(innervent InnerEvent);\n",
                    "Failed to validate event type 'InnerEvent', expected a Json or Map event type");

                env.TryInvalidCompile(
                    "create json schema InvalidDecl(int fieldname)",
					"Nestable type configuration encountered an unexpected property type name 'fieldname' for property 'System.Int32', expected System.Type or Map or the name of a previously-declared event type");

                env.TryInvalidCompile(
                    "create json schema InvalidDecl(comparable System.IComparable)",
                    "Unsupported type 'System.IComparable' for property 'comparable' (use @JsonSchemaField to declare additional information)");
            }
        }
    }
} // end of namespace