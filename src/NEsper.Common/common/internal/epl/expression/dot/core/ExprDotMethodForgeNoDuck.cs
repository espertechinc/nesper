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
using com.espertech.esper.common.@internal.@event.core;
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

        public ExprDotMethodForgeNoDuck(
            string optionalStatementName,
            MethodInfo method,
            Type methodTargetType,
            ExprForge[] parameters,
            DuckType type)
        {
            OptionalStatementName = optionalStatementName;
            Method = method;
            MethodTargetType = methodTargetType;
            Parameters = parameters;
            WrapType = type;
        }

        public string OptionalStatementName { get; }

        public MethodInfo Method { get; }
        
        public Type MethodTargetType { get; }

        public ExprForge[] Parameters { get; }

        public DuckType WrapType { get; }

        public EPChainableType TypeInfo {
            get {
                if (WrapType == DuckType.WRAPARRAY) {
                    var returnType = Method.ReturnType;
                    var componentType = returnType.GetElementType();
                    return EPChainableTypeHelper.CollectionOfSingleValue(componentType);
                }

                return EPChainableTypeHelper.FromMethod(Method);
            }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitMethod(Method.Name);
        }

        public ExprDotEval DotEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parameters);
                return WrapType switch {
                    DuckType.WRAPARRAY => new ExprDotMethodForgeNoDuckEvalWrapArray(this, evaluators),
                    DuckType.PLAIN => new ExprDotMethodForgeNoDuckEvalPlain(this, evaluators),
                    _ => new ExprDotMethodForgeNoDuckEvalUnderlying(this, evaluators)
                };
            }
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return WrapType switch {
                DuckType.WRAPARRAY => ExprDotMethodForgeNoDuckEvalWrapArray.CodegenWrapArray(this, inner, innerType, parent, symbols, classScope),
                DuckType.PLAIN => ExprDotMethodForgeNoDuckEvalPlain.CodegenPlain(this, inner, innerType, parent, symbols, classScope),
                _ => ExprDotMethodForgeNoDuckEvalUnderlying.CodegenUnderlying(this, inner, innerType, parent, symbols, classScope)
            };
        }
    }
} // end of namespace