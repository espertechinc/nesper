///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionLocalMethod : CodegenExpression
    {
        private readonly CodegenMethod _methodNode;
        private readonly IList<CodegenExpression> _parameters;

        internal CodegenMethod MethodNode => _methodNode;

        public CodegenExpressionLocalMethod(
            CodegenMethod methodNode,
            IList<CodegenExpression> parameters)
        {
            _methodNode = methodNode;
            if (methodNode == null) {
                throw new ArgumentException("Null method node");
            }

            _parameters = parameters;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_methodNode.AssignedMethod == null) {
                throw new IllegalStateException("Method has no assignment for " + _methodNode.AdditionalDebugInfo);
            }

            if (_methodNode.AssignedProviderClassName != null) {
                builder.Append(_methodNode.AssignedProviderClassName);
                builder.Append(".");
            }

            builder.Append(_methodNode.AssignedMethod.Name).Append("(");
            var delimiter = "";

            // pass explicit parameters first
            foreach (var expression in _parameters) {
                builder.Append(delimiter);
                expression.Render(builder, isInnerClass, level, indent);
                delimiter = ",";
            }

            // pass pass-thru second
            if (_methodNode.OptionalSymbolProvider == null) {
                foreach (var name in _methodNode.DeepParameters) {
                    builder.Append(delimiter);
                    builder.Append(name);
                    delimiter = ",";
                }
            }

            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _methodNode.MergeClasses(classes);
            _parameters.For(p => p.MergeClasses(classes));
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            CodegenExpressionBuilder.TraverseMultiple(_parameters, consumer);
        }
    }
} // end of namespace