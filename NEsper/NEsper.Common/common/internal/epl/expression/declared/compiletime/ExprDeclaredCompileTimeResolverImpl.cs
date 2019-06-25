///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredCompileTimeResolverImpl : ExprDeclaredCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ICollection<string> moduleUses;
        private readonly ExprDeclaredCompileTimeRegistry locals;
        private readonly PathRegistry<string, ExpressionDeclItem> path;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public ExprDeclaredCompileTimeResolverImpl(
            string moduleName,
            ICollection<string> moduleUses,
            ExprDeclaredCompileTimeRegistry locals,
            PathRegistry<string, ExpressionDeclItem> path,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.locals = locals;
            this.path = path;
            this.moduleDependencies = moduleDependencies;
        }

        public ExpressionDeclItem Resolve(string name)
        {
            // try self-originated protected types first
            ExpressionDeclItem localExpr = locals.Expressions.Get(name);
            if (localExpr != null)
            {
                return localExpr;
            }

            try
            {
                var expression = path.GetAnyModuleExpectSingle(name, moduleUses);
                if (expression != null)
                {
                    if (!NameAccessModifier.Visible(expression.First.Visibility, expression.First.ModuleName, moduleName))
                    {
                        return null;
                    }

                    moduleDependencies.AddPathExpression(name, expression.Second);
                    return expression.First;
                }
            }
            catch (PathException e)
            {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.EXPRDECL, name, e);
            }

            return null;
        }
    }
} // end of namespace