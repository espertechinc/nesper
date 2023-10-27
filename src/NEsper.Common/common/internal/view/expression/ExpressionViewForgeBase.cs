///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    /// Base factory for expression-based window and batch view.
    /// </summary>
    public abstract class ExpressionViewForgeBase : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider
    {
        public ExprNode ExpiryExpression { get; internal set; }

        public IDictionary<string, VariableMetaData> VariableNames { get; internal set; }

        public EventType BuiltinType { get; internal set; }

        public int? SubqueryNumber { get; internal set; }

        public int StreamNumber { get; internal set; }

        public bool IsTargetHa { get; internal set; }

        public EventType EventType1 {
            get => eventType;
            internal set => eventType = value;
        }

        protected abstract void MakeSetters(
            CodegenExpressionRef factory,
            CodegenBlock block);

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
            //streamNumber = streamNumber;
            SubqueryNumber = viewForgeEnv.SubqueryNumber;
            IsTargetHa = viewForgeEnv.SerdeResolver.IsTargetHA;
            // define built-in fields
            var builtinTypeDef = ExpressionViewOAFieldEnumExtensions.AsMapOfTypes(eventType);
            var outputEventTypeName =
                viewForgeEnv.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewExpr(StreamNumber);
            var metadata = new EventTypeMetadata(
                outputEventTypeName,
                viewForgeEnv.ModuleName,
                EventTypeTypeClass.VIEWDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var propertyTypes = EventTypeUtility.GetPropertyTypesNonPrimitive(builtinTypeDef);
            BuiltinType = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                viewForgeEnv.BeanEventTypeFactoryProtected,
                viewForgeEnv.EventTypeCompileTimeResolver);
            viewForgeEnv.EventTypeModuleCompileTimeRegistry.NewType(BuiltinType);
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] { eventType, BuiltinType },
                new string[2],
                new bool[2],
                false,
                false);
            // validate expression
            ExpiryExpression = ViewForgeSupport.ValidateExpr(
                ViewName,
                ExpiryExpression,
                streamTypeService,
                viewForgeEnv,
                0);
            var summaryVisitor = new ExprNodeSummaryVisitor();
            ExpiryExpression.Accept(summaryVisitor);
            if (summaryVisitor.HasSubselect || summaryVisitor.HasStreamSelect || summaryVisitor.HasPreviousPrior) {
                throw new ViewParameterException(
                    "Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context");
            }

            var returnType = ExpiryExpression.Forge.EvaluationType;
            if (!returnType.IsTypeBoolean()) {
                throw new ViewParameterException(
                    "Invalid return value for expiry expression, expected a boolean return value but received " +
                    returnType.CleanName());
            }

            // determine variables used, if any
            var visitor =
                new ExprNodeVariableVisitor(viewForgeEnv.StatementCompileTimeServices.VariableCompileTimeResolver);
            ExpiryExpression.Accept(visitor);
            VariableNames = visitor.VariableNames;
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (ScheduleCallbackId == -1) {
                throw new IllegalStateException("Schedule callback id not provided");
            }

            var evalClass = MakeExpiryEval(classScope);
            classScope.AddInnerClass(evalClass);
            method.Block
                .DeclareVar(evalClass.ClassName, "eval", CodegenExpressionBuilder.NewInstanceInner(evalClass.ClassName, Ref("statementFields")))
                .SetProperty(
                    factory,
                    "BuiltinMapType",
                    EventTypeUtility.ResolveTypeCodegen(BuiltinType, EPStatementInitServicesConstants.REF))
                .SetProperty(factory, "ScheduleCallbackId", Constant(ScheduleCallbackId))
                .SetProperty(
                    factory,
                    "AggregationServiceFactory",
                    MakeAggregationService(classScope, method, symbols, IsTargetHa))
                .SetProperty(factory, "AggregationResultFutureAssignable", Ref("eval"))
                .SetProperty(factory, "ExpiryEval", Ref("eval"))
                .SetProperty(factory, "SubqueryNumber", Constant(SubqueryNumber))
                .SetProperty(factory, "StreamNumber", Constant(StreamNumber));
            if (VariableNames != null && !VariableNames.IsEmpty()) {
                method.Block.SetProperty(
                    factory,
                    "Variables",
                    VariableDeployTimeResolver.MakeResolveVariables(
                        VariableNames.Values,
                        symbols.GetAddInitSvc(method)));
            }

            MakeSetters(factory, method.Block);
        }

        public override void AssignStateMgmtSettings(
            FabricCharge fabricCharge,
            ViewForgeEnv viewForgeEnv,
            int[] grouping)
        {
            // determine aggregation nodes, if any
            IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(ExpiryExpression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                try {
                    var attributionKey = new AggregationAttributionKeyView(
                        viewForgeEnv.StreamNumber,
                        viewForgeEnv.SubqueryNumber,
                        grouping);
                    AggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
                        attributionKey,
                        EmptyList<ExprAggregateNode>.Instance,
                        EmptyDictionary<ExprNode, string>.Instance,
                        EmptyList<ExprDeclaredNode>.Instance, 
                        null,
                        null,
                        aggregateNodes,
                        EmptyList<ExprAggregateNode>.Instance, 
                        EmptyList<ExprAggregateNodeGroupKey>.Instance, 
                        false,
                        viewForgeEnv.Annotations,
                        viewForgeEnv.VariableCompileTimeResolver,
                        false,
                        null,
                        null,
                        new EventType[] { eventType, BuiltinType },
                        null,
                        viewForgeEnv.ContextName,
                        null,
                        null,
                        false,
                        false,
                        false,
                        viewForgeEnv.ImportServiceCompileTime,
                        viewForgeEnv.StatementRawInfo,
                        viewForgeEnv.SerdeResolver,
                        viewForgeEnv.StateMgmtSettingsProvider);
                }
                catch (ExprValidationException ex) {
                    throw new ViewParameterException(ex.Message, ex);
                }
            }

            base.AssignStateMgmtSettings(fabricCharge, viewForgeEnv, grouping);
            if (AggregationServiceForgeDesc != null) {
                fabricCharge.Add(AggregationServiceForgeDesc.FabricCharge);
            }
        }

        public int ScheduleCallbackId { get; set; } = -1;

        private CodegenExpression MakeAggregationService(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            bool isTargetHA)
        {
            if (AggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames =
                new AggregationClassNames(CodegenNamespaceScopeNames.ClassPostfixAggregationForView(StreamNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                AggregationServiceForgeDesc.AggregationServiceFactoryForge,
                parent,
                classScope,
                classScope.OutermostClassName,
                aggregationClassNames,
                isTargetHA);
            classScope.AddInnerClasses(aggResult.InnerClasses);
            return LocalMethod(aggResult.InitMethod, symbols.GetAddInitSvc(parent));
        }

        private CodegenInnerClass MakeExpiryEval(CodegenClassScope classScope)
        {
            var classNameExpressionEval = "Exprview_eval_" + StreamNumber;
            var evalMethod = CodegenMethod.MakeParentNode(
                    typeof(object),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(PARAMS);
            var evalMethodCall = CodegenLegoMethodExpression.CodegenExpression(
                ExpiryExpression.Forge,
                evalMethod,
                classScope);
            evalMethod.Block.MethodReturn(LocalMethod(evalMethodCall, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            var assignMethod = CodegenMethod
                .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<AggregationResultFuture>("future");
            var field = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameViewAgg(StreamNumber),
                typeof(AggregationResultFuture));
            assignMethod.Block.AssignRef(field, Ref("future"));

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(evalMethod, "Evaluate", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", innerMethods, innerProperties);
            
            var statementFieldsClassName = classScope.NamespaceScope.FieldsClassNameOptional;
            var ctor = new CodegenCtor(
                typeof(StmtClassForgeableRSPFactoryProvider),
                classScope,
                new List<CodegenTypedParam>() {
                    new CodegenTypedParam(statementFieldsClassName, null, "statementFields")
                });
            return new CodegenInnerClass(
                classNameExpressionEval,
                typeof(AggregationResultFutureAssignableWEval),
                ctor,
                EmptyList<CodegenTypedParam>.Instance, 
                innerMethods,
                innerProperties);
        }

        public AggregationServiceForgeDesc AggregationServiceForgeDesc { get; private set; }
    }
} // end of namespace