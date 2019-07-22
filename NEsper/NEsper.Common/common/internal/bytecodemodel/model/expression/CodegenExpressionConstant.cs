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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionConstant : CodegenExpression
    {
        private readonly object _constant;

        public CodegenExpressionConstant(object constant)
        {
            this._constant = constant;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            RenderConstant(builder, _constant);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_constant == null) {
                return;
            }

            if (_constant.GetType().IsArray) {
                classes.Add(_constant.GetType().GetElementType());
            }
            else if (_constant.GetType().IsEnum) {
                classes.Add(_constant.GetType());
            }
        }
    }
} // end of namespace