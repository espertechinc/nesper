///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class SupportPatternCompileHook : PatternCompileHook
    {
        private static readonly IList<EvalRootForgeNode> roots = new List<EvalRootForgeNode>();

        public void Pattern(EvalRootForgeNode root)
        {
            roots.Add(root);
        }

        public static EvalRootForgeNode GetOneAndReset()
        {
            Assert.AreEqual(1, roots.Count);
            return roots.DeleteAt(0);
        }

        public static void Reset()
        {
            roots.Clear();
        }
    }
} // end of namespace