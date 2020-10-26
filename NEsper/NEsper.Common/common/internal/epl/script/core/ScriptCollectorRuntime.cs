///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptCollectorRuntime : ScriptCollector
    {
        private readonly IDictionary<NameAndParamNum, ExpressionScriptProvided> scripts;

        public ScriptCollectorRuntime(IDictionary<NameAndParamNum, ExpressionScriptProvided> scripts)
        {
            this.scripts = scripts;
        }

        public void RegisterScript(
            string scriptName,
            int numParameters,
            ExpressionScriptProvided meta)
        {
            var key = new NameAndParamNum(scriptName, numParameters);
            if (scripts.ContainsKey(key)) {
                throw new IllegalStateException("Script already found '" + key + "'");
            }

            scripts.Put(key, meta);
        }
    }
} // end of namespace