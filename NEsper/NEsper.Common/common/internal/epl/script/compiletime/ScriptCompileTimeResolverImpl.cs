///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.compiletime
{
    public class ScriptCompileTimeResolverImpl : ScriptCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ISet<string> moduleUses;
        private readonly ScriptCompileTimeRegistry locals;
        private readonly PathRegistry<NameAndParamNum, ExpressionScriptProvided> path;
        private readonly ModuleDependenciesCompileTime moduleDependencies;

        public ScriptCompileTimeResolverImpl(
            string moduleName,
            ISet<string> moduleUses,
            ScriptCompileTimeRegistry locals,
            PathRegistry<NameAndParamNum, ExpressionScriptProvided> path,
            ModuleDependenciesCompileTime moduleDependencies)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.locals = locals;
            this.path = path;
            this.moduleDependencies = moduleDependencies;
        }

        public ExpressionScriptProvided Resolve(
            string name,
            int numParameters)
        {
            var key = new NameAndParamNum(name, numParameters);

            // try self-originated protected types first
            ExpressionScriptProvided localExpr = locals.Scripts.Get(key);
            if (localExpr != null) {
                return localExpr;
            }

            try {
                var expression = path.GetAnyModuleExpectSingle(
                    new NameAndParamNum(name, numParameters), moduleUses);
                if (expression != null) {
                    if (!NameAccessModifier.Visible(expression.First.Visibility, expression.First.ModuleName, moduleName)) {
                        return null;
                    }

                    moduleDependencies.AddPathScript(key, expression.Second);
                    return expression.First;
                }
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.SCRIPT, name, e);
            }

            return null;
        }
    }
} // end of namespace