///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilableSODA : Compilable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CompilableSODA(EPStatementObjectModel soda)
        {
            Soda = soda;
        }

        public EPStatementObjectModel Soda { get; }

        public string ToEPL()
        {
            try {
                return Soda.ToEPL();
            }
            catch (Exception ex) {
                Log.Debug("Failed to get EPL from SODA: " + ex.Message, ex);
                return "(cannot obtain EPL expression)";
            }
        }
    }
} // end of namespace