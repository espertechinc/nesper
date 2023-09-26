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
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionLocalProperty : CodegenExpression
    {
        private readonly CodegenProperty _propertyNode;

        public CodegenExpressionLocalProperty(
            CodegenProperty propertyNode)
        {
            _propertyNode = propertyNode;
            if (propertyNode == null) {
                throw new ArgumentException("Null property node");
            }
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_propertyNode.AssignedProperty == null) {
                throw new IllegalStateException("Property has no assignment for " + _propertyNode.AdditionalDebugInfo);
            }

            builder.Append(_propertyNode.AssignedProperty.Name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _propertyNode.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            // No Parameters
        }
    }
} // end of namespace