///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapEventBeanColl : ExprDotStaticMethodWrap
    {
        private readonly EventType type;

        public ExprDotStaticMethodWrapEventBeanColl(EventType type)
        {
            this.type = type;
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfEvents(type);

        public ICollection<EventBean> ConvertNonNull(object result)
        {
            return (ICollection<EventBean>)result;
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return result;
        }
    }
} // end of namespace