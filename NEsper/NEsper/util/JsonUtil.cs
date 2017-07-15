///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;

namespace com.espertech.esper.util
{
    public class JsonUtil
    {
        public static Object ParsePopulate(String json, Type topClass, ExprNodeOrigin exprNodeOrigin, ExprValidationContext exprValidationContext)
        {
            var startRuleSelector = new ParseRuleSelector(parser => parser.startJsonValueRule());
            var parseResult = ParseHelper.Parse(json, json, true, startRuleSelector, false);
            var tree = (EsperEPL2GrammarParser.StartJsonValueRuleContext)parseResult.Tree;
            var parsed = ASTJsonHelper.Walk(parseResult.TokenStream, tree.jsonvalue());

            if (!(parsed is IDictionary<String, Object>))
            {
                throw new ExprValidationException(
                    "Failed to map value to object of type " + topClass.FullName +
                    ", expected Json Map/Object format, received " + (parsed != null ? parsed.GetType().Name : "null"));
            }
            var objectProperties = (IDictionary<String, Object>)parsed;
            return PopulateUtil.InstantiatePopulateObject(objectProperties, topClass, exprNodeOrigin, exprValidationContext);
        }
    }
}
