///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleDependenciesRuntime
    {
        public NameAndModule[] PathEventTypes { get; set; } = NameAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathNamedWindows { get; set; } = NameAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathTables { get; set; } = NameAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathVariables { get; set; } = NameAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathContexts { get; set; } = NameAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathExpressions { get; set; } = NameAndModule.EMPTY_ARRAY;

        public string[] PublicEventTypes { get; set; } = CollectionUtil.STRINGARRAY_EMPTY;

        public string[] PublicVariables { get; set; } = CollectionUtil.STRINGARRAY_EMPTY;

        public ModuleIndexMeta[] PathIndexes { get; set; } = ModuleIndexMeta.EMPTY_ARRAY;

        public NameParamNumAndModule[] PathScripts { get; set; } = NameParamNumAndModule.EMPTY_ARRAY;

        public NameAndModule[] PathClasses { get; set; } = NameAndModule.EMPTY_ARRAY;
    }
} // end of namespace