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
    public class EngineImportApplicationDotMethodRectangeIntersectsRectangle
        : EngineImportApplicationDotMethodBase
    {
        public const string LOOKUP_OPERATION_NAME = "rectangle.Intersects(rectangle)";
        public const string INDEX_TYPE_NAME = "mxcifquadtree";

        public EngineImportApplicationDotMethodRectangeIntersectsRectangle(
            string lhsName,
            IList<ExprNode> lhs,
            string dotMethodName,
            string rhsName,
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
            EPLValidationUtil.ValidateParameterNumber(lhsName, LHS_VALIDATION_NAME, false, 4, lhs.Count);
            EPLValidationUtil.ValidateParametersTypePredefined(lhs, lhsName, LHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            EPLValidationUtil.ValidateParameterNumber(rhsName, RHS_VALIDATION_NAME, true, 4, rhs.Count);
            EPLValidationUtil.ValidateParametersTypePredefined(rhs, rhsName, RHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            var meXEval = lhs[0].ExprEvaluator;
            var meYEval = lhs[1].ExprEvaluator;
            var meWidthEval = lhs[2].ExprEvaluator;
            var meHeightEval = lhs[3].ExprEvaluator;

            var otherXEval = rhs[0].ExprEvaluator;
            var otherYEval = rhs[1].ExprEvaluator;
            var otherWidthEval = rhs[2].ExprEvaluator;
            var otherHeightEval = rhs[3].ExprEvaluator;
            return new RectangleIntersectsRectangleEvaluator(meXEval, meYEval, meWidthEval, meHeightEval, otherXEval,
                otherYEval, otherWidthEval, otherHeightEval);
        }

        public class RectangleIntersectsRectangleEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator _meHeightEval;
            private readonly ExprEvaluator _meWidthEval;
            private readonly ExprEvaluator _meXEval;
            private readonly ExprEvaluator _meYEval;
            private readonly ExprEvaluator _otherHeightEval;
            private readonly ExprEvaluator _otherWidthEval;
            private readonly ExprEvaluator _otherXEval;
            private readonly ExprEvaluator _otherYEval;

            public RectangleIntersectsRectangleEvaluator(ExprEvaluator meXEval, ExprEvaluator meYEval,
                ExprEvaluator meWidthEval, ExprEvaluator meHeightEval, ExprEvaluator otherXEval,
                ExprEvaluator otherYEval, ExprEvaluator otherWidthEval, ExprEvaluator otherHeightEval)
            {
                _meXEval = meXEval;
                _meYEval = meYEval;
                _meWidthEval = meWidthEval;
                _meHeightEval = meHeightEval;
                _otherXEval = otherXEval;
                _otherYEval = otherYEval;
                _otherWidthEval = otherWidthEval;
                _otherHeightEval = otherHeightEval;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                var meX = _meXEval.Evaluate(evaluateParams);
                if (meX == null) return null;
                var meY = _meYEval.Evaluate(evaluateParams);
                if (meY == null) return null;
                var meWidth = _meWidthEval.Evaluate(evaluateParams);
                if (meWidth == null) return null;
                var meHeight = _meHeightEval.Evaluate(evaluateParams);
                if (meHeight == null) return null;
                var otherX = _otherXEval.Evaluate(evaluateParams);
                if (otherX == null) return null;
                var otherY = _otherYEval.Evaluate(evaluateParams);
                if (otherY == null) return null;
                var otherWidth = _otherWidthEval.Evaluate(evaluateParams);
                if (otherWidth == null) return null;
                var otherHeight = _otherHeightEval.Evaluate(evaluateParams);
                if (otherHeight == null) return null;

                var x = meX.AsDouble();
                var y = meY.AsDouble();
                var width = meWidth.AsDouble();
                var height = meHeight.AsDouble();
                return BoundingBox.IntersectsBoxIncludingEnd(
                    x, y, x + width, y + height,
                    otherX.AsDouble(),
                    otherY.AsDouble(),
                    otherWidth.AsDouble(),
                    otherHeight.AsDouble());
            }

            public Type ReturnType => typeof(bool?);
        }
    }
} // end of namespace