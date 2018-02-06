///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.lookup;

// import static com.espertech.esper.epl.index.service.AdvancedIndexValidationHelper.validateColumnCount;
// import static com.espertech.esper.epl.index.service.AdvancedIndexValidationHelper.validateColumnReturnTypeNumber;

namespace com.espertech.esper.epl.index.quadtree
{
    public class AdvancedIndexFactoryProviderPointRegionQuadTree : AdvancedIndexFactoryProviderQuadTree
    {
        public override EventAdvancedIndexProvisionDesc ValidateEventIndex(
            string indexName, string indexTypeName, ExprNode[] columns, ExprNode[] parameters)
        {
            AdvancedIndexValidationHelper.ValidateColumnCount(2, indexTypeName, columns.Length);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 0, columns[0], AdvancedIndexQuadTreeConstants.COL_X);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 1, columns[1], AdvancedIndexQuadTreeConstants.COL_Y);
    
            ValidateParameters(indexTypeName, parameters);
    
            var indexDesc = new AdvancedIndexDesc(indexTypeName, columns);
            ExprEvaluator xEval = indexDesc.IndexedExpressions[0].ExprEvaluator;
            ExprEvaluator yEval = indexDesc.IndexedExpressions[1].ExprEvaluator;
            var indexStatementConfigs = new AdvancedIndexConfigStatementPointRegionQuadtree(xEval, yEval);
    
            return new EventAdvancedIndexProvisionDesc(
                indexDesc, ExprNodeUtility.GetEvaluators(parameters), 
                EventAdvancedIndexFactoryQuadTreePointRegion.INSTANCE, 
                indexStatementConfigs);
        }
    }
} // end of namespace
