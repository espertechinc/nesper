///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;

namespace com.espertech.esper.common.@internal.epl.expression.declared.core
{
    public interface ExprDeclaredCollector
    {
        void RegisterExprDeclared(
            string expressionName,
            ExpressionDeclItem meta);
    }
} // end of namespace