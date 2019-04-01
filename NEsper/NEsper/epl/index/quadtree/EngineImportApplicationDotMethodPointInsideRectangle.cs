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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.util;
using com.espertech.esper.spatial.quadtree.core;

namespace com.espertech.esper.epl.index.quadtree
{
    public class EngineImportApplicationDotMethodPointInsideRectangle : EngineImportApplicationDotMethodBase
    {
        public const string LOOKUP_OPERATION_NAME = "point.Inside(rectangle)";
        public const string INDEX_TYPE_NAME = "pointregionquadtree";

        public EngineImportApplicationDotMethodPointInsideRectangle(
            string lhsName,
            IList<ExprNode> lhs,
            string dotMethodName, string rhsName,
            IList<ExprNode> rhs,
            IList<ExprNode> indexNamedParameter)
            : base(lhsName, lhs, dotMethodName, rhsName, rhs, indexNamedParameter)
        {
        }

        protected override string OperationName => LOOKUP_OPERATION_NAME;

        protected override string IndexTypeName => INDEX_TYPE_NAME;

        protected override ExprEvaluator ValidateAll(
            string lhsName,
            IList<ExprNode> lhs,
            string rhsName,
            IList<ExprNode> rhs,
            ExprValidationContext validationContext)
        {
            EPLValidationUtil.ValidateParameterNumber(lhsName, LHS_VALIDATION_NAME, false, 2, lhs.Count);
            EPLValidationUtil.ValidateParametersTypePredefined(lhs, lhsName, LHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            EPLValidationUtil.ValidateParameterNumber(rhsName, RHS_VALIDATION_NAME, true, 4, rhs.Count);
            EPLValidationUtil.ValidateParametersTypePredefined(rhs, rhsName, RHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            var pxEval = lhs[0].ExprEvaluator;
            var pyEval = lhs[1].ExprEvaluator;
            var xEval = rhs[0].ExprEvaluator;
            var yEval = rhs[1].ExprEvaluator;
            var widthEval = rhs[2].ExprEvaluator;
            var heightEval = rhs[3].ExprEvaluator;
            return new PointIntersectsRectangleEvaluator(pxEval, pyEval, xEval, yEval, widthEval, heightEval);
        }

        public class PointIntersectsRectangleEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator _heightEval;
            private readonly ExprEvaluator _pxEval;
            private readonly ExprEvaluator _pyEval;
            private readonly ExprEvaluator _widthEval;
            private readonly ExprEvaluator _xEval;
            private readonly ExprEvaluator _yEval;

            internal PointIntersectsRectangleEvaluator(ExprEvaluator pxEval, ExprEvaluator pyEval, ExprEvaluator xEval,
                ExprEvaluator yEval, ExprEvaluator widthEval, ExprEvaluator heightEval)
            {
                _pxEval = pxEval;
                _pyEval = pyEval;
                _xEval = xEval;
                _yEval = yEval;
                _widthEval = widthEval;
                _heightEval = heightEval;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                var px = _pxEval.Evaluate(evaluateParams);
                if (px == null) return null;
                var py = _pyEval.Evaluate(evaluateParams);
                if (py == null) return null;
                var x = _xEval.Evaluate(evaluateParams);
                if (x == null) return null;
                var y = _yEval.Evaluate(evaluateParams);
                if (y == null) return null;
                var width = _widthEval.Evaluate(evaluateParams);
                if (width == null) return null;
                var height = _heightEval.Evaluate(evaluateParams);
                if (height == null) return null;
                return BoundingBox.ContainsPoint(
                    x.AsDouble(), 
                    y.AsDouble(),
                    width.AsDouble(), 
                    height.AsDouble(),
                    px.AsDouble(),
                    py.AsDouble());
            }

            public Type ReturnType => typeof(bool?);
        }
    }
} // end of namespace