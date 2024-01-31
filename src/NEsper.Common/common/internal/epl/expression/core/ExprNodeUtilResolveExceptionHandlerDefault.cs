///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilResolveExceptionHandlerDefault : ExprNodeUtilResolveExceptionHandler
    {
        private readonly bool _configuredAsSingleRow;
        private readonly string _resolvedExpression;

        public ExprNodeUtilResolveExceptionHandlerDefault(
            string resolvedExpression,
            bool configuredAsSingleRow)
        {
            _resolvedExpression = resolvedExpression;
            _configuredAsSingleRow = configuredAsSingleRow;
        }

        public ExprValidationException Handle(Exception e)
        {
            string message;
            if (e is EPException || _configuredAsSingleRow) {
                message = e.Message;
            }
            else {
                message = "Failed to resolve '" +
                          _resolvedExpression +
                          "' to a property, single-row function, aggregation function, script, stream or class name";
            }

            return new ExprValidationException(message, e);
        }
    }
}