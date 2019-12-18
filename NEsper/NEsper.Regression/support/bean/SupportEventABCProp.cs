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
    public class SupportEventABCProp
    {
        public SupportEventABCProp(
            string a,
            string b,
            string c,
            string d,
            string e,
            string f,
            string g,
            string h)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
            G = g;
            H = h;
        }

        public string A { get; }

        public string B { get; }

        public string C { get; }

        public string D { get; }

        public string E { get; }

        public string F { get; }

        public string G { get; }

        public string H { get; }
    }
} // end of namespace