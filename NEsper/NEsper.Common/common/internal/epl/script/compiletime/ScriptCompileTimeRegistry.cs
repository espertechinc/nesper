///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.compiletime
{
    public class ScriptCompileTimeRegistry : CompileTimeRegistry
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptCompileTimeRegistry" /> class.
        /// </summary>
        public ScriptCompileTimeRegistry()
        {
            Scripts = new Dictionary<NameAndParamNum, ExpressionScriptProvided>();
        }

        /// <summary>
        /// Gets the dictionary of scripts.
        /// </summary>
        public IDictionary<NameAndParamNum, ExpressionScriptProvided> Scripts { get; }

        public void NewScript(ExpressionScriptProvided detail)
        {
            if (!detail.Visibility.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for contexts");
            }

            var key = new NameAndParamNum(detail.Name, detail.ParameterNames.Length);
            var existing = Scripts.Get(key);
            if (existing != null) {
                throw new IllegalStateException("Duplicate script has been encountered for name '" + key + "'");
            }

            Scripts.Put(key, detail);
        }
    }
} // end of namespace