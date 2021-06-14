///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
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
    ///     Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpressionForge : OutputConditionFactoryForge,
        ScheduleHandleCallbackProvider
    {
        private readonly ExprNode andWhenTerminatedExpressionNodeEval;
        internal readonly bool isStartConditionOnCreation;
        private readonly bool isUsingBuiltinProperties;
        private readonly IDictionary<string, VariableMetaData> variableNames;
        private readonly VariableReadWritePackageForge variableReadWritePackage;
        private readonly VariableReadWritePackageForge variableReadWritePackageAfterTerminated;
        private readonly ExprNode whenExpressionNodeEval;
        private int scheduleCallbackId = -1;

        public OutputConditionExpressionForge(
            ExprNode whenExpressionNode,
            IList<OnTriggerSetAssignment> assignments,
            ExprNode andWhenTerminatedExpr,
            IList<OnTriggerSetAssignment> afterTerminateAssignments,
            bool isStartConditionOnCreation,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            whenExpressionNodeEval = whenExpressionNode;
            andWhenTerminatedExpressionNodeEval = andWhenTerminatedExpr;
            this.isStartConditionOnCreation = isStartConditionOnCreation;

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
                .DeclareVar<OutputConditionExpressionFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.RESULTSETPROCESSORHELPERFACTORY)
                        .Add("MakeOutputConditionExpression"))
                .SetProperty(
                    Ref("factory"),
                    "WhenExpressionNodeEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(
                        whenExpressionNodeEval.Forge,
                        method,
                        GetType(),
                        classScope))
                .SetProperty(
                    Ref("factory"),
                    "WhenTerminatedExpressionNodeEval",
                    andWhenTerminatedExpressionNodeEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            andWhenTerminatedExpressionNodeEval.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(Ref("factory"), "IsUsingBuiltinProperties", Constant(isUsingBuiltinProperties))
                .SetProperty(
                    Ref("factory"),
                    "VariableReadWritePackage",
                    variableReadWritePackage == null
                        ? ConstantNull()
                        : variableReadWritePackage.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "VariableReadWritePackageAfterTerminated",
                    variableReadWritePackageAfterTerminated == null
                        ? ConstantNull()
                        : variableReadWritePackageAfterTerminated.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "Variables",
                    variableNames == null
                        ? ConstantNull()
                        : VariableDeployTimeResolver.MakeResolveVariables(
                            variableNames.Values,
                            symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("factory"), "ScheduleCallbackId", Constant(scheduleCallbackId))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("factory")))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
            scheduleHandleCallbackProviders.Add(this);
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        private bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return !propertyVisitor.ExprProperties.IsEmpty();
        }
    }
} // end of namespace