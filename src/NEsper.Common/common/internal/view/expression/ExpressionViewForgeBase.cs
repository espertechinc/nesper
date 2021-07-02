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
        private AggregationServiceForgeDesc _aggregationServiceForgeDesc;
        private EventType _builtinType;
        private ExprNode _expiryExpression;
        private int _scheduleCallbackId = -1;
        private int _streamNumber;
        private IDictionary<string, VariableMetaData> _variableNames;

        public int ScheduleCallbackId {
            set => _scheduleCallbackId = value;
        }

        public ExprNode ExpiryExpression {
            get => _expiryExpression;
            set => _expiryExpression = value;
        }

        internal abstract void MakeSetters(
            CodegenExpressionRef factory,
            CodegenBlock block);

        public override void AttachValidate(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            eventType = parentEventType;
            _streamNumber = streamNumber;

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
            _builtinType = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                viewForgeEnv.BeanEventTypeFactoryProtected,
                viewForgeEnv.EventTypeCompileTimeResolver);
            viewForgeEnv.EventTypeModuleCompileTimeRegistry.NewType(_builtinType);

            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                new[] {eventType, _builtinType},
                new string[2],
                new bool[2],
                false,
                false);

            // validate expression
            _expiryExpression = ViewForgeSupport.ValidateExpr(
                ViewName,
                _expiryExpression,
                streamTypeService,
                viewForgeEnv,
                0,
                streamNumber);

            var summaryVisitor = new ExprNodeSummaryVisitor();
            _expiryExpression.Accept(summaryVisitor);
            if (summaryVisitor.HasSubselect ||
                summaryVisitor.HasStreamSelect ||
                summaryVisitor.HasPreviousPrior) {
                throw new ViewParameterException(
                    "Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context");
            }

            var returnType = _expiryExpression.Forge.EvaluationType;
            if (returnType.GetBoxedType() != typeof(bool?)) {
                throw new ViewParameterException(
                    "Invalid return value for expiry expression, expected a boolean return value but received " +
                    returnType.GetParameterAsString());
            }

            // determine variables used, if any
            var visitor =
                new ExprNodeVariableVisitor(viewForgeEnv.StatementCompileTimeServices.VariableCompileTimeResolver);
            _expiryExpression.Accept(visitor);
            _variableNames = visitor.VariableNames;

            // determine aggregation nodes, if any
            IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(_expiryExpression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                try {
                    _aggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
                        Collections.GetEmptyList<ExprAggregateNode>(),
                        Collections.GetEmptyMap<ExprNode, string>(),
                        Collections.GetEmptyList<ExprDeclaredNode>(),
                        null,
                        null,
                        aggregateNodes,
                        Collections.GetEmptyList<ExprAggregateNode>(),
                        Collections.GetEmptyList<ExprAggregateNodeGroupKey>(),
                        false,
                        viewForgeEnv.Annotations,
                        viewForgeEnv.VariableCompileTimeResolver,
                        false,
                        null,
                        null,
                        streamTypeService.EventTypes,
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
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_scheduleCallbackId == -1) {
                throw new IllegalStateException("Schedule callback id not provided");
            }

            var evalClass = MakeExpiryEval(classScope);
            classScope.AddInnerClass(evalClass);

            method.Block
                .DeclareVar(
                    evalClass.ClassName,
                    "eval",
                    NewInstanceNamed(evalClass.ClassName, Ref("statementFields")))
                .SetProperty(
                    factory,
                    "BuiltinMapType",
                    EventTypeUtility.ResolveTypeCodegen(_builtinType, EPStatementInitServicesConstants.REF))
                .SetProperty(
                    factory,
                    "ScheduleCallbackId",
                    Constant(_scheduleCallbackId))
                .SetProperty(
                    factory,
                    "AggregationServiceFactory",
                    MakeAggregationService(classScope, method, symbols))
                .SetProperty(
                    factory,
                    "AggregationResultFutureAssignable",
                    Ref("eval"))
                .SetProperty(
                    factory,
                    "ExpiryEval",
                    Ref("eval"));
            if (_variableNames != null && !_variableNames.IsEmpty()) {
                method.Block.SetProperty(
                    factory,
                    "Variables",
                    VariableDeployTimeResolver.MakeResolveVariables(
                        _variableNames.Values,
                        symbols.GetAddInitSvc(method)));
            }

            MakeSetters(factory, method.Block);
        }

        private CodegenExpression MakeAggregationService(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            if (_aggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames =
                new AggregationClassNames(CodegenNamespaceScopeNames.ClassPostfixAggregationForView(_streamNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                false,
                _aggregationServiceForgeDesc.AggregationServiceFactoryForge,
                parent,
                classScope,
                classScope.OutermostClassName,
                aggregationClassNames);
            classScope.AddInnerClasses(aggResult.InnerClasses);
            return LocalMethod(aggResult.InitMethod, symbols.GetAddInitSvc(parent));
        }

        private CodegenInnerClass MakeExpiryEval(CodegenClassScope classScope)
        {
            var classNameExpressionEval = "ExprViewEval_" + _streamNumber;

            var evalMethod = CodegenMethod.MakeMethod(
                    typeof(object),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(PARAMS);
            var evalMethodCall = CodegenLegoMethodExpression.CodegenExpression(ExpiryExpression.Forge, evalMethod, classScope);
            evalMethod.Block.MethodReturn(LocalMethod(evalMethodCall, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));

            var assignMethod = CodegenMethod
                .MakeMethod(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationResultFuture), "future");
            var field = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameViewAgg(_streamNumber),
                typeof(AggregationResultFuture));
            assignMethod.Block.AssignRef(field, Ref("future"));

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(evalMethod, "Evaluate", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", innerMethods, innerProperties);

            var statementFieldsClassName = classScope.NamespaceScope.FieldsClassName;
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
                Collections.GetEmptyList<CodegenTypedParam>(),
                innerMethods,
                innerProperties);
        }
    }
} // end of namespace