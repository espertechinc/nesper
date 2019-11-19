///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.declared.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.declared.runtime
{
    public class ExprDeclaredCollectorRuntime : ExprDeclaredCollector
    {
        private readonly IDictionary<string, ExpressionDeclItem> expressions;

        public ExprDeclaredCollectorRuntime(IDictionary<string, ExpressionDeclItem> expressions)
        {
            this.expressions = expressions;
        }

        public void RegisterExprDeclared(
            string expressionName,
            ExpressionDeclItem meta)
        {
            if (expressions.ContainsKey(expressionName)) {
                throw new IllegalStateException("Expression name already found '" + expressionName + "'");
            }

            expressions.Put(expressionName, meta);
        }
    }
} // end of namespace