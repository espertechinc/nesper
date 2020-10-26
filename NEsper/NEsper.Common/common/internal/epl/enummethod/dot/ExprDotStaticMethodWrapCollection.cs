///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapCollection : ExprDotStaticMethodWrap
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type componentType;
        private readonly string methodName;

        public ExprDotStaticMethodWrapCollection(
            string methodName,
            Type componentType)
        {
            this.methodName = methodName;
            this.componentType = componentType;
        }

        public EPType TypeInfo => EPTypeHelper.CollectionOfSingleValue(
            componentType,
            null);

        public ICollection<EventBean> ConvertNonNull(object result)
        {
            if (!(result is ICollection<EventBean>)) {
                Log.Warn(
                    "Expected collection-type input from method '" + methodName + "' but received " + result.GetType());
                return null;
            }

            return (ICollection<EventBean>) result;
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (componentType == typeof(object)) {
                return result;
            }

            return CodegenExpressionBuilder.Unwrap<object>(result);
        }
    }
} // end of namespace