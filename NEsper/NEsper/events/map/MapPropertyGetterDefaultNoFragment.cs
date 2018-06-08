///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    /// <summary>Getter for map entry.</summary>
    public class MapPropertyGetterDefaultNoFragment : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultNoFragment(string propertyName, EventAdapterService eventAdapterService)
            : base(propertyName, null, eventAdapterService)
        {
        }

        protected override object HandleCreateFragment(object value)
        {
            return null;
        }

        protected override ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value,
            ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace