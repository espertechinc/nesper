///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.lookup;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class EventAdvancedIndexFactoryForgeQuadTreePointRegionForge : EventAdvancedIndexFactoryForgeQuadTreeForge
    {
        public static readonly EventAdvancedIndexFactoryForgeQuadTreePointRegionForge INSTANCE =
            new EventAdvancedIndexFactoryForgeQuadTreePointRegionForge();

        private EventAdvancedIndexFactoryForgeQuadTreePointRegionForge()
        {
        }

        public override bool ProvidesIndexForOperation(string operationName)
        {
            return operationName.Equals(SettingsApplicationDotMethodPointInsideRectangle.LOOKUP_OPERATION_NAME);
        }

        public override CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory), "INSTANCE");
        }

        public override EventAdvancedIndexFactory RuntimeFactory {
            get => EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory.INSTANCE;
        }
    }
} // end of namespace