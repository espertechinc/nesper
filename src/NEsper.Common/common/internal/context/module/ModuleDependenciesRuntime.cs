///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleDependenciesRuntime
    {
        public NameAndModule[] PathEventTypes { get; set; } = new NameAndModule[0];

        public NameAndModule[] PathNamedWindows { get; set; } = new NameAndModule[0];

        public NameAndModule[] PathTables { get; set; } = new NameAndModule[0];

        public NameAndModule[] PathVariables { get; set; } = new NameAndModule[0];

        public NameAndModule[] PathContexts { get; set; } = new NameAndModule[0];

        public NameAndModule[] PathExpressions { get; set; } = new NameAndModule[0];

        public string[] PublicEventTypes { get; set; } = new string[0];

        public string[] PublicVariables { get; set; } = new string[0];

        public ModuleIndexMeta[] PathIndexes { get; set; } = new ModuleIndexMeta[0];

        public NameParamNumAndModule[] PathScripts { get; set; } = new NameParamNumAndModule[0];
        
        public NameAndModule[] PathClasses { get; set; } = new NameAndModule[0];
    }
} // end of namespace