///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.lookup;
using static com.espertech.esper.common.@internal.epl.index.advanced.index.service.AdvancedIndexValidationHelper;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class AdvancedIndexFactoryProviderMXCIFQuadTree : AdvancedIndexFactoryProviderQuadTree
    {
        public override EventAdvancedIndexProvisionCompileTime ValidateEventIndex(
            string indexName,
            string indexTypeName,
            ExprNode[] columns,
            ExprNode[] parameters)
        {
            ValidateColumnCount(4, indexTypeName, columns.Length);
            ValidateColumnReturnTypeNumber(indexTypeName, 0, columns[0], AdvancedIndexQuadTreeConstants.COL_X);
            ValidateColumnReturnTypeNumber(indexTypeName, 1, columns[1], AdvancedIndexQuadTreeConstants.COL_Y);
            ValidateColumnReturnTypeNumber(indexTypeName, 2, columns[2], AdvancedIndexQuadTreeConstants.COL_WIDTH);
            ValidateColumnReturnTypeNumber(indexTypeName, 3, columns[3], AdvancedIndexQuadTreeConstants.COL_HEIGHT);

            ValidateParameters(indexTypeName, parameters);

            var indexDesc = new AdvancedIndexDescWExpr(indexTypeName, columns);
            var xEval = indexDesc.IndexedExpressions[0].Forge;
            var yEval = indexDesc.IndexedExpressions[1].Forge;
            var widthEval = indexDesc.IndexedExpressions[2].Forge;
            var heightEval = indexDesc.IndexedExpressions[3].Forge;
            var indexStatementConfigs =
                new AdvancedIndexConfigStatementMXCIFQuadtreeForge(xEval, yEval, widthEval, heightEval);

            return new EventAdvancedIndexProvisionCompileTime(
                indexDesc, parameters, EventAdvancedIndexFactoryForgeQuadTreeMXCIFForge.INSTANCE,
                indexStatementConfigs);
        }
    }
} // end of namespace