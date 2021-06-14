///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class ISupportAImplSuperGImplPlus : ISupportAImplSuperG,
        ISupportB,
        ISupportC
    {
        public ISupportAImplSuperGImplPlus()
        {
        }

        public ISupportAImplSuperGImplPlus(
            string valueG,
            string valueA,
            string valueBaseAB,
            string valueB,
            string valueC)
        {
            G = valueG;
            A = valueA;
            BaseAB = valueBaseAB;
            B = valueB;
            C = valueC;
        }

        public override string G { get; }

        public override string A { get; }

        public override string BaseAB { get; }

        public string B { get; }

        public string C { get; }
    }
} // end of namespace