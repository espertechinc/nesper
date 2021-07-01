///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.declared.core
{
    public class ExprDeclaredCacheKeyGlobal
    {
        public ExprDeclaredCacheKeyGlobal(
            string deploymentIdExpr,
            string expressionName)
        {
            DeploymentIdExpr = deploymentIdExpr;
            ExpressionName = expressionName;
        }

        public string DeploymentIdExpr { get; }

        public string ExpressionName { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprDeclaredCacheKeyGlobal) o;

            if (!DeploymentIdExpr.Equals(that.DeploymentIdExpr)) {
                return false;
            }

            return ExpressionName.Equals(that.ExpressionName);
        }

        public override int GetHashCode()
        {
            var result = DeploymentIdExpr.GetHashCode();
            result = 31 * result + ExpressionName.GetHashCode();
            return result;
        }
    }
} // end of namespace