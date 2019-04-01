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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionField : CodegenExpression
    {
        private readonly CodegenField _field;

        public CodegenExpressionField(CodegenField field)
        {
            if (field == null) {
                throw new ArgumentException("Null field");
            }

            this._field = field;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            _field.Render(builder);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _field.MergeClasses(classes);
        }
    }
} // end of namespace