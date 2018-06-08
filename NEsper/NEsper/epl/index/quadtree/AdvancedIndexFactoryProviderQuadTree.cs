///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.spatial.quadtree.pointregion;

//import static com.espertech.esper.epl.index.quadtree.AdvancedIndexQuadTreeConstants.*;
//import static com.espertech.esper.epl.index.service.AdvancedIndexEvaluationHelper.*;
//import static com.espertech.esper.epl.index.service.AdvancedIndexValidationHelper.*;

namespace com.espertech.esper.epl.index.quadtree
{
    public abstract class AdvancedIndexFactoryProviderQuadTree : AdvancedIndexFactoryProvider
    {
        public AdvancedIndexConfigContextPartition ValidateConfigureFilterIndex(
            string indexName, string indexTypeName, ExprNode[] parameters, ExprValidationContext validationContext)
        {
            ValidateParameters(indexTypeName, parameters);
            try
            {
                return ConfigureQuadTree(indexName, ExprNodeUtility.GetEvaluators(parameters),
                    validationContext.ExprEvaluatorContext);
            }
            catch (EPException ex)
            {
                throw new ExprValidationException(ex.Message, ex);
            }
        }

        protected static void ValidateParameters(string indexTypeName, ExprNode[] parameters)
        {
            AdvancedIndexValidationHelper.ValidateParameterCount(4, 6, indexTypeName, parameters?.Length ?? 0);
            AdvancedIndexValidationHelper.ValidateParameterReturnTypeNumber(indexTypeName, 0, parameters[0],
                AdvancedIndexQuadTreeConstants.PARAM_XMIN);
            AdvancedIndexValidationHelper.ValidateParameterReturnTypeNumber(indexTypeName, 1, parameters[1],
                AdvancedIndexQuadTreeConstants.PARAM_YMIN);
            AdvancedIndexValidationHelper.ValidateParameterReturnTypeNumber(indexTypeName, 2, parameters[2],
                AdvancedIndexQuadTreeConstants.PARAM_WIDTH);
            AdvancedIndexValidationHelper.ValidateParameterReturnTypeNumber(indexTypeName, 3, parameters[3],
                AdvancedIndexQuadTreeConstants.PARAM_HEIGHT);
            if (parameters.Length > 4)
            {
                AdvancedIndexValidationHelper.ValidateParameterReturnType(typeof(int?), indexTypeName, 4, parameters[4],
                    AdvancedIndexQuadTreeConstants.PARAM_LEAFCAPACITY);
            }

            if (parameters.Length > 5)
            {
                AdvancedIndexValidationHelper.ValidateParameterReturnType(typeof(int?), indexTypeName, 5, parameters[5],
                    AdvancedIndexQuadTreeConstants.PARAM_MAXTREEHEIGHT);
            }
        }

        internal static AdvancedIndexConfigContextPartition ConfigureQuadTree(
            string indexName, ExprEvaluator[] parameters, ExprEvaluatorContext exprEvaluatorContext)
        {
            var x = AdvancedIndexEvaluationHelper.EvalDoubleParameter(parameters[0], indexName,
                AdvancedIndexQuadTreeConstants.PARAM_XMIN, exprEvaluatorContext);
            var y = AdvancedIndexEvaluationHelper.EvalDoubleParameter(parameters[1], indexName,
                AdvancedIndexQuadTreeConstants.PARAM_YMIN, exprEvaluatorContext);
            var width = AdvancedIndexEvaluationHelper.EvalDoubleParameter(parameters[2], indexName,
                AdvancedIndexQuadTreeConstants.PARAM_WIDTH, exprEvaluatorContext);
            if (width <= 0)
            {
                throw AdvancedIndexEvaluationHelper.InvalidParameterValue(indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_WIDTH, width, "value>0");
            }

            var height = AdvancedIndexEvaluationHelper.EvalDoubleParameter(parameters[3], indexName,
                AdvancedIndexQuadTreeConstants.PARAM_HEIGHT, exprEvaluatorContext);
            if (height <= 0)
            {
                throw AdvancedIndexEvaluationHelper.InvalidParameterValue(indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_HEIGHT, height, "value>0");
            }

            var leafCapacity = parameters.Length > 4
                ? AdvancedIndexEvaluationHelper.EvalIntParameter(parameters[4], indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_LEAFCAPACITY, exprEvaluatorContext)
                : PointRegionQuadTreeFactory<object>.DEFAULT_LEAF_CAPACITY;
            if (leafCapacity < 1)
            {
                throw AdvancedIndexEvaluationHelper.InvalidParameterValue(indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_LEAFCAPACITY, leafCapacity, "value>=1");
            }

            var maxTreeHeight = parameters.Length > 5
                ? AdvancedIndexEvaluationHelper.EvalIntParameter(parameters[5], indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_MAXTREEHEIGHT, exprEvaluatorContext)
                : PointRegionQuadTreeFactory<object>.DEFAULT_MAX_TREE_HEIGHT;
            if (maxTreeHeight < 2)
            {
                throw AdvancedIndexEvaluationHelper.InvalidParameterValue(indexName,
                    AdvancedIndexQuadTreeConstants.PARAM_MAXTREEHEIGHT, maxTreeHeight, "value>=2");
            }

            return new AdvancedIndexConfigContextPartitionQuadTree(x, y, width, height, leafCapacity, maxTreeHeight);
        }

        public abstract EventAdvancedIndexProvisionDesc ValidateEventIndex(
            string indexName, string indexTypeName, ExprNode[] columns, ExprNode[] parameters);
    }
} // end of namespace
