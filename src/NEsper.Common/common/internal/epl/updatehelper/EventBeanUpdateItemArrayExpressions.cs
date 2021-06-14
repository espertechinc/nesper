///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateItemArrayExpressions
    {
        public EventBeanUpdateItemArrayExpressions(
            CodegenExpression index,
            CodegenExpression arrayGet)
        {
            Index = index;
            ArrayGet = arrayGet;
        }

        public CodegenExpression Index { get; }

        public CodegenExpression ArrayGet { get; }
    }
} // end of namespace