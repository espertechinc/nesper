///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredCompileTimeResolverEmpty : ExprDeclaredCompileTimeResolver
    {
        public readonly static ExprDeclaredCompileTimeResolverEmpty INSTANCE = new ExprDeclaredCompileTimeResolverEmpty();

        private ExprDeclaredCompileTimeResolverEmpty()
        {
        }

        public ExpressionDeclItem Resolve(string name)
        {
            return null;
        }

        public ExpressionScriptProvided ResolveScript(
            string name,
            int numParameters)
        {
            return null;
        }
    }
} // end of namespace