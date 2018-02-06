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

//import static com.espertech.esper.epl.index.service.AdvancedIndexValidationHelper.validateColumnCount;
//import static com.espertech.esper.epl.index.service.AdvancedIndexValidationHelper.validateColumnReturnTypeNumber;

namespace com.espertech.esper.epl.index.quadtree
{
    public class AdvancedIndexFactoryProviderMXCIFQuadTree : AdvancedIndexFactoryProviderQuadTree
    {
        public override EventAdvancedIndexProvisionDesc ValidateEventIndex(
            string indexName, 
            string indexTypeName, 
            ExprNode[] columns, 
            ExprNode[] parameters)
        {
            AdvancedIndexValidationHelper.ValidateColumnCount(4, indexTypeName, columns.Length);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 0, columns[0], AdvancedIndexQuadTreeConstants.COL_X);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 1, columns[1], AdvancedIndexQuadTreeConstants.COL_Y);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 2, columns[2], AdvancedIndexQuadTreeConstants.COL_WIDTH);
            AdvancedIndexValidationHelper.ValidateColumnReturnTypeNumber(indexTypeName, 3, columns[3], AdvancedIndexQuadTreeConstants.COL_HEIGHT);
    
            ValidateParameters(indexTypeName, parameters);
    
            var indexDesc = new AdvancedIndexDesc(indexTypeName, columns);
            var xEval = indexDesc.IndexedExpressions[0].ExprEvaluator;
            var yEval = indexDesc.IndexedExpressions[1].ExprEvaluator;
            var widthEval = indexDesc.IndexedExpressions[2].ExprEvaluator;
            var heightEval = indexDesc.IndexedExpressions[3].ExprEvaluator;
            var indexStatementConfigs = new AdvancedIndexConfigStatementMXCIFQuadtree(xEval, yEval, widthEval, heightEval);
    
            return new EventAdvancedIndexProvisionDesc(
                indexDesc, ExprNodeUtility.GetEvaluators(parameters), 
                EventAdvancedIndexFactoryQuadTreeMXCIF.INSTANCE, 
                indexStatementConfigs);
        }
    }
} // end of namespace
