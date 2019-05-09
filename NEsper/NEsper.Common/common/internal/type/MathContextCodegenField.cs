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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Math context member
    /// </summary>
    public class MathContextCodegenField : CodegenFieldSharable
    {
        private readonly MathContext mathContext;

        public MathContextCodegenField(MathContext mathContext)
        {
            this.mathContext = mathContext;
        }

        public Type Type()
        {
            return typeof(MathContext);
        }

        public CodegenExpression InitCtorScoped()
        {
            if (mathContext == null)
            {
                return ConstantNull();
            }

            return NewInstance<MathContext>(
                Constant(mathContext.Precision),
                EnumValue(mathContext.RoundingMode));
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            MathContextCodegenField that = (MathContextCodegenField) o;

            return mathContext != null ? mathContext.Equals(that.mathContext) : that.mathContext == null;
        }

        public override int GetHashCode()
        {
            return mathContext != null ? mathContext.GetHashCode() : 0;
        }
    }
} // end of namespace