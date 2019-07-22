///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapEventBeanArr : ExprDotStaticMethodWrap
    {
        private EventType type;

        public ExprDotStaticMethodWrapEventBeanArr(EventType type)
        {
            this.type = type;
        }

        public EPType TypeInfo {
            get => EPTypeHelper.CollectionOfEvents(type);
        }

        public ICollection<EventBean> ConvertNonNull(object result)
        {
            if (!result.GetType().IsArray) {
                return null;
            }

            return result.Unwrap<EventBean>();
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(CompatExtensions), "Unwrap", result);
        }
    }
} // end of namespace