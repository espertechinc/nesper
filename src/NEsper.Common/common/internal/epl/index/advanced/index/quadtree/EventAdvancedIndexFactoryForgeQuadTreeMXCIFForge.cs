///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge : EventAdvancedIndexFactoryForgeQuadTreeForge
    {
        public static readonly EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge INSTANCE =
            new EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge();

        private EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge()
        {
        }

        public override EventAdvancedIndexFactory RuntimeFactory =>
            EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory.INSTANCE;

        public override bool ProvidesIndexForOperation(string operationName)
        {
            return operationName.Equals(SettingsApplicationDotMethodRectangeIntersectsRectangle.LOOKUP_OPERATION_NAME);
        }

        public override CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory), "INSTANCE");
        }
    }
} // end of namespace