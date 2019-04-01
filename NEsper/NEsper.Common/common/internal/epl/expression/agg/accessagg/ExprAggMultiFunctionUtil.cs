///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    public class ExprAggMultiFunctionUtil
    {
        public static int ValidateStreamWildcardGetStreamNum(ExprNode node)
        {
            if (!(node is ExprStreamUnderlyingNode))
            {
                throw new IllegalStateException("Expression not stream-wildcard");
            }
            ExprStreamUnderlyingNode und = (ExprStreamUnderlyingNode)node;
            if (und.StreamId == -1)
            {
                throw new ExprValidationException("The expression does not resolve to a stream");
            }
            return und.StreamId;
        }

        public static void ValidateWildcardStreamNumbers(StreamTypeService streamTypeService, string aggFuncName)
        {
            CheckWildcardNotJoinOrSubquery(streamTypeService, aggFuncName);
            CheckWildcardHasStream(streamTypeService, aggFuncName);
        }

        public static void CheckWildcardNotJoinOrSubquery(StreamTypeService streamTypeService, string aggFuncName)
        {
            if (streamTypeService.StreamNames.Length > 1)
            {
                throw new ExprValidationException(GetErrorPrefix(aggFuncName) + " requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead");
            }
        }

        private static void CheckWildcardHasStream(StreamTypeService streamTypeService, string aggFuncName)
        {
            if (streamTypeService.StreamNames.Length == 0)
            {    // could be the case for
                throw new ExprValidationException(GetErrorPrefix(aggFuncName) + " requires that at least one stream is provided");
            }
        }

        public static string GetErrorPrefix(string aggFuncName)
        {
            return "The '" + aggFuncName + "' aggregation function";
        }
    }
} // end of namespace