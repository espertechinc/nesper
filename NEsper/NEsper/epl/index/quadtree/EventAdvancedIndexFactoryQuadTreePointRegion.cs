///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.epl.index.quadtree
{
    public class EventAdvancedIndexFactoryQuadTreePointRegion : EventAdvancedIndexFactoryQuadTree
    {
        public static readonly EventAdvancedIndexFactoryQuadTreePointRegion INSTANCE = 
            new EventAdvancedIndexFactoryQuadTreePointRegion();
    
        private EventAdvancedIndexFactoryQuadTreePointRegion() {}
    
        public override bool ProvidesIndexForOperation(string operationName, IDictionary<int, ExprNode> value) {
            return operationName.Equals(EngineImportApplicationDotMethodPointInsideRectangle.LOOKUP_OPERATION_NAME);
        }
    
        public override EventTable Make(
            EventAdvancedIndexConfigStatement configStatement, 
            AdvancedIndexConfigContextPartition configCP, 
            EventTableOrganization organization)
        {
            var qt = (AdvancedIndexConfigContextPartitionQuadTree) configCP;
            var quadTree = PointRegionQuadTreeFactory<object>.Make(qt.X, qt.Y, qt.Width, qt.Height, qt.LeafCapacity, qt.MaxTreeHeight);
            return new EventTableQuadTreePointRegionImpl(organization, (AdvancedIndexConfigStatementPointRegionQuadtree) configStatement, quadTree);
        }
    }
} // end of namespace
