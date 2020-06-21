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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionInstanceField : CodegenExpression
    {
        private readonly CodegenExpression _instance;
        private readonly CodegenField _fieldNode;

        public CodegenExpressionInstanceField(
            CodegenExpression instance,
            CodegenField fieldNode)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance), "null instance");
            _fieldNode = fieldNode ?? throw new ArgumentNullException(nameof(fieldNode), "null field node");
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            _instance.Render(builder, isInnerClass, level, indent);
            builder.Append('.');
            builder.Append(_fieldNode.Name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _instance.MergeClasses(classes);
            _fieldNode.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace