///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Base factory for expression-based window and batch view.
    /// </summary>
    public abstract class ExpressionViewForgeBase : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        ScheduleHandleCallbackProvider
    {
        internal AggregationServiceForgeDesc aggregationServiceForgeDesc;
        internal EventType builtinType;
        internal ExprNode expiryExpression;
        internal int scheduleCallbackId = -1;
        internal int streamNumber;
        internal IDictionary<string, VariableMetaData> variableNames;

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        internal abstract void MakeSetters(
            CodegenExpressionRef factory,
            CodegenBlock block);

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
            this.streamNumber = streamNumber;

            // define built-in fields
            LinkedHashMap<string, object> builtinTypeDef = ExpressionViewOAFieldEnum.AsMapOfTypes(eventType);
            var outputEventTypeName =
                viewForgeEnv.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewExpr(streamNumber);
            var metadata = new EventTypeMetadata(
                outputEventTypeName, viewForgeEnv.ModuleName, EventTypeTypeClass.VIEWDERIVED,
                EventTypeApplicationType.OBJECTARR, NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                EventTypeIdPair.Unassigned());
            IDictionary<string, object> propertyTypes = EventTypeUtility.GetPropertyTypesNonPrimitive(builtinTypeDef);
            builtinType = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata, propertyTypes, null, null, null, null, viewForgeEnv.BeanEventTypeFactoryProtected,
                viewForgeEnv.EventTypeCompileTimeResolver);
            viewForgeEnv.EventTypeModuleCompileTimeRegistry.NewType(builtinType);

            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new[] {eventType, builtinType}, new string[2], new bool[2], false, false);

            // validate expression
            expiryExpression = ViewForgeSupport.ValidateExpr(
                ViewName, expiryExpression, streamTypeService, viewForgeEnv, 0, streamNumber);

            var summaryVisitor = new ExprNodeSummaryVisitor();
            expiryExpression.Accept(summaryVisitor);
            if (summaryVisitor.HasSubselect || summaryVisitor.HasStreamSelect ||
                summaryVisitor.HasPreviousPrior) {
                throw new ViewParameterException(
                    "Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context");
            }

            var returnType = expiryExpression.Forge.EvaluationType;
            if (returnType.GetBoxedType() != typeof(bool?)) {
                throw new ViewParameterException(
                    "Invalid return value for expiry expression, expected a boolean return value but received " +
                    returnType.GetParameterAsString());
            }

            // determine variables used, if any
            var visitor =
                new ExprNodeVariableVisitor(viewForgeEnv.StatementCompileTimeServices.VariableCompileTimeResolver);
            expiryExpression.Accept(visitor);
            variableNames = visitor.VariableNames;

            // determine aggregation nodes, if any
            IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(expiryExpression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                try {
                    aggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
                        Collections.GetEmptyList<ExprAggregateNode>(),
                        Collections.GetEmptyMap<ExprNode, string>(),
                        Collections.GetEmptyList<ExprDeclaredNode>(), null, aggregateNodes,
                        Collections.GetEmptyList<ExprAggregateNode>(),
                        Collections.GetEmptyList<ExprAggregateNodeGroupKey>(),
                        false,
                        viewForgeEnv.Annotations,
                        viewForgeEnv.VariableCompileTimeResolver, false, null, null,
                        streamTypeService.EventTypes, null,
                        viewForgeEnv.ContextName, null, null, false, false, false,
                        viewForgeEnv.ImportServiceCompileTime,
                        viewForgeEnv.OptionalStatementName);
                }
                catch (ExprValidationException ex) {
                    throw new ViewParameterException(ex.Message, ex);
                }
            }
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
                .DeclareVar(evalClass.ClassName, "eval", NewInstance(evalClass.ClassName))
                .SetProperty(factory, "BuiltinMapType",
                    EventTypeUtility.ResolveTypeCodegen(builtinType, EPStatementInitServicesConstants.REF))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId))
                .SetProperty(factory, "AggregationServiceFactory", MakeAggregationService(classScope, method, symbols))
                .SetProperty(factory, "AggregationResultFutureAssignable", Ref("eval"))
                .SetProperty(factory, "ExpiryEval", Ref("eval"));
            if (variableNames != null && !variableNames.IsEmpty()) {
                method.Block.SetProperty(factory, "Variables",
                    VariableDeployTimeResolver.MakeResolveVariables(
                        variableNames.Values, symbols.GetAddInitSvc(method)));
            }

            MakeSetters(factory, method.Block);
        }

        private CodegenExpression MakeAggregationService(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            if (aggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames =
                new AggregationClassNames(CodegenPackageScopeNames.ClassPostfixAggregationForView(streamNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                false, aggregationServiceForgeDesc.AggregationServiceFactoryForge, parent, classScope,
                classScope.OutermostClassName, aggregationClassNames);
            classScope.AddInnerClasses(aggResult.InnerClasses);
            return LocalMethod(aggResult.InitMethod, symbols.GetAddInitSvc(parent));
        }

        private CodegenInnerClass MakeExpiryEval(CodegenClassScope classScope)
        {
            var classNameExpressionEval = "exprview_eval_" + streamNumber;

            var evalMethod = CodegenMethod.MakeParentNode(
                typeof(object), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope).AddParam(PARAMS);
            var evalMethodCall = CodegenLegoMethodExpression.CodegenExpression(
                expiryExpression.Forge, evalMethod, classScope);
            evalMethod.Block.MethodReturn(LocalMethod(evalMethodCall, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));

            var assignMethod = CodegenMethod
                .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationResultFuture), "future");
            CodegenExpression field = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameViewAgg(streamNumber), typeof(AggregationResultFuture));
            assignMethod.Block.AssignRef(field, Ref("future"));

            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(evalMethod, "evaluate", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "assign", innerMethods);

            var ctor = new CodegenCtor(
                typeof(StmtClassForgableRSPFactoryProvider), classScope, Collections.GetEmptyList<CodegenTypedParam>());

            return new CodegenInnerClass(
                classNameExpressionEval, typeof(AggregationResultFutureAssignableWEval), ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                innerMethods);
        }
    }
} // end of namespace