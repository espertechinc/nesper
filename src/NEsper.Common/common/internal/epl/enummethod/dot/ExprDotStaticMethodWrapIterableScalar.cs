///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapIterableScalar : ExprDotStaticMethodWrap
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly string methodName;
        private readonly Type componentType;

        public ExprDotStaticMethodWrapIterableScalar(
            string methodName,
            Type componentType)
        {
            this.methodName = methodName;
            this.componentType = componentType;
        }

        public object ConvertNonNull(object result)
        {
            if (result == null)
                return null;
            if (result.GetType().IsGenericEnumerable())
                return result;
            
            Log.Warn(
                "Expected iterable-type input from method '" + methodName + "' but received " + result.GetType());
            return null;
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(CompatExtensions),
                "Unwrap",
                Cast(typeof(IEnumerable), result),
                ConstantFalse());
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfSingleValue(componentType);
    }
} // end of namespace