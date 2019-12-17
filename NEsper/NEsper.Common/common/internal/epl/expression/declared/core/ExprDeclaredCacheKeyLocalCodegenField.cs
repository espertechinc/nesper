///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.core
{
    public class ExprDeclaredCacheKeyLocalCodegenField : CodegenFieldSharable
    {
        private readonly string expressionName;

        public ExprDeclaredCacheKeyLocalCodegenField(string expressionName)
        {
            this.expressionName = expressionName;
        }

        public Type Type()
        {
            return typeof(object);
        }

        public CodegenExpression InitCtorScoped()
        {
            return NewInstance(typeof(object));
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            ExprDeclaredCacheKeyLocalCodegenField that = (ExprDeclaredCacheKeyLocalCodegenField) o;

            return expressionName.Equals(that.expressionName);
        }

        public override int GetHashCode()
        {
            return expressionName.GetHashCode();
        }
    }
} // end of namespace