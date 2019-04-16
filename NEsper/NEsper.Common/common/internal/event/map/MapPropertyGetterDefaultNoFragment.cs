///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for map entry.
    /// </summary>
    public class MapPropertyGetterDefaultNoFragment : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultNoFragment(
            string propertyName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
            : base(
                propertyName, null, eventBeanTypedEventFactory)
        {
        }

        internal override object HandleCreateFragment(object value)
        {
            return null;
        }

        internal override CodegenExpression HandleCreateFragmentCodegen(
            CodegenExpression value,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace