///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateItemForgeWExpressions
    {
        public EventBeanUpdateItemForgeWExpressions(
            CodegenExpression rhsExpression,
            EventBeanUpdateItemArrayExpressions optionalArrayExpressions)
        {
            RhsExpression = rhsExpression;
            OptionalArrayExpressions = optionalArrayExpressions;
        }

        public CodegenExpression RhsExpression { get; }

        public EventBeanUpdateItemArrayExpressions OptionalArrayExpressions { get; }
    }
} // end of namespace