///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ExpressionScriptCompiledImpl : ExpressionScriptCompiled
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionScriptCompiledImpl"/> class.
        /// </summary>
        /// <param name="scriptAction">The script action.</param>
        public ExpressionScriptCompiledImpl(Func<ScriptArgs, Object> scriptAction)
        {
            ScriptAction = scriptAction;
        }

        public Func<ScriptArgs, Object> ScriptAction { get; private set; }

        public Type KnownReturnType {
            get { return null; }
        }
    }
}