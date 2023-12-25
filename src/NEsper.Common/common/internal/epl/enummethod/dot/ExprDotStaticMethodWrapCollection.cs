///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapCollection : ExprDotStaticMethodWrap
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type _componentType;
        private readonly string _methodName;

        public ExprDotStaticMethodWrapCollection(
            string methodName,
            Type componentType) 
        {
            _methodName = methodName;
            _componentType = componentType;
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfSingleValue(
            _componentType);

        public object ConvertNonNull(object result)
        {
            if (result == null)
                return null;
            if (result.GetType().IsGenericCollection())
                return result;

            Log.Warn("Expected collection-type input from method '" + _methodName + "' but received " + result.GetType());
            return null;
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_componentType == typeof(object)) {
                return result;
            }

            return CodegenExpressionBuilder.Unwrap(_componentType, result);
        }
    }
} // end of namespace