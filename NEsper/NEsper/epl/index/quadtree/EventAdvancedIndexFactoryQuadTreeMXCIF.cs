///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.epl.index.quadtree
{
    public class EventAdvancedIndexFactoryQuadTreeMXCIF : EventAdvancedIndexFactoryQuadTree
    {
        public static readonly EventAdvancedIndexFactoryQuadTreeMXCIF INSTANCE =
            new EventAdvancedIndexFactoryQuadTreeMXCIF();

        private EventAdvancedIndexFactoryQuadTreeMXCIF()
        {
        }

        public override bool ProvidesIndexForOperation(string operationName, IDictionary<int, ExprNode> value)
        {
            return operationName.Equals(EngineImportApplicationDotMethodRectangeIntersectsRectangle
                .LOOKUP_OPERATION_NAME);
        }

        public override EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configCP,
            EventTableOrganization organization)
        {
            var qt = (AdvancedIndexConfigContextPartitionQuadTree) configCP;
            var quadTree =
                MXCIFQuadTreeFactory<object>.Make(qt.X, qt.Y, qt.Width, qt.Height, qt.LeafCapacity, qt.MaxTreeHeight);
            return new EventTableQuadTreeMXCIFImpl(organization,
                (AdvancedIndexConfigStatementMXCIFQuadtree) configStatement, quadTree);
        }
    }
} // end of namespace
