///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilResolveExceptionHandlerDefault : ExprNodeUtilResolveExceptionHandler
    {
        private readonly String resolvedExpression;
        private readonly bool configuredAsSingleRow;

        public ExprNodeUtilResolveExceptionHandlerDefault(
            String resolvedExpression,
            bool configuredAsSingleRow)
        {
            this.resolvedExpression = resolvedExpression;
            this.configuredAsSingleRow = configuredAsSingleRow;
        }

        public ExprValidationException Handle(Exception e)
        {
            String message;
            if (configuredAsSingleRow) {
                message = e.Message;
            }
            else {
                message = "Failed to resolve '" +
                          resolvedExpression +
                          "' to a property, single-row function, aggregation function, script, stream or class name";
            }

            return new ExprValidationException(message, e);
        }
    }
}