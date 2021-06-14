///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtMgmtCallablePair
    {
        public StmtMgmtCallablePair(
            string epl,
            EPCompiled compiled)
        {
            Epl = epl;
            Compiled = compiled;
        }

        public string Epl { get; }

        public EPCompiled Compiled { get; }
    }
} // end of namespace