///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowProcessingOrder
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithDispatchBackQueue(execs);
            WithOrderedDeleteAndSelect(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOrderedDeleteAndSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOrderedDeleteAndSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithDispatchBackQueue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                execs.Add(new InfraDispatchBackQueue(rep));
            }

            return execs;
        }

        internal class InfraDispatchBackQueue : RegressionExecution
        {
            private readonly EventRepresentationChoice eventRepresentationEnum;

            public InfraDispatchBackQueue(EventRepresentationChoice eventRepresentationEnum)
            {
                this.eventRepresentationEnum = eventRepresentationEnum;
            }

            public void Run(RegressionEnvironment env)
            {
                string epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedStartValueEvent>() +
                             " create schema StartValueEvent as (dummy string);\n";
                epl += eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTestForwardEvent>() +
                       " create schema TestForwardEvent as (prop1 string);\n";
                epl += eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTestInputEvent>() +
                       " create schema TestInputEvent as (dummy string);\n";
                epl += "insert into TestForwardEvent select'V1' as prop1 from TestInputEvent;\n";
                epl += eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNamedWin>() +
                       " create window NamedWin#unique(prop1) (prop1 string, prop2 string);\n";
                epl += "insert into NamedWin select 'V1' as prop1, 'O1' as prop2 from StartValueEvent;\n";
                epl += "on TestForwardEvent update NamedWin as work set prop2 = 'U1' where work.prop1 = 'V1';\n";
                epl += "@Name('select') select irstream prop1, prop2 from NamedWin;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("select");

                var fields = new[] {"prop1", "prop2"};
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {"dummyValue"}, "StartValueEvent");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    env.SendEventMap(new Dictionary<string, object>(), "StartValueEvent");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    env.EventService.SendEventAvro(
                        new GenericRecord(
                            SchemaBuilder.Record("soemthing")),
                        "StartValueEvent");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    env.EventService.SendEventJson("{}", "StartValueEvent");
                }
                else {
                    Assert.Fail();
                }

                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"V1", "O1"});

                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {"dummyValue"}, "TestInputEvent");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    env.SendEventMap(new Dictionary<string, object>(), "TestInputEvent");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    env.EventService.SendEventAvro(
                        new GenericRecord(
                            SchemaBuilder.Record("soemthing")),
                        "TestInputEvent");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    env.EventService.SendEventJson("{}", "TestInputEvent");
                }
                else {
                    Assert.Fail();
                }

                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastOldData[0],
                    fields,
                    new object[] {"V1", "O1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {"V1", "U1"});
                env.UndeployAll();
            }
        }

        internal class InfraOrderedDeleteAndSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#lastevent as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 7;\n" +
                          "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 5;\n" +
                          "on MyWindow e insert into ResultStream select e.* from MyWindow;\n" +
                          "@Name('s0') select * from ResultStream;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 7));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 8));
                Assert.AreEqual(env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"), "E2");

                env.SendEventBean(new SupportBean("E3", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E4", 6));
                Assert.AreEqual(env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"), "E4");

                env.UndeployAll();
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedStartValueEvent
        {
            public string dummy;
        }

        [Serializable]
        public class MyLocalJsonProvidedTestForwardEvent
        {
            public string prop1;
        }

        [Serializable]
        public class MyLocalJsonProvidedTestInputEvent
        {
            public string dummy;
        }

        [Serializable]
        public class MyLocalJsonProvidedNamedWin
        {
            public string prop1;
            public string prop2;
        }
    }
} // end of namespace