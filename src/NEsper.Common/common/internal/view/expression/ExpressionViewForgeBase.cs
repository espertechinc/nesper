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
        private ExprNode expiryExpression;
        private IDictionary<string, VariableMetaData> variableNames;
        private EventType builtinType;
        private int scheduleCallbackId = -1;
        private AggregationServiceForgeDesc aggregationServiceForgeDesc;
        private int? subqueryNumber;
        private int streamNumber;
        private bool isTargetHA;

        public ExprNode ExpiryExpression {
            get => expiryExpression;
            internal set => expiryExpression = value;
        }

        public IDictionary<string, VariableMetaData> VariableNames {
            get => variableNames;
            internal set => variableNames = value;
        }

        public EventType BuiltinType {
            get => builtinType;
            internal set => builtinType = value;
        }

        public int? SubqueryNumber {
            get => subqueryNumber;
            internal set => subqueryNumber = value;
        }

        public int StreamNumber {
            get => streamNumber;
            internal set => streamNumber = value;
        }

        public bool IsTargetHa {
            get => isTargetHA;
            internal set => isTargetHA = value;
        }

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
            streamNumber = streamNumber;
            subqueryNumber = viewForgeEnv.SubqueryNumber;
            isTargetHA = viewForgeEnv.SerdeResolver.IsTargetHA;
            // define built-in fields
            var builtinTypeDef = ExpressionViewOAFieldEnumExtensions.AsMapOfTypes(eventType);
            var outputEventTypeName =
                viewForgeEnv.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewExpr(streamNumber);
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
            builtinType = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                viewForgeEnv.BeanEventTypeFactoryProtected,
                viewForgeEnv.EventTypeCompileTimeResolver);
            viewForgeEnv.EventTypeModuleCompileTimeRegistry.NewType(builtinType);
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new EventType[] { eventType, builtinType },
                new string[2],
                new bool[2],
                false,
                false);
            // validate expression
            expiryExpression = ViewForgeSupport.ValidateExpr(
                ViewName,
                expiryExpression,
                streamTypeService,
                viewForgeEnv,
                0);
            var summaryVisitor = new ExprNodeSummaryVisitor();
            expiryExpression.Accept(summaryVisitor);
            if (summaryVisitor.HasSubselect || summaryVisitor.HasStreamSelect || summaryVisitor.HasPreviousPrior) {
                throw new ViewParameterException(
                    "Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context");
            }

            var returnType = expiryExpression.Forge.EvaluationType;
            if (!returnType.IsTypeBoolean()) {
                throw new ViewParameterException(
                    "Invalid return value for expiry expression, expected a boolean return value but received " +
                    returnType.CleanName());
            }

            // determine variables used, if any
            var visitor =
                new ExprNodeVariableVisitor(viewForgeEnv.StatementCompileTimeServices.VariableCompileTimeResolver);
            expiryExpression.Accept(visitor);
            variableNames = visitor.VariableNames;
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Schedule callback id not provided");
            }

            var evalClass = MakeExpiryEval(classScope);
            classScope.AddInnerClass(evalClass);
            method.Block
                .DeclareVar(evalClass.ClassName, "eval", CodegenExpressionBuilder.NewInstanceInner(evalClass.ClassName))
                .ExprDotMethod(
                    factory,
                    "setBuiltinMapType",
                    EventTypeUtility.ResolveTypeCodegen(builtinType, EPStatementInitServicesConstants.REF))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(
                    factory,
                    "AggregationServiceFactory",
                    MakeAggregationService(classScope, method, symbols, isTargetHA))
                .SetProperty(factory, "AggregationResultFutureAssignable", Ref("eval"))
                .SetProperty(factory, "ExpiryEval", Ref("eval"))
                .SetProperty(factory, "SubqueryNumber", Constant(subqueryNumber))
                .SetProperty(factory, "StreamNumber", Constant(streamNumber));
            if (variableNames != null && !variableNames.IsEmpty()) {
                method.Block.ExprDotMethod(
                    factory,
                    "setVariables",
                    VariableDeployTimeResolver.MakeResolveVariables(
                        variableNames.Values,
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
            ExprAggregateNodeUtil.GetAggregatesBottomUp(expiryExpression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                try {
                    var attributionKey = new AggregationAttributionKeyView(
                        viewForgeEnv.StreamNumber,
                        viewForgeEnv.SubqueryNumber,
                        grouping);
                    aggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
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
                        new EventType[] { eventType, builtinType },
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
            if (aggregationServiceForgeDesc != null) {
                fabricCharge.Add(aggregationServiceForgeDesc.FabricCharge);
            }
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        private CodegenExpression MakeAggregationService(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            bool isTargetHA)
        {
            if (aggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames =
                new AggregationClassNames(CodegenNamespaceScopeNames.ClassPostfixAggregationForView(streamNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                aggregationServiceForgeDesc.AggregationServiceFactoryForge,
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
            var classNameExpressionEval = "exprview_eval_" + streamNumber;
            var evalMethod = CodegenMethod.MakeParentNode(
                    typeof(object),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(PARAMS);
            var evalMethodCall = CodegenLegoMethodExpression.CodegenExpression(
                expiryExpression.Forge,
                evalMethod,
                classScope);
            evalMethod.Block.MethodReturn(LocalMethod(evalMethodCall, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            var assignMethod = CodegenMethod
                .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<AggregationResultFuture>("future");
            CodegenExpression field = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameViewAgg(streamNumber),
                typeof(AggregationResultFuture));
            assignMethod.Block.AssignRef(field, Ref("future"));
            var innerMethods = new CodegenClassMethods();
            var innerProperties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(evalMethod, "Evaluate", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", innerMethods, innerProperties);
            var ctor = new CodegenCtor(
                typeof(StmtClassForgeableRSPFactoryProvider),
                classScope,
                EmptyList<CodegenTypedParam>.Instance);
            return new CodegenInnerClass(
                classNameExpressionEval,
                typeof(AggregationResultFutureAssignableWEval),
                ctor,
                EmptyList<CodegenTypedParam>.Instance, 
                innerMethods,
                innerProperties);
        }

        public AggregationServiceForgeDesc AggregationServiceForgeDesc => aggregationServiceForgeDesc;
    }
} // end of namespace