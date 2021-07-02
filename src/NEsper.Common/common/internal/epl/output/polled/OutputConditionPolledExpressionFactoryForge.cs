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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpressionFactoryForge : OutputConditionPolledFactoryForge
    {
        private readonly ExprForge _whenExpressionNode;
        private readonly VariableReadWritePackageForge _variableReadWritePackage;
        private readonly bool _isUsingBuiltinProperties;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="whenExpressionNode">the expression to evaluate, returning true when to output</param>
        /// <param name="assignments">is the optional then-clause variable assignments, or null or empty if none</param>
        /// <param name="statementName">the statement name</param>
        /// <param name="services">services</param>
        /// <throws>ExprValidationException when validation fails</throws>
        public OutputConditionPolledExpressionFactoryForge(
            ExprNode whenExpressionNode,
            IList<OnTriggerSetAssignment> assignments,
            string statementName,
            StatementCompileTimeServices services)
        {
            _whenExpressionNode = whenExpressionNode.Forge;

            // determine if using properties
            _isUsingBuiltinProperties = false;
            if (ContainsBuiltinProperties(whenExpressionNode)) {
                _isUsingBuiltinProperties = true;
            }
            else {
                if (assignments != null) {
                    foreach (var assignment in assignments) {
                        if (ContainsBuiltinProperties(assignment.Expression)) {
                            _isUsingBuiltinProperties = true;
                        }
                    }
                }
            }

            if (assignments != null) {
                _variableReadWritePackage = new VariableReadWritePackageForge(assignments, statementName, services);
            }
            else {
                _variableReadWritePackage = null;
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            // initialize+resolve variables
            var symbols = new SAIFFInitializeSymbol();
            var variableInit = classScope.NamespaceScope
                .InitMethod
                .MakeChildWithScope(typeof(VariableReadWritePackage), GetType(), symbols, classScope)
                .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
            variableInit.Block
                .MethodReturn(_variableReadWritePackage.Make(variableInit, symbols, classScope));
            var variableRW = classScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(VariableReadWritePackage),
                LocalMethod(variableInit, EPStatementInitServicesConstants.REF));

            var method = parent.MakeChild(typeof(OutputConditionPolledExpressionFactory), GetType(), classScope);
            method.Block
                .DeclareVar<OutputConditionPolledExpressionFactory>(
                    "factory",
                    NewInstance(typeof(OutputConditionPolledExpressionFactory)))
                .SetProperty(
                    Ref("factory"),
                    "WhenExpression",
                    ExprNodeUtilityCodegen.CodegenEvaluator(_whenExpressionNode, method, GetType(), classScope))
                .SetProperty(Ref("factory"), "VariableReadWritePackage", variableRW)
                .SetProperty(Ref("factory"), "IsUsingBuiltinProperties", Constant(_isUsingBuiltinProperties))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        private bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return !propertyVisitor.ExprProperties.IsEmpty();
        }
    }
} // end of namespace