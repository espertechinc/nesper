///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.client.runtime;
using com.espertech.esper.regressionlib.suite.@event.infra;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NEsper.Avro.Extensions;
using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientRuntime : AbstractTestBase
    {
        public TestSuiteClientRuntime() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[]
                     {
                         typeof(SupportBean),
                         typeof(SupportBeanComplexProps),
                         typeof(SupportBeanWithEnum),
                         typeof(SupportMarketDataBean),
                         typeof(SupportMarkerInterface),
                         typeof(SupportBean_A),
                         typeof(SupportBean_B),
                         typeof(SupportBean_C),
                         typeof(SupportBean_D),
                         typeof(SupportBean_S0)
                     }

                    )
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            configuration.Common.AddEventType(ClientRuntimeListener.MAP_TYPENAME,
                Collections.SingletonDataMap("Ident", "string"));
            configuration.Common.AddEventType(ClientRuntimeListener.OA_TYPENAME, new[] { "Ident" },
                new[] { typeof(string) });
            configuration.Common.AddEventType(ClientRuntimeListener.BEAN_TYPENAME,
                typeof(ClientRuntimeListener.RoutedBeanEvent));
            configuration.Common.AddImportNamespace(typeof(MyAnnotationNestedAttribute));
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" + "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"Ident\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" + "\t</xs:element>\n" + "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(ClientRuntimeListener.XML_TYPENAME, eventTypeMeta);
            Schema avroSchema = SchemaBuilder.Record(EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                TypeBuilder.Field("Ident",
                    TypeBuilder.StringType(TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            configuration.Common.AddEventTypeAvro(ClientRuntimeListener.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
            configuration.Common.AddImportType(typeof(MyAnnotationValueEnumAttribute));
            configuration.Common.AddImportNamespace(typeof(MyAnnotationNestableValuesAttribute));
            configuration.Common.AddAnnotationImportType(typeof(SupportEnum));
            configuration.Compiler.AddPlugInAggregationFunctionForge("myinvalidagg",
                typeof(SupportInvalidAggregationFunctionForge));
            // add service (not serializable, transient configuration)
            var transients = new Dictionary<string, object>();
            transients.Put(ClientRuntimeItself.TEST_SERVICE_NAME,
                new ClientRuntimeItself.MyLocalService(ClientRuntimeItself.TEST_SECRET_VALUE));
            configuration.Common.TransientConfiguration = transients;
            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
            configuration.Runtime.Execution.IsPrioritized = true;
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeStatementAnnotation
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeStatementAnnotation : AbstractTestBase
        {
            public TestClientRuntimeStatementAnnotation() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRecursive() =>
                RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithRecursive());

            [Test, RunInApplicationDomain]
            public void WithSpecificImport() =>
                RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithSpecificImport());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithAppNested() =>
                RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithAppNested());

            [Test, RunInApplicationDomain]
            public void WithAppSimple() =>
                RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithAppSimple());

            [Test, RunInApplicationDomain]
            public void WithBuiltin() => RegressionRunner.Run(_session, ClientRuntimeStatementAnnotation.WithBuiltin());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeEPStatement
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeEPStatement.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeEPStatement : AbstractTestBase
        {
            public TestClientRuntimeEPStatement() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAlreadyDestroyed() =>
                RegressionRunner.Run(_session, ClientRuntimeEPStatement.WithAlreadyDestroyed());

            [Test, RunInApplicationDomain]
            public void WithListenerWReplay() =>
                RegressionRunner.Run(_session, ClientRuntimeEPStatement.WithListenerWReplay());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeExceptionHandler
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeExceptionHandler.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeExceptionHandler : AbstractTestBase
        {
            public TestClientRuntimeExceptionHandler() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withg() =>
                RegressionRunner.Run(_session, ClientRuntimeExceptionHandler.WithRuntimeExHandlerInvalidAgg());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeItself
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeItself.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeItself : AbstractTestBase
        {
            public TestClientRuntimeItself() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWrongCompileMethod() =>
                RegressionRunner.Run(_session, ClientRuntimeItself.WithWrongCompileMethod());

            [Test, RunInApplicationDomain]
            public void WithSPIBeanAnonymousType() =>
                RegressionRunner.Run(_session, ClientRuntimeItself.WithSPIBeanAnonymousType());

            [Test, RunInApplicationDomain]
            public void WithSPIStatementSelection() =>
                RegressionRunner.Run(_session, ClientRuntimeItself.WithSPIStatementSelection());

            [Test, RunInApplicationDomain]
            public void WithSPICompileReflective() =>
                RegressionRunner.Run(_session, ClientRuntimeItself.WithSPICompileReflective());

            [Test, RunInApplicationDomain]
            public void WithItselfTransientConfiguration() =>
                RegressionRunner.Run(_session, ClientRuntimeItself.WithItselfTransientConfiguration());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeListener
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeListener.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeListener : AbstractTestBase
        {
            public TestClientRuntimeListener() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withe() => RegressionRunner.Run(_session, ClientRuntimeListener.Withe());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimePriorityAndDropInstructions
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimePriorityAndDropInstructions.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimePriorityAndDropInstructions : AbstractTestBase
        {
            public TestClientRuntimePriorityAndDropInstructions() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAddRemoveStmts() => RegressionRunner.Run(_session,
                ClientRuntimePriorityAndDropInstructions.WithAddRemoveStmts());

            [Test, RunInApplicationDomain]
            public void WithPriority() =>
                RegressionRunner.Run(_session, ClientRuntimePriorityAndDropInstructions.WithPriority());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowDrop() => RegressionRunner.Run(_session,
                ClientRuntimePriorityAndDropInstructions.WithNamedWindowDrop());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowPriority() => RegressionRunner.Run(_session,
                ClientRuntimePriorityAndDropInstructions.WithNamedWindowPriority());

            [Test, RunInApplicationDomain]
            public void WithSchedulingDrop() => RegressionRunner.Run(_session,
                ClientRuntimePriorityAndDropInstructions.WithSchedulingDrop());

            [Test, RunInApplicationDomain]
            public void WithSchedulingPriority() => RegressionRunner.Run(_session,
                ClientRuntimePriorityAndDropInstructions.WithSchedulingPriority());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeRuntimeProvider
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeRuntimeProvider.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeRuntimeProvider : AbstractTestBase
        {
            public TestClientRuntimeRuntimeProvider() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withk() => RegressionRunner.Run(_session, ClientRuntimeRuntimeProvider.Withk());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeSolutionPatternPortScan
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeSolutionPatternPortScan.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeSolutionPatternPortScan : AbstractTestBase
        {
            public TestClientRuntimeSolutionPatternPortScan() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFallsUnderThreshold() => RegressionRunner.Run(_session,
                ClientRuntimeSolutionPatternPortScan.WithFallsUnderThreshold());

            [Test, RunInApplicationDomain]
            public void WithKeepAlerting() =>
                RegressionRunner.Run(_session, ClientRuntimeSolutionPatternPortScan.WithKeepAlerting());

            [Test, RunInApplicationDomain]
            public void WithPrimarySuccess() =>
                RegressionRunner.Run(_session, ClientRuntimeSolutionPatternPortScan.WithPrimarySuccess());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeStatementName
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeStatementName.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeStatementName : AbstractTestBase
        {
            public TestClientRuntimeStatementName() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSingleModuleTwoStatementsNoDep() => RegressionRunner.Run(_session,
                ClientRuntimeStatementName.WithSingleModuleTwoStatementsNoDep());

            [Test, RunInApplicationDomain]
            public void WithStatementAllowNameDuplicate() => RegressionRunner.Run(_session,
                ClientRuntimeStatementName.WithStatementAllowNameDuplicate());

            [Test]
            [RunInApplicationDomain]
            public void WithStatementNameUnassigned() =>
                RegressionRunner.Run(_session, ClientRuntimeStatementName.WithStatementNameUnassigned());

            [Test]
            [RunInApplicationDomain]
            public void WithStatementNameRuntimeResolverDuplicate() => RegressionRunner.Run(_session,
                ClientRuntimeStatementName.WithStatementNameRuntimeResolverDuplicate());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeSubscriber
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeSubscriber.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeSubscriber : AbstractTestBase
        {
            public TestClientRuntimeSubscriber() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            [Parallelizable(ParallelScope.None)]
            public void WithPerformanceSynthetic() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithPerformanceSynthetic());

            [Test, RunInApplicationDomain]
            [Parallelizable(ParallelScope.None)]
            public void WithPerformanceSyntheticUndelivered() => RegressionRunner.Run(_session,
                ClientRuntimeSubscriber.WithPerformanceSyntheticUndelivered());

            [Test, RunInApplicationDomain]
            public void WithSimpleSelectUpdateOnly() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithSimpleSelectUpdateOnly());

            [Test, RunInApplicationDomain]
            public void WithVariables() => RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithVariables());

            [Test, RunInApplicationDomain]
            public void WithStartStopStatement() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithStartStopStatement());

            [Test, RunInApplicationDomain]
            public void WithNamedWindow() => RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithInvocationTargetEx() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithInvocationTargetEx());

            [Test, RunInApplicationDomain]
            public void WithBindWildcardJoin() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithBindWildcardJoin());

            [Test, RunInApplicationDomain]
            public void WithSubscriberAndListener() =>
                RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithSubscriberAndListener());

            [Test]
            public void WithBindings() => RegressionRunner.Run(_session, ClientRuntimeSubscriber.WithBindings());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeTimeControl
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeTimeControl.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeTimeControl : AbstractTestBase
        {
            public TestClientRuntimeTimeControl() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNextScheduledTime() =>
                RegressionRunner.Run(_session, ClientRuntimeTimeControl.WithNextScheduledTime());

            [Test, RunInApplicationDomain]
            public void WithSendTimeSpan() =>
                RegressionRunner.Run(_session, ClientRuntimeTimeControl.WithSendTimeSpan());
        }

        /// <summary>
        /// Auto-test(s): ClientRuntimeUnmatchedListener
        /// <code>
        /// RegressionRunner.Run(_session, ClientRuntimeUnmatchedListener.Executions());
        /// </code>
        /// </summary>
        public class TestClientRuntimeUnmatchedListener : AbstractTestBase
        {
            public TestClientRuntimeUnmatchedListener() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInsertInto() =>
                RegressionRunner.Run(_session, ClientRuntimeUnmatchedListener.WithInsertInto());

            [Test, RunInApplicationDomain]
            public void WithCreateStatement() =>
                RegressionRunner.Run(_session, ClientRuntimeUnmatchedListener.WithCreateStatement());

            [Test, RunInApplicationDomain]
            public void WithSendEvent() =>
                RegressionRunner.Run(_session, ClientRuntimeUnmatchedListener.WithSendEvent());
        }
    }
} // end of namespace
