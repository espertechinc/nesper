///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.namedwindow.compile;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleDependenciesCompileTime
    {
        private readonly ICollection<NameAndModule> pathContexts = new HashSet<NameAndModule>();
        private readonly ICollection<NameAndModule> pathEventTypes = new HashSet<NameAndModule>();
        private readonly ICollection<NameAndModule> pathExpressions = new HashSet<NameAndModule>();
        private readonly ICollection<ModuleIndexMeta> pathIndexes = new HashSet<ModuleIndexMeta>();
        private readonly ICollection<NameAndModule> pathNamedWindows = new HashSet<NameAndModule>();
        private readonly ICollection<NameParamNumAndModule> pathScripts = new HashSet<NameParamNumAndModule>();
        private readonly ICollection<NameAndModule> pathClasses = new HashSet<NameAndModule>();
        private readonly ICollection<NameAndModule> pathTables = new HashSet<NameAndModule>();
        private readonly ICollection<NameAndModule> pathVariables = new HashSet<NameAndModule>();
        private readonly ICollection<string> publicEventTypes = new HashSet<string>();
        private readonly ICollection<string> publicVariables = new HashSet<string>();

        public void AddPathEventType(
            string eventTypeName,
            string moduleName)
        {
            pathEventTypes.Add(new NameAndModule(eventTypeName, moduleName));
        }

        public void AddPathNamedWindow(
            string namedWindowName,
            string moduleName)
        {
            pathNamedWindows.Add(new NameAndModule(namedWindowName, moduleName));
        }

        public void AddPathTable(
            string tableName,
            string moduleName)
        {
            pathTables.Add(new NameAndModule(tableName, moduleName));
        }

        public void AddPathVariable(
            string variableName,
            string moduleName)
        {
            pathVariables.Add(new NameAndModule(variableName, moduleName));
        }

        public void AddPathContext(
            string contextName,
            string moduleName)
        {
            pathContexts.Add(new NameAndModule(contextName, moduleName));
        }

        public void AddPathExpression(
            string expressionName,
            string moduleName)
        {
            pathExpressions.Add(new NameAndModule(expressionName, moduleName));
        }

        public void AddPathScript(
            NameAndParamNum key,
            string moduleName)
        {
            pathScripts.Add(new NameParamNumAndModule(key.Name, key.ParamNum, moduleName));
        }

        public void AddPublicEventType(string eventTypeName)
        {
            publicEventTypes.Add(eventTypeName);
        }

        public void AddPublicVariable(string variableName)
        {
            publicVariables.Add(variableName);
        }

        public void AddPathClass(
            string className,
            string moduleName)
        {
            pathClasses.Add(new NameAndModule(className, moduleName));
        }

        public void AddPathIndex(
            bool namedWindow,
            string infraName,
            string infraModuleName,
            string indexName,
            string indexModuleName,
            NamedWindowCompileTimeRegistry namedWindowCompileTimeRegistry,
            TableCompileTimeRegistry tableCompileTimeRegistry)
        {
            if (indexName == null) { // ignore unnamed non-explicit indexes
                return;
            }

            if (!namedWindow && infraName.Equals(indexName)) {
                return; // not tracking primary key index as a dependency
            }

            if (namedWindow && namedWindowCompileTimeRegistry.NamedWindows.Get(infraName) != null) {
                return; // ignore when the named window was registered in the same EPL
            }

            if (!namedWindow && tableCompileTimeRegistry.Tables.Get(infraName) != null) {
                return; // ignore when the table was registered in the same EPL
            }

            pathIndexes.Add(new ModuleIndexMeta(namedWindow, infraName, infraModuleName, indexName, indexModuleName));
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ModuleDependenciesRuntime), GetType(), classScope);
            method.Block
                .DeclareVar<ModuleDependenciesRuntime>("md", NewInstance(typeof(ModuleDependenciesRuntime)))
                .SetProperty(Ref("md"), "PathEventTypes", NameAndModule.MakeArray(pathEventTypes))
                .SetProperty(Ref("md"), "PathNamedWindows", NameAndModule.MakeArray(pathNamedWindows))
                .SetProperty(Ref("md"), "PathTables", NameAndModule.MakeArray(pathTables))
                .SetProperty(Ref("md"), "PathVariables", NameAndModule.MakeArray(pathVariables))
                .SetProperty(Ref("md"), "PathContexts", NameAndModule.MakeArray(pathContexts))
                .SetProperty(Ref("md"), "PathExpressions", NameAndModule.MakeArray(pathExpressions))
                .SetProperty(Ref("md"), "PathIndexes", ModuleIndexMeta.MakeArray(pathIndexes))
                .SetProperty(Ref("md"), "PathScripts", NameParamNumAndModule.MakeArray(pathScripts))
                .SetProperty(Ref("md"), "PathClasses", NameAndModule.MakeArray(pathClasses))
                .SetProperty(Ref("md"), "PublicEventTypes", Constant(publicEventTypes.ToArray()))
                .SetProperty(Ref("md"), "PublicVariables", Constant(publicVariables.ToArray()))
                .MethodReturn(Ref("md"));
            return LocalMethod(method);
        }

        [Obsolete]
        public CodegenBlock Inject(CodegenBlock block)
        {
            return block
                .DeclareVar<ModuleDependenciesRuntime>("md", NewInstance(typeof(ModuleDependenciesRuntime)))
                .SetProperty(Ref("md"), "PathEventTypes", NameAndModule.MakeArray(pathEventTypes))
                .SetProperty(Ref("md"), "PathNamedWindows", NameAndModule.MakeArray(pathNamedWindows))
                .SetProperty(Ref("md"), "PathTables", NameAndModule.MakeArray(pathTables))
                .SetProperty(Ref("md"), "PathVariables", NameAndModule.MakeArray(pathVariables))
                .SetProperty(Ref("md"), "PathContexts", NameAndModule.MakeArray(pathContexts))
                .SetProperty(Ref("md"), "PathExpressions", NameAndModule.MakeArray(pathExpressions))
                .SetProperty(Ref("md"), "PathIndexes", ModuleIndexMeta.MakeArray(pathIndexes))
                .SetProperty(Ref("md"), "PathScripts", NameParamNumAndModule.MakeArray(pathScripts))
                .SetProperty(Ref("md"), "PathClasses", NameAndModule.MakeArray(pathClasses))
                .SetProperty(Ref("md"), "PublicEventTypes", Constant(publicEventTypes.ToArray()))
                .SetProperty(Ref("md"), "PublicVariables", Constant(publicVariables.ToArray()))
                .BlockReturn(Ref("md"));
        }
    }
} // end of namespace