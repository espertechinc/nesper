///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapArrayScalar : ExprDotStaticMethodWrap
    {
        private readonly Type _arrayType;
        private readonly string _methodName;

        public ExprDotStaticMethodWrapArrayScalar(
            string methodName,
            Type arrayType)
        {
            _methodName = methodName;
            _arrayType = arrayType;
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfSingleValue(
            _arrayType.GetElementType());

        public object ConvertNonNull(object result)
        {
            if (result == null)
                return null;
            if (result.GetType().IsGenericCollection())
                return result;

            return result.Unwrap<object>(false);
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CollectionUtil.ArrayToCollectionAllowNullCodegen(
                codegenMethodScope,
                _arrayType,
                result,
                codegenClassScope);
        }
    }
} // end of namespace