///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        internal readonly string propertyName;

        public MapEventBeanPropertyWriter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public virtual void Write(
            object value,
            EventBean target)
        {
            var map = (MappedEventBean)target;
            Write(value, map.Properties);
        }

        public virtual CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(underlying, "Put", Constant(propertyName), assigned);
        }

        public virtual void Write(
            object value,
            IDictionary<string, object> map)
        {
            map.Put(propertyName, value);
        }
    }
} // end of namespace