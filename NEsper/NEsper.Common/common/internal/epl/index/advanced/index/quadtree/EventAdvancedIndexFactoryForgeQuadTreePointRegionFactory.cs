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
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory : EventAdvancedIndexFactoryForgeQuadTreeFactory
    {
        public readonly static EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory INSTANCE =
            new EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory();

        private EventAdvancedIndexFactoryForgeQuadTreePointRegionFactory()
        {
        }

        public override EventAdvancedIndexFactoryForge Forge {
            get => EventAdvancedIndexFactoryForgeQuadTreePointRegionForge.INSTANCE;
        }

        public override EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configCP,
            EventTableOrganization organization)
        {
            AdvancedIndexConfigContextPartitionQuadTree qt = (AdvancedIndexConfigContextPartitionQuadTree) configCP;
            PointRegionQuadTree<object> quadTree = PointRegionQuadTreeFactory.Make(
                qt.X, qt.Y, qt.Width, qt.Height, qt.LeafCapacity, qt.MaxTreeHeight);
            return new EventTableQuadTreePointRegionImpl(organization, (AdvancedIndexConfigStatementPointRegionQuadtree) configStatement, quadTree);
        }

        public override EventAdvancedIndexConfigStatementForge ToConfigStatement(ExprNode[] indexedExpr)
        {
            return new AdvancedIndexConfigStatementPointRegionQuadtreeForge(
                indexedExpr[0].Forge,
                indexedExpr[1].Forge);
        }
    }
} // end of namespace