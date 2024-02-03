///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredCompileTimeRegistry : CompileTimeRegistry
    {
        public IDictionary<string, ExpressionDeclItem> Expressions { get; } =
            new Dictionary<string, ExpressionDeclItem>();

        public void NewExprDeclared(ExpressionDeclItem detail)
        {
            if (!detail.Visibility.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for contexts");
            }

            var name = detail.Name;
            var existing = Expressions.Get(name);
            if (existing != null) {
                throw new IllegalStateException("Duplicate declared expression encountered for name '" + name + "'");
            }

            Expressions.Put(name, detail);
        }
    }
} // end of namespace