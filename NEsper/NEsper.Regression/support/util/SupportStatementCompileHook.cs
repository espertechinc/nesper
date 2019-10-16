///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportStatementCompileHook : StatementCompileHook
    {
        private static readonly IList<StatementSpecCompiled> SPECS = new List<StatementSpecCompiled>();

        public void Compiled(StatementSpecCompiled compiled)
        {
            SPECS.Add(compiled);
        }

        public static string ResetGetClassName()
        {
            Reset();
            return typeof(SupportStatementCompileHook).CleanName();
        }

        public static void Reset()
        {
            SPECS.Clear();
        }

        public static IList<StatementSpecCompiled> GetSpecs()
        {
            IList<StatementSpecCompiled> copy = new List<StatementSpecCompiled>(SPECS);
            Reset();
            return copy;
        }
    }
} // end of namespace