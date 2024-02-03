///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public partial class SettingsApplicationDotMethodPointInsideRectangle : SettingsApplicationDotMethodBase
    {
        public const string LOOKUP_OPERATION_NAME = "point.inside(rectangle)";
        public const string INDEXTYPE_NAME = "pointregionquadtree";

        public SettingsApplicationDotMethodPointInsideRectangle(
            ExprDotNodeImpl parent,
            string lhsName,
            ExprNode[] lhs,
            string dotMethodName,
            string rhsName,
            ExprNode[] rhs,
            ExprNode[] indexNamedParameter)
            : base(parent, lhsName, lhs, dotMethodName, rhsName, rhs, indexNamedParameter)
        {
        }

        protected override ExprForge ValidateAll(
            string lhsName,
            ExprNode[] lhs,
            string rhsName,
            ExprNode[] rhs,
            ExprValidationContext validationContext)
        {
            EPLValidationUtil.ValidateParameterNumber(lhsName, LHS_VALIDATION_NAME, false, 2, lhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(
                lhs,
                lhsName,
                LHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            EPLValidationUtil.ValidateParameterNumber(rhsName, RHS_VALIDATION_NAME, true, 4, rhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(
                rhs,
                rhsName,
                RHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            var pxEval = lhs[0].Forge;
            var pyEval = lhs[1].Forge;
            var xEval = rhs[0].Forge;
            var yEval = rhs[1].Forge;
            var widthEval = rhs[2].Forge;
            var heightEval = rhs[3].Forge;
            return new PointIntersectsRectangleForge(parent, pxEval, pyEval, xEval, yEval, widthEval, heightEval);
        }

        protected override string OperationName => LOOKUP_OPERATION_NAME;

        protected override string IndexTypeName => INDEXTYPE_NAME;
    }
} // end of namespace