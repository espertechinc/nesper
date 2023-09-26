///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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

        private readonly string _className;
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
            _className = className;
            _spec = spec;
            _namespaceScope = namespaceScope;
            _numStreams = numStreams;
            _raw = raw;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            if (_spec.IsDirectAndSimple) {
                return null;
            }

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
                    new CodegenTypedParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref, false));
                var providerCtor = new CodegenCtor(
                    typeof(StmtClassForgeableOPVFactoryProvider),
                    includeDebugSymbols,
                    ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>();
                providerExplicitMembers.Add(
                    new CodegenTypedParam(typeof(OutputProcessViewFactory), MEMBERNAME_OPVFACTORY));
                if (_spec.IsCodeGenerated) {
                    // make factory and view both, assign to member
                    providerExplicitMembers.Add(
                        new CodegenTypedParam(typeof(StatementResultService), MEMBERNAME_STATEMENTRESULTSVC));
                    MakeOPVFactory(classScope, innerClasses, providerCtor, _className);
                    MakeOPV(
                        classScope,
                        innerClasses,
                        Collections.GetEmptyList<CodegenTypedParam>(),
                        providerCtor,
                        _className,
                        _spec,
                        _numStreams);
                }
                else {
                    // build factory from existing classes
                    var symbols = new SAIFFInitializeSymbol();
                    var init = providerCtor
                        .MakeChildWithScope(typeof(OutputProcessViewFactory), GetType(), symbols, classScope)
                        .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
                    _spec.ProvideCodegen(init, symbols, classScope);
                    providerCtor.Block.AssignMember(
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
                    CodegenClassType.OUTPUTPROCESSVIEWFACTORYPROVIDER,
                    typeof(OutputProcessViewFactoryProvider),
                    _className,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (Exception ex) {
                throw new EPException(
                    "Fatal exception during code-generation for " +
                    debugInformationProvider.Invoke() +
                    " : " +
                    ex.Message,
                    ex);
            }
        }

        private static void MakeOPVFactory(
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            CodegenCtor providerCtor,
            string providerClassName)
        {
            var makeViewMethod = CodegenMethod
                .MakeParentNode(
                    typeof(OutputProcessView),
                    typeof(StmtClassForgeableOPVFactoryProvider),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<ResultSetProcessor>(NAME_RESULTSETPROCESSOR)
                .AddParam<AgentInstanceContext>(NAME_AGENTINSTANCECONTEXT);
            makeViewMethod.Block.MethodReturn(
                CodegenExpressionBuilder.NewInstanceInner(
                    CLASSNAME_OUTPUTPROCESSVIEW,
                    Ref("o"),
                    MEMBER_RESULTSETPROCESSOR,
                    MEMBER_AGENTINSTANCECONTEXT));
            
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
            providerCtor.Block
                .AssignMember(
                    MEMBERNAME_OPVFACTORY,
                    NewInstanceInner(CLASSNAME_OUTPUTPROCESSVIEWFACTORY, Ref("this")))
                .AssignMember(
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
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam<EventBean[]>(NAME_NEWDATA)
                .AddParam<EventBean[]>(NAME_OLDDATA);
            if (numStreams == 1) {
                forge.UpdateCodegen(updateMethod, classScope);
            }
            else {
                updateMethod.Block.MethodThrowUnsupported();
            }

            // Process-Join-Result Method
            var processMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), NAME_NEWDATA)
                .AddParam(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), NAME_OLDDATA)
                .AddParam(typeof(ExprEvaluatorContext), "notApplicable");
            if (numStreams == 1) {
                processMethod.Block.MethodThrowUnsupported();
            }
            else {
                forge.ProcessCodegen(processMethod, classScope);
            }

            // Stop-Method (generates last as other methods may allocate members)
            var enumeratorMethod = CodegenMethod
                .MakeParentNode(
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
            var stopMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithOverride()
                .AddParam<AgentInstanceStopServices>("svc");
            
            // Terminate-Method (no action for generated code)
            var terminatedMethod = CodegenMethod.MakeParentNode(
                typeof(void),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);

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
                EmptyList<CodegenTypedParam>.Instance, 
                innerMethods,
                innerProperties);
            
            innerClasses.Add(innerClass);
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.OPVPROVIDER;
    }
} // end of namespace