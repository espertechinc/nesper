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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.variant
{
    public class VariantPropertyGetterCacheCodegenField : CodegenFieldSharable
    {
        private readonly VariantEventType _variantEventType;

        public VariantPropertyGetterCacheCodegenField(VariantEventType variantEventType)
        {
            this._variantEventType = variantEventType;
        }

        public Type Type()
        {
            return typeof(VariantPropertyGetterCache);
        }

        public CodegenExpression InitCtorScoped()
        {
            var type = Cast(
                typeof(VariantEventType),
                EventTypeUtility.ResolveTypeCodegen(_variantEventType, EPStatementInitServicesConstants.REF));
            return ExprDotName(type, "VariantPropertyGetterCache");
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (VariantPropertyGetterCacheCodegenField) o;

            return _variantEventType.Equals(that._variantEventType);
        }

        public override int GetHashCode()
        {
            return _variantEventType.GetHashCode();
        }
    }
} // end of namespace