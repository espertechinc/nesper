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

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredCollectorCompileTime : ExprDeclaredCollector
    {
        private readonly IDictionary<string, ExpressionDeclItem> moduleExpressions;

        public ExprDeclaredCollectorCompileTime(IDictionary<string, ExpressionDeclItem> moduleExpressions)
        {
            this.moduleExpressions = moduleExpressions;
        }

        public void RegisterExprDeclared(
            string expressionName,
            ExpressionDeclItem meta)
        {
            moduleExpressions.Put(expressionName, meta);
        }
    }
} // end of namespace