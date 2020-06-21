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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StmtClassForgeableOPVFactoryProvider : StmtClassForgeable
    {
        private const string MEMBERNAME_OPVFACTORY = "opvFactory";
        private const string CLASSNAME_OUTPUTPROCESSVIEWFACTORY = "OPVFactory";
        private const string CLASSNAME_OUTPUTPROCESSVIEW = "OPV";
        private const string MEMBERNAME_STATEMENTRESULTSVC = "statementResultService";

        private readonly int _numStreams;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly StatementRawInfo _raw;

        private readonly OutputProcessViewFactoryForge _spec;

        public StmtClassForgeableOPVFactoryProvider(
            string className,
            OutputProcessViewFactoryForge spec,
            CodegenNamespaceScope namespaceScope,
            int numStreams,
            StatementRawInfo raw)
        {
            ClassName = className;
            _spec = spec;
            _namespaceScope = namespaceScope;
            _numStreams = numStreams;
            _raw = raw;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            Supplier<string> debugInformationProvider = () => {
                var writer = new StringWriter();
                _raw.AppendCodeDebugInfo(writer);
                writer.Write(" output-processor ");
                writer.Write(_spec.GetType().FullName);
                return writer.ToString();
            };

            try {
                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                // build ctor
                IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
                ctorParms.Add(
                    new CodegenTypedParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref,
                        false));
                ctorParms.Add(
                    new CodegenTypedParam(
                        _namespaceScope.FieldsClassName,
                        null,
                        "statementFields",
                        true,
                        false));

                var providerCtor = new CodegenCtor(
                    typeof(StmtClassForgeableOPVFactoryProvider),
                    ClassName,
                    includeDebugSymbols,
                    ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, ClassName);
                var providerExplicitMembers = new List<CodegenTypedParam>();
                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(StatementResultService), MEMBERNAME_STATEMENTRESULTSVC));
                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(OutputProcessViewFactory), MEMBERNAME_OPVFACTORY));

                if (_spec.IsCodeGenerated) {
                    // make factory and view both, assign to member
                    MakeOPVFactory(classScope, innerClasses, providerExplicitMembers, providerCtor, ClassName);
                    MakeOPV(
                        classScope,
                        innerClasses,
                        Collections.GetEmptyList<CodegenTypedParam>(),
                        providerCtor,
                        ClassName,
                        _spec,
                        _numStreams);
                }
                else {
                    // build factory from existing classes
                    var symbols = new SAIFFInitializeSymbol();
                    var init = providerCtor
                        .MakeChildWithScope(typeof(OutputProcessViewFactory), GetType(), symbols, classScope)
                        .AddParam(
                            typeof(EPStatementInitServices),
                            EPStatementInitServicesConstants.REF.Ref);
                    _spec.ProvideCodegen(init, symbols, classScope);
                    providerCtor.Block.AssignRef(
                        MEMBERNAME_OPVFACTORY,
                        LocalMethod(init, EPStatementInitServicesConstants.REF));
                }

                // make get-factory method
                var factoryMethodGetter = CodegenProperty.MakePropertyNode(
                    typeof(OutputProcessViewFactory),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                factoryMethodGetter.GetterBlock.BlockReturn(Ref(MEMBERNAME_OPVFACTORY));

                var properties = new CodegenClassProperties();
                var methods = new CodegenClassMethods();
                CodegenStackGenerator.RecursiveBuildStack(
                    providerCtor,
                    "ctor",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    factoryMethodGetter,
                    "OutputProcessViewFactory",
                    methods,
                    properties);

                // render and compile
                return new CodegenClass(
                    typeof(OutputProcessViewFactoryProvider),
                    _namespaceScope.Namespace,
                    ClassName,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (Exception t) {
                throw new EPException(
                    "Fatal exception during code-generation for " +
                    debugInformationProvider.Invoke() +
                    " : " +
                    t.Message,
                    t);
            }
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.OPVPROVIDER;

        private static void MakeOPVFactory(
            CodegenClassScope classScope,
            ICollection<CodegenInnerClass> innerClasses,
            ICollection<CodegenTypedParam> providerExplicitMembers,
            CodegenCtor providerCtor,
            string providerClassName)
        {
            var makeViewMethod = CodegenMethod.MakeMethod(
                    typeof(OutputProcessView),
                    typeof(StmtClassForgeableOPVFactoryProvider),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(ResultSetProcessor), NAME_RESULTSETPROCESSOR)
                .AddParam(typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            makeViewMethod.Block.MethodReturn(
                NewInstance(CLASSNAME_OUTPUTPROCESSVIEW, Ref("o"), MEMBER_RESULTSETPROCESSOR, REF_AGENTINSTANCECONTEXT));
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();

            CodegenStackGenerator.RecursiveBuildStack(makeViewMethod, "MakeView", methods, properties);

            var ctorParams = Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(StmtClassForgeableOPVFactoryProvider), classScope, ctorParams);

            var innerClass = new CodegenInnerClass(
                CLASSNAME_OUTPUTPROCESSVIEWFACTORY,
                typeof(OutputProcessViewFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods,
                properties);
            innerClasses.Add(innerClass);

            providerCtor.Block.AssignRef(
                    MEMBERNAME_OPVFACTORY,
                    NewInstance(CLASSNAME_OUTPUTPROCESSVIEWFACTORY, Ref("this")))
                .AssignRef(
                    MEMBERNAME_STATEMENTRESULTSVC,
                    ExprDotName(
                        EPStatementInitServicesConstants.REF,
                        EPStatementInitServicesConstants.STATEMENTRESULTSERVICE));
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
            var serviceCtor = new CodegenCtor(typeof(StmtClassForgeableOPVFactoryProvider), classScope, ctorParams);

            // Get-Result-Type Method
            var eventTypeGetter = CodegenProperty
                .MakePropertyNode(typeof(EventType), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride();
            eventTypeGetter.GetterBlock.BlockReturn(ExprDotName(Ref(NAME_RESULTSETPROCESSOR), "ResultEventType"));

            // Process-View-Result Method
            var updateMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam(typeof(EventBean[]), NAME_NEWDATA)
                .AddParam(typeof(EventBean[]), NAME_OLDDATA);
            if (numStreams == 1) {
                forge.UpdateCodegen(updateMethod, classScope);
            }
            else {
                updateMethod.Block.MethodThrowUnsupported();
            }

            // Process-Join-Result Method
            var processMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), NAME_NEWDATA)
                .AddParam(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), NAME_OLDDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    "notApplicable");
            if (numStreams == 1) {
                processMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessCodegen(processMethod, classScope);
            }

            // Stop-Method (generates last as other methods may allocate members)
            var enumeratorMethod = CodegenMethod
                .MakeMethod(
                    typeof(IEnumerator<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .WithOverride();
            forge.EnumeratorCodegen(enumeratorMethod, classScope);

            // NumChangesetRows (always zero for generated code)
            var numChangesetRowsProp = CodegenProperty
                .MakePropertyNode(typeof(int), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride();
            numChangesetRowsProp.GetterBlock.BlockReturn(Constant(0));

            // OptionalOutputCondition (always null for generated code)
            var optionalOutputConditionProp = CodegenProperty
                .MakePropertyNode(typeof(OutputCondition), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride();
            optionalOutputConditionProp.GetterBlock.BlockReturn(ConstantNull());

            // Stop-Method (no action for generated code)
            CodegenMethod stopMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam(typeof(AgentInstanceStopServices), "svc");

            // Terminate-Method (no action for generated code)
            CodegenMethod terminatedMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride();

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(
                eventTypeGetter,
                "EventType",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                updateMethod,
                "Update",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                processMethod,
                "Process",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                enumeratorMethod,
                "GetEnumerator",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                numChangesetRowsProp,
                "NumChangesetRows",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                optionalOutputConditionProp,
                "OptionalOutputCondition",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                stopMethod,
                "Stop",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                terminatedMethod,
                "Terminated",
                innerMethods,
                innerProperties);


            var innerClass = new CodegenInnerClass(
                CLASSNAME_OUTPUTPROCESSVIEW,
                typeof(OutputProcessView),
                serviceCtor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                innerMethods,
                innerProperties);
            innerClasses.Add(innerClass);
        }
    }
} // end of namespace