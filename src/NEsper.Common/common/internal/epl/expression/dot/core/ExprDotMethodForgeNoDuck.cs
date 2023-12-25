///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeNoDuck : ExprDotForge
    {
        public enum WrapType
        {
            WRAPARRAY,
            UNDERLYING,
            PLAIN
        }

        private readonly MethodInfo _method;
        private readonly Type _methodTargetType;

        private readonly WrapType _wrapType;

        public ExprDotMethodForgeNoDuck(
            string optionalStatementName,
            MethodInfo method,
            Type methodTargetType,
            ExprForge[] parameters,
            WrapType wrapType)
        {
            OptionalStatementName = optionalStatementName;
            this._method = method;
            this._methodTargetType = methodTargetType;
            Parameters = parameters;
            this._wrapType = wrapType;
        }

        public string OptionalStatementName { get; }

        public MethodInfo Method => _method;

        public ExprForge[] Parameters { get; }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitMethod(_method.Name);
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_wrapType == WrapType.WRAPARRAY) {
                return ExprDotMethodForgeNoDuckEvalWrapArray.CodegenWrapArray(
                    this,
                    inner,
                    innerType,
                    parent,
                    symbols,
                    classScope);
            }

            if (_wrapType == WrapType.PLAIN) {
                return ExprDotMethodForgeNoDuckEvalPlain.CodegenPlain(
                    this,
                    inner,
                    innerType,
                    parent,
                    symbols,
                    classScope);
            }

            return ExprDotMethodForgeNoDuckEvalUnderlying.CodegenUnderlying(
                this,
                inner,
                innerType,
                parent,
                symbols,
                classScope);
        }

        public EPChainableType TypeInfo {
            get {
                if (_wrapType == WrapType.WRAPARRAY) {
                    var returnType = _method.ReturnType;
                    var componentType = returnType.GetComponentType();
                    return EPChainableTypeHelper.CollectionOfSingleValue(componentType);
                }

                return EPChainableTypeHelper.FromMethod(_method);
            }
        }

        public ExprDotEval DotEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parameters);
                if (_wrapType == WrapType.WRAPARRAY) {
                    return new ExprDotMethodForgeNoDuckEvalWrapArray(this, evaluators);
                }

                if (_wrapType == WrapType.PLAIN) {
                    return new ExprDotMethodForgeNoDuckEvalPlain(this, evaluators);
                }

                return new ExprDotMethodForgeNoDuckEvalUnderlying(this, evaluators);
            }
        }

        public WrapType GetWrapType()
        {
            return _wrapType;
        }
    }
} // end of namespace