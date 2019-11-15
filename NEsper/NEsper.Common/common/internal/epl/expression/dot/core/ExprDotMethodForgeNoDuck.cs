///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeNoDuck : ExprDotForge
    {
        public enum DuckType
        {
            WRAPARRAY,
            UNDERLYING,
            PLAIN
        }

        private readonly DuckType type;

        public ExprDotMethodForgeNoDuck(
            string optionalStatementName,
            MethodInfo method,
            ExprForge[] parameters,
            DuckType type)
        {
            OptionalStatementName = optionalStatementName;
            Method = method;
            Parameters = parameters;
            this.type = type;
        }

        public string OptionalStatementName { get; }

        public MethodInfo Method { get; }

        public ExprForge[] Parameters { get; }

        public EPType TypeInfo {
            get {
                if (type == DuckType.WRAPARRAY) {
                    return EPTypeHelper.CollectionOfSingleValue(
                        Method.ReturnType.GetElementType(),
                        Method.ReturnType);
                }

                return EPTypeHelper.FromMethod(Method);
            }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitMethod(Method.Name);
        }

        public ExprDotEval DotEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parameters);
                if (type == DuckType.WRAPARRAY) {
                    return new ExprDotMethodForgeNoDuckEvalWrapArray(this, evaluators);
                }

                if (type == DuckType.PLAIN) {
                    return new ExprDotMethodForgeNoDuckEvalPlain(this, evaluators);
                }

                return new ExprDotMethodForgeNoDuckEvalUnderlying(this, evaluators);
            }
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (type == DuckType.WRAPARRAY) {
                return ExprDotMethodForgeNoDuckEvalWrapArray.CodegenWrapArray(
                    this,
                    inner,
                    innerType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            if (type == DuckType.PLAIN) {
                return ExprDotMethodForgeNoDuckEvalPlain.CodegenPlain(
                    this,
                    inner,
                    innerType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            return ExprDotMethodForgeNoDuckEvalUnderlying.CodegenUnderlying(
                this,
                inner,
                innerType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace