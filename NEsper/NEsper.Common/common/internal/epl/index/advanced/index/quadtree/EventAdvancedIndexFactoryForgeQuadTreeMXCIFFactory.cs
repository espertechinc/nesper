///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory : EventAdvancedIndexFactoryForgeQuadTreeFactory
    {
        public static readonly EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory INSTANCE =
            new EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory();

        private EventAdvancedIndexFactoryForgeQuadTreeMXCIFFactory()
        {
        }

        public override EventAdvancedIndexFactoryForge Forge =>
            EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge.INSTANCE;

        public override EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configCP,
            EventTableOrganization organization)
        {
            var qt = (AdvancedIndexConfigContextPartitionQuadTree) configCP;
            var quadTree = MXCIFQuadTreeFactory<object>.Make(
                qt.X,
                qt.Y,
                qt.Width,
                qt.Height,
                qt.LeafCapacity,
                qt.MaxTreeHeight);
            return new EventTableQuadTreeMXCIFImpl(
                organization,
                (AdvancedIndexConfigStatementMXCIFQuadtree) configStatement,
                quadTree);
        }

        public override EventAdvancedIndexConfigStatementForge ToConfigStatement(ExprNode[] indexedExpr)
        {
            return new AdvancedIndexConfigStatementMXCIFQuadtreeForge(
                indexedExpr[0].Forge,
                indexedExpr[1].Forge,
                indexedExpr[2].Forge,
                indexedExpr[3].Forge);
        }
    }
} // end of namespace