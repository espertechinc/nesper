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
        public NameAndModule[] PathEventTypes { get; set; }

        public NameAndModule[] PathNamedWindows { get; set; }

        public NameAndModule[] PathTables { get; set; }

        public NameAndModule[] PathVariables { get; set; }

        public NameAndModule[] PathContexts { get; set; }

        public NameAndModule[] PathExpressions { get; set; }

        public string[] PublicEventTypes { get; set; }

        public string[] PublicVariables { get; set; }

        public ModuleIndexMeta[] PathIndexes { get; set; }

        public NameParamNumAndModule[] PathScripts { get; set; }
        
        public NameAndModule[] PathClasses { get; set; }
    }
} // end of namespace