///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationUseFlags
    {
        private readonly bool isUnidirectional;
        private readonly bool isFireAndForget;
        private readonly bool isOnSelect;

        public AggregationUseFlags(
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            this.isUnidirectional = isUnidirectional;
            this.isFireAndForget = isFireAndForget;
            this.isOnSelect = isOnSelect;
        }

        public bool IsUnidirectional => isUnidirectional;

        public bool IsFireAndForget => isFireAndForget;

        public bool IsOnSelect => isOnSelect;

        public CodegenExpression ToExpression()
        {
            return NewInstance<AggregationUseFlags>(
                Constant(isUnidirectional),
                Constant(isFireAndForget),
                Constant(isOnSelect));
        }
    }
} // end of namespace