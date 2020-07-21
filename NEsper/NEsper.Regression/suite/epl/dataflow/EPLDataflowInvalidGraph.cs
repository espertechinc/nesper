///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.dataflow.SupportDataFlowAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowInvalidGraph
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowInvalidCompile());
            execs.Add(new EPLDataflowInvalidInstantiate());
            return execs;
        }

        internal class EPLDataflowInvalidCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid syntax
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph MySource -> select",
                    "Incorrect syntax near 'select' (a reserved keyword) at line 1 column 36 [");

                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph MySource -> myout",
                    "Incorrect syntax near end-of-input expecting a left curly bracket '{' but found EOF at line 1 column 41 [");

                // duplicate data flow name
                epl = "create dataflow MyGraph Emitter -> outstream<?> {};\n" +
                      "create dataflow MyGraph Emitter -> outstream<?> {};\n";
                TryInvalidCompile(env, epl, "A dataflow by name 'MyGraph' has already been declared [");

                // type not found
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph DefaultSupportSourceOp -> outstream<ABC> {}",
                    "Failed to find event type 'ABC'");

                // invalid schema (need not test all variants, same as create-schema)
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph create schema DUMMY com.mycompany.DUMMY, " +
                    "DefaultSupportSourceOp -> outstream<?> {}",
                    "Could not load class by name 'com.mycompany.DUMMY', please check imports");

                // can't find op
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph DummyOp {}",
                    "Failed to resolve forge class for operator 'DummyOp': Could not load class by name 'DummyOpForge', please check imports");

                // op is some other class
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph Random {}",
                    "Forge class for operator 'Random' does not implement interface 'DataFlowOperatorForge'");

                // input stream not found
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph DefaultSupportCaptureOp(nostream) {}",
                    "Input stream 'nostream' consumed by operator 'DefaultSupportCaptureOp' could not be found");

                // failed op factory
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph MyInvalidOp {}",
                    "Failed to obtain operator 'MyInvalidOp': Failed-Here");

                // inject properties: property not found
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph DefaultSupportCaptureOp {dummy: 1}",
                    "Failed to find writable property 'dummy' for class");

                // inject properties: property invalid type
                TryInvalidCompile(
                    env,
                    "create dataflow MyGraph MyTestOp {theString: 1}",
                    "Property 'theString' of class " +
                    typeof(MyTestOp).CleanName() +
                    " expects an System.String but receives a value of type System.Int32");

                // two incompatible input streams: different types
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                      "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                      "MyTestOp((out1, out2) as ABC) {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'MyTestOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out2' typed 'SupportBean_B'");

                // two incompatible input streams: one is wildcard
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<?> {}\n" +
                      "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                      "MyTestOp((out1, out2) as ABC) {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'MyTestOp' streams 'out1' and 'out2' have differing wildcard type information");

                // two incompatible input streams: underlying versus eventbean
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<Eventbean<SupportBean_B>> {}\n" +
                      "DefaultSupportSourceOp -> out2<SupportBean_B> {}\n" +
                      "MyTestOp((out1, out2) as ABC) {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'MyTestOp' streams 'out1' and 'out2' have differing underlying information");

                // output stream multiple type parameters
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<SupportBean_A, SupportBean_B> {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate operator 'DefaultSupportSourceOp': Multiple output types for a single stream 'out1' are not supported [");

                // same output stream declared twice
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<SupportBean_A>, out1<SupportBean_B> {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'DefaultSupportSourceOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");

                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<Eventbean<SupportBean_A>>, out1<Eventbean<SupportBean_B>> {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'DefaultSupportSourceOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");

                // two incompatible output streams: underlying versus eventbean
                epl = "create dataflow MyGraph " +
                      "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                      "DefaultSupportSourceOp -> out1<SupportBean_B> {}\n" +
                      "MyTestOp(out1) {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "For operator 'MyTestOp' stream 'out1' typed 'SupportBean_A' is not the same type as stream 'out1' typed 'SupportBean_B'");

                // same schema defined twice
                epl = "create dataflow MyGraph " +
                      "create schema ABC (c0 string), create schema ABC (c1 string), " +
                      "DefaultSupportSourceOp -> out1<SupportBean_A> {}";
                TryInvalidCompile(
                    env,
                    epl,
                    "Schema name 'ABC' is declared more then once [");
            }
        }

        internal class EPLDataflowInvalidInstantiate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // incompatible on-input method
                var epl = "@name('flow') create dataflow MyGraph " +
                          "DefaultSupportSourceOp -> out1<SupportBean_A> {}\n" +
                          "MySBInputOp(out1) {}";
                TryInvalidInstantiate(
                    env,
                    "MyGraph",
                    epl,
                    "Failed to instantiate data flow 'MyGraph': Failed to find OnInput method on for operator 'MySBInputOp#1(out1)' class " +
                    typeof(MySBInputOp).Name +
                    ", expected an OnInput method that takes any of {Object, Object[");
            }
        }

        public class MyInvalidOpForge : DataFlowOperatorForge
        {
            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                throw new EPException("Failed-Here");
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return ConstantNull();
            }
        }

        public class MyTestOp : DataFlowOperatorForge
        {
            [DataFlowOpParameter] private string theString;

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return ConstantNull();
            }
        }

        public class MySBInputOp : DataFlowOperatorForge,
            DataFlowOperatorFactory,
            DataFlowOperator
        {
            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MySBInputOp();
            }

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(MySBInputOp));
            }

            public void OnInput(SupportBean_B b)
            {
            }
        }
    }
} // end of namespace