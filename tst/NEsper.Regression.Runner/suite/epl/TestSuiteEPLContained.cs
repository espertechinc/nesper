///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLContained : AbstractTestBase
    {
        public TestSuiteEPLContained() : base(Configure)
        {
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventSimple()
        {
            RegressionRunner.Run(_session, EPLContainedEventSimple.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventArray()
        {
            RegressionRunner.Run(_session, EPLContainedEventArray.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventExample()
        {
            RegressionRunner.Run(_session, EPLContainedEventExample.Executions(_session.Container.ResourceManager()));
        }

        [Test, RunInApplicationDomain]
        public void TestEPLContainedEventNested()
        {
            RegressionRunner.Run(_session, EPLContainedEventNested.Executions());
        }

        /// <summary>
        /// Auto-test(s): EPLContainedEventSplitExpr
        /// <code>
        /// RegressionRunner.Run(_session, EPLContainedEventSplitExpr.Executions());
        /// </code>
        /// </summary>

        public class TestEPLContainedEventSplitExpr : AbstractTestBase
        {
            public TestEPLContainedEventSplitExpr() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSingleRowSplitAndType() => RegressionRunner.Run(_session, EPLContainedEventSplitExpr.WithSingleRowSplitAndType());

            [Test, RunInApplicationDomain]
            public void WithSplitExprReturnsEventBean() => RegressionRunner.Run(_session, EPLContainedEventSplitExpr.WithSplitExprReturnsEventBean());

            [Test, RunInApplicationDomain]
            public void WithScriptContextValue() => RegressionRunner.Run(_session, EPLContainedEventSplitExpr.WithScriptContextValue());
        }
        
        public static void Configure(Configuration configuration)
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
                typeof(SupportAvroArrayEvent),
                typeof(SupportJsonArrayEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            var innerMapDef = Collections.SingletonDataMap("p", typeof(string));
            configuration.Common.AddEventType("MyInnerMap", innerMapDef);
            var outerMapDef = Collections.SingletonDataMap("i", "MyInnerMap[]");
            configuration.Common.AddEventType("MyOuterMap", outerMapDef);

            var funcs = new [] { "SplitSentence","SplitSentenceBean","SplitWord" };
            for (var i = 0; i < funcs.Length; i++) {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    string[] methods;
                    if (rep.IsObjectArrayEvent()) {
                        methods = new string[] {
                            "SplitSentenceMethodReturnObjectArray",
                            "SplitSentenceBeanMethodReturnObjectArray",
                            "SplitWordMethodReturnObjectArray"
                        };
                    }
                    else if (rep.IsMapEvent()) {
                        methods = new string[] {
                            "SplitSentenceMethodReturnMap",
                            "SplitSentenceBeanMethodReturnMap",
                            "SplitWordMethodReturnMap"
                        };
                    }
                    else if (rep.IsAvroEvent()) {
                        methods = new string[] {
                            "SplitSentenceMethodReturnAvro",
                            "SplitSentenceBeanMethodReturnAvro",
                            "SplitWordMethodReturnAvro"
                        };
                    }
                    else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                        methods = new string[] {
                            "SplitSentenceMethodReturnJson",
                            "SplitSentenceBeanMethodReturnJson",
                            "SplitWordMethodReturnJson"
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

            configuration.Compiler.ByteCode.IsAllowSubscriber =true;
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