///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.suite.epl.contained;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.wordexample;
using com.espertech.esper.regressionrun.Runner;

using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLContained
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestEPLContainedEventSimple()
        {
            RegressionRunner.Run(session, EPLContainedEventSimple.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventArray()
        {
            RegressionRunner.Run(session, EPLContainedEventArray.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventExample()
        {
            RegressionRunner.Run(session, EPLContainedEventExample.Executions(session.Container.ResourceManager()));
        }

        [Test]
        public void TestEPLContainedEventNested()
        {
            RegressionRunner.Run(session, EPLContainedEventNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventSplitExpr()
        {
            RegressionRunner.Run(session, EPLContainedEventSplitExpr.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new Type[] {
                typeof(SupportBean),
                typeof(OrderBean),
                typeof(BookDesc),
                typeof(SentenceEvent),
                typeof(SupportStringBeanWithArray),
                typeof(SupportBeanArrayCollMap),
                typeof(SupportObjectArrayEvent),
                typeof(SupportCollectionEvent),
                typeof(SupportResponseEvent),
                typeof(SupportAvroArrayEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            var innerMapDef = Collections.SingletonDataMap("p", typeof(string));
            configuration.Common.AddEventType("MyInnerMap", innerMapDef);
            var outerMapDef = Collections.SingletonDataMap("i", "MyInnerMap[]");
            configuration.Common.AddEventType("MyOuterMap", outerMapDef);

            var funcs = new [] { "splitSentence","splitSentenceBean","splitWord" };
            for (var i = 0; i < funcs.Length; i++) {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    string[] methods;
                    if (rep.IsObjectArrayEvent()) {
                        methods = new string[] {
                            "splitSentenceMethodReturnObjectArray",
                            "splitSentenceBeanMethodReturnObjectArray",
                            "splitWordMethodReturnObjectArray"
                        };
                    }
                    else if (rep.IsMapEvent()) {
                        methods = new string[] {
                            "splitSentenceMethodReturnMap",
                            "splitSentenceBeanMethodReturnMap",
                            "splitWordMethodReturnMap"
                        };
                    }
                    else if (rep.IsAvroEvent()) {
                        methods = new string[] {
                            "splitSentenceMethodReturnAvro",
                            "splitSentenceBeanMethodReturnAvro",
                            "splitWordMethodReturnAvro"
                        };
                    }
                    else {
                        throw new IllegalStateException("Unrecognized enum " + rep);
                    }

                    configuration.Compiler.AddPlugInSingleRowFunction(
                        funcs[i] + "_" + rep.GetName(),
                        typeof(EPLContainedEventSplitExpr),
                        methods[i]);
                }
            }

            var config = new ConfigurationCommonEventTypeXMLDOM();
            var resourceManager = configuration.ResourceManager;
            config.SchemaResource = resourceManager.ResolveResourceURL("regression/mediaOrderSchema.xsd").ToString();
            config.RootElementName = "MediaOrder";
            configuration.Common.AddEventType("MediaOrder", config);
            configuration.Common.AddEventType("Cancel", config);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction(
                "invalidSentence",
                typeof(EPLContainedEventSplitExpr),
                "InvalidSentenceMethod");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "mySplitUDFReturnEventBeanArray",
                typeof(EPLContainedEventSplitExpr),
                "MySplitUDFReturnEventBeanArray");
        }
    }
} // end of namespace