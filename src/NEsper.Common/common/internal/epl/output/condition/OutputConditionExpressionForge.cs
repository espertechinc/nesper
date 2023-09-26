///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpressionForge : OutputConditionFactoryForge,
        ScheduleHandleCallbackProvider
    {
        private readonly ExprNode whenExpressionNodeEval;
        private readonly ExprNode andWhenTerminatedExpressionNodeEval;
        private readonly VariableReadWritePackageForge variableReadWritePackage;
        private readonly VariableReadWritePackageForge variableReadWritePackageAfterTerminated;
        private readonly IDictionary<string, VariableMetaData> variableNames;
        protected readonly bool isStartConditionOnCreation;
        private readonly StateMgmtSetting stateMgmtSetting;
        private readonly bool isUsingBuiltinProperties;
        private int scheduleCallbackId = -1;

        public OutputConditionExpressionForge(
            ExprNode whenExpressionNode,
            IList<OnTriggerSetAssignment> assignments,
            ExprNode andWhenTerminatedExpr,
            IList<OnTriggerSetAssignment> afterTerminateAssignments,
            bool isStartConditionOnCreation,
            StateMgmtSetting stateMgmtSetting,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            whenExpressionNodeEval = whenExpressionNode;
            andWhenTerminatedExpressionNodeEval = andWhenTerminatedExpr;
            this.isStartConditionOnCreation = isStartConditionOnCreation;
            this.stateMgmtSetting = stateMgmtSetting;
            // determine if using variables
            var variableVisitor = new ExprNodeVariableVisitor(services.VariableCompileTimeResolver);
            whenExpressionNode.Accept(variableVisitor);
            variableNames = variableVisitor.VariableNames;
            // determine if using properties
            var containsBuiltinProperties = ContainsBuiltinProperties(whenExpressionNode);
            if (!containsBuiltinProperties && assignments != null) {
                foreach (var assignment in assignments) {
                    if (ContainsBuiltinProperties(assignment.Expression)) {
                        containsBuiltinProperties = true;
                    }
                }
            }

            if (!containsBuiltinProperties && andWhenTerminatedExpressionNodeEval != null) {
                containsBuiltinProperties = ContainsBuiltinProperties(andWhenTerminatedExpr);
            }

            if (!containsBuiltinProperties && afterTerminateAssignments != null) {
                foreach (var assignment in afterTerminateAssignments) {
                    if (ContainsBuiltinProperties(assignment.Expression)) {
                        containsBuiltinProperties = true;
                    }
                }
            }

            isUsingBuiltinProperties = containsBuiltinProperties;
            if (assignments != null && !assignments.IsEmpty()) {
                variableReadWritePackage = new VariableReadWritePackageForge(
                    assignments,
                    statementRawInfo.StatementName,
                    services);
            }
            else {
                variableReadWritePackage = null;
            }

            if (afterTerminateAssignments != null) {
                variableReadWritePackageAfterTerminated = new VariableReadWritePackageForge(
                    afterTerminateAssignments,
                    statementRawInfo.StatementName,
                    services);
            }
            else {
                variableReadWritePackageAfterTerminated = null;
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("Schedule callback id not provided");
            }

            var method = parent.MakeChild(typeof(OutputConditionFactory), GetType(), classScope);
            method.Block
                .DeclareVar<
                    OutputConditionExpressionFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.RESULTSETPROCESSORHELPERFACTORY)
                        .Add("makeOutputConditionExpression"))
                .ExprDotMethod(
                    Ref("factory"),
                    "setWhenExpressionNodeEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(
                        whenExpressionNodeEval.Forge,
                        method,
                        GetType(),
                        classScope))
                .ExprDotMethod(
                    Ref("factory"),
                    "setAndWhenTerminatedExpressionNodeEval",
                    andWhenTerminatedExpressionNodeEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            andWhenTerminatedExpressionNodeEval.Forge,
                            method,
                            GetType(),
                            classScope))
                .ExprDotMethod(Ref("factory"), "setUsingBuiltinProperties", Constant(isUsingBuiltinProperties))
                .ExprDotMethod(
                    Ref("factory"),
                    "setVariableReadWritePackage",
                    variableReadWritePackage == null
                        ? ConstantNull()
                        : variableReadWritePackage.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("factory"),
                    "setVariableReadWritePackageAfterTerminated",
                    variableReadWritePackageAfterTerminated == null
                        ? ConstantNull()
                        : variableReadWritePackageAfterTerminated.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("factory"),
                    "setVariables",
                    variableNames == null
                        ? ConstantNull()
                        : VariableDeployTimeResolver.MakeResolveVariables(
                            variableNames.Values,
                            symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("factory"), "setScheduleCallbackId", Constant(scheduleCallbackId))
                .ExprDotMethod(Ref("factory"), "setStateMgmtSetting", stateMgmtSetting.ToExpression())
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("addReadyCallback", Ref("factory")))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public void CollectSchedules(
            CallbackAttributionOutputRate callbackAttribution,
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
            scheduleHandleCallbackProviders.Add(new ScheduleHandleTracked(callbackAttribution, this));
        }

        private bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return !propertyVisitor.ExprProperties.IsEmpty();
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;

            set => scheduleCallbackId = value;
        }
    }
} // end of namespace