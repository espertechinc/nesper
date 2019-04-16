///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StmtClassForgableOPVFactoryProvider : StmtClassForgable
    {
        private const string MEMBERNAME_OPVFACTORY = "opvFactory";
        private const string CLASSNAME_OUTPUTPROCESSVIEWFACTORY = "OPVFactory";
        private const string CLASSNAME_OUTPUTPROCESSVIEW = "OPV";
        private const string MEMBERNAME_STATEMENTRESULTSVC = "statementResultService";

        private readonly int numStreams;
        private readonly CodegenPackageScope packageScope;
        private readonly StatementRawInfo raw;
        private readonly OutputProcessViewFactoryForge spec;

        public StmtClassForgableOPVFactoryProvider(
            string className,
            OutputProcessViewFactoryForge spec,
            CodegenPackageScope packageScope,
            int numStreams,
            StatementRawInfo raw)
        {
            ClassName = className;
            this.spec = spec;
            this.packageScope = packageScope;
            this.numStreams = numStreams;
            this.raw = raw;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            Supplier<string> debugInformationProvider = () => {
                var writer = new StringWriter();
                raw.AppendCodeDebugInfo(writer);
                writer.Write(" output-processor ");
                writer.Write(spec.GetType().FullName);
                return writer.ToString();
            };

            try {
                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                // build ctor
                IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
                ctorParms.Add(
                    new CodegenTypedParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref, false));
                var providerCtor = new CodegenCtor(
                    typeof(StmtClassForgableOPVFactoryProvider), includeDebugSymbols, ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, packageScope, ClassName);
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>();
                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(StatementResultService), MEMBERNAME_STATEMENTRESULTSVC));
                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(OutputProcessViewFactory), MEMBERNAME_OPVFACTORY));

                if (spec.IsCodeGenerated) {
                    // make factory and view both, assign to member
                    MakeOPVFactory(classScope, innerClasses, providerExplicitMembers, providerCtor, ClassName);
                    MakeOPV(
                        classScope, innerClasses, Collections.GetEmptyList<CodegenTypedParam>(),
                        providerCtor, ClassName, spec, numStreams);
                }
                else {
                    // build factory from existing classes
                    var symbols = new SAIFFInitializeSymbol();
                    var init = providerCtor
                        .MakeChildWithScope(typeof(OutputProcessViewFactory), GetType(), symbols, classScope).AddParam(
                            typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                    spec.ProvideCodegen(init, symbols, classScope);
                    providerCtor.Block.AssignRef(MEMBERNAME_OPVFACTORY, LocalMethod(init, EPStatementInitServicesConstants.REF));
                }

                // make get-factory method
                var getFactoryMethod = CodegenMethod.MakeParentNode(
                    typeof(OutputProcessViewFactory), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
                getFactoryMethod.Block.MethodReturn(Ref(MEMBERNAME_OPVFACTORY));

                var methods = new CodegenClassMethods();
                CodegenStackGenerator.RecursiveBuildStack(providerCtor, "ctor", methods);
                CodegenStackGenerator.RecursiveBuildStack(getFactoryMethod, "getOutputProcessViewFactory", methods);

                // render and compile
                return new CodegenClass(
                    typeof(OutputProcessViewFactoryProvider), packageScope.PackageName, ClassName, classScope,
                    providerExplicitMembers, providerCtor, methods, innerClasses);
            }
            catch (Exception t) {
                throw new EPException(
                    "Fatal exception during code-generation for " + debugInformationProvider.Invoke() + " : " + t.Message,
                    t);
            }
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.OPVPROVIDER;

        private static void MakeOPVFactory(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> providerExplicitMembers,
            CodegenCtor providerCtor,
            string providerClassName)
        {
            var makeViewMethod = CodegenMethod.MakeParentNode(
                    typeof(OutputProcessView), typeof(StmtClassForgableOPVFactoryProvider),
                    CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ResultSetProcessor), NAME_RESULTSETPROCESSOR)
                .AddParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            makeViewMethod.Block.MethodReturn(
                NewInstance(CLASSNAME_OUTPUTPROCESSVIEW, Ref("o"), REF_RESULTSETPROCESSOR, REF_AGENTINSTANCECONTEXT));
            var methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(makeViewMethod, "makeView", methods);

            var ctorParams = Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(StmtClassForgableOPVFactoryProvider), classScope, ctorParams);

            var innerClass = new CodegenInnerClass(
                CLASSNAME_OUTPUTPROCESSVIEWFACTORY, typeof(OutputProcessViewFactory), ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods);
            innerClasses.Add(innerClass);

            providerCtor.Block.AssignRef(
                    MEMBERNAME_OPVFACTORY, NewInstance(CLASSNAME_OUTPUTPROCESSVIEWFACTORY, Ref("this")))
                .AssignRef(
                    MEMBERNAME_STATEMENTRESULTSVC,
                    ExprDotMethod(
                        EPStatementInitServicesConstants.REF,
                        EPStatementInitServicesConstants.GETSTATEMENTRESULTSERVICE));
        }

        private static void MakeOPV(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> factoryExplicitMembers,
            CodegenCtor factoryCtor,
            string classNameParent,
            OutputProcessViewFactoryForge forge,
            int numStreams)
        {
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(classNameParent, "o"));
            ctorParams.Add(new CodegenTypedParam(typeof(ResultSetProcessor), NAME_RESULTSETPROCESSOR));
            ctorParams.Add(new CodegenTypedParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT));

            // make ctor code
            var serviceCtor = new CodegenCtor(typeof(StmtClassForgableOPVFactoryProvider), classScope, ctorParams);

            // Get-Result-Type Method
            var getEventTypeMethod = CodegenMethod.MakeParentNode(
                typeof(EventType), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getEventTypeMethod.Block.MethodReturn(ExprDotMethod(Ref(NAME_RESULTSETPROCESSOR), "getResultEventType"));

            // Process-View-Result Method
            var updateMethod = CodegenMethod.MakeParentNode(
                    typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_NEWDATA).AddParam(typeof(EventBean[]), NAME_OLDDATA);
            if (numStreams == 1) {
                forge.UpdateCodegen(updateMethod, classScope);
            }
            else {
                updateMethod.Block.MethodThrowUnsupported();
            }

            // Process-Join-Result Method
            var processMethod = CodegenMethod.MakeParentNode(
                    typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ISet<object>), NAME_NEWDATA).AddParam(typeof(ISet<object>), NAME_OLDDATA).AddParam(
                    typeof(ExprEvaluatorContext), "not_applicable");
            if (numStreams == 1) {
                processMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessCodegen(processMethod, classScope);
            }

            // Stop-Method (generates last as other methods may allocate members)
            var iteratorMethod = CodegenMethod.MakeParentNode(
                typeof(IEnumerator<object>), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            forge.IteratorCodegen(iteratorMethod, classScope);

            // GetNumChangesetRows-Methods (always zero for generated code)
            var getNumChangesetRowsMethod = CodegenMethod.MakeParentNode(
                typeof(int), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getNumChangesetRowsMethod.Block.MethodReturn(Constant(0));

            // GetOptionalOutputCondition-Method (always null for generated code)
            var getOptionalOutputConditionMethod = CodegenMethod.MakeParentNode(
                typeof(OutputCondition), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getOptionalOutputConditionMethod.Block.MethodReturn(ConstantNull());

            // Stop-Method (no action for generated code)
            var stopMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AgentInstanceStopServices), "svc");

            // Terminate-Method (no action for generated code)
            var terminatedMethod = CodegenMethod.MakeParentNode(
                typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);

            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(getEventTypeMethod, "getEventType", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(updateMethod, "update", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(processMethod, "process", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(iteratorMethod, "iterator", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(getNumChangesetRowsMethod, "getNumChangesetRows", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getOptionalOutputConditionMethod, "getOptionalOutputCondition", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(stopMethod, "stop", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(terminatedMethod, "terminated", innerMethods);

            var innerClass = new CodegenInnerClass(
                CLASSNAME_OUTPUTPROCESSVIEW, typeof(OutputProcessView), serviceCtor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                innerMethods);
            innerClasses.Add(innerClass);
        }
    }
} // end of namespace