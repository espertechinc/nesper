///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionNewInstance : CodegenExpression
    {
        private readonly Type _clazz;
        private readonly CodegenExpression[] _params;

        public CodegenExpressionNewInstance(
            Type clazz,
            CodegenExpression[] @params)
        {
            _clazz = clazz;
            _params = @params;
            CodegenExpressionExtensions.AssertNonNullArgs(@params);
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("new ");
            CodeGenerationHelper.AppendClassName(builder, _clazz);
            builder.Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, _params, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_clazz);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _params);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            CodegenExpressionBuilder.TraverseMultiple(_params, consumer);
        }
    }
} // end of namespace