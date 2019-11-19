///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.framework
{
    public class RegressionPath
    {
        public IList<EPCompiled> Compileds { get; } = new List<EPCompiled>();

        public void Add(EPCompiled compiled)
        {
            Compileds.Add(compiled);
        }

        public void Clear()
        {
            Compileds.Clear();
        }
    }
} // end of namespace