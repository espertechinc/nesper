///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.compiletime
{
    public class ScriptCollectorCompileTime : ScriptCollector
    {
        private readonly IDictionary<NameAndParamNum, ExpressionScriptProvided> _moduleScripts;

        public ScriptCollectorCompileTime(IDictionary<NameAndParamNum, ExpressionScriptProvided> moduleScripts)
        {
            this._moduleScripts = moduleScripts;
        }

        public void RegisterScript(
            string scriptName,
            int numParams,
            ExpressionScriptProvided meta)
        {
            _moduleScripts.Put(new NameAndParamNum(scriptName, numParams), meta);
        }
    }
} // end of namespace