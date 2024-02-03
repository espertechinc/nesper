///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBean_StringAlphabetic
    {
        public SupportBean_StringAlphabetic(
            string a,
            string b,
            string c,
            string d,
            string e,
            string f,
            string g,
            string h,
            string i)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
            G = g;
            H = h;
            I = i;
        }

        public SupportBean_StringAlphabetic(
            string a,
            string b,
            string c,
            string d,
            string e) : this(a, b, c, d, e, null, null, null, null)
        {
        }

        public SupportBean_StringAlphabetic(
            string a,
            string b,
            string c) : this(a, b, c, null, null)
        {
        }

        public SupportBean_StringAlphabetic(
            string a,
            string b) : this(a, b, null)
        {
        }

        public SupportBean_StringAlphabetic(string a) : this(a, null, null)
        {
        }

        public string A { get; }

        public string B { get; }

        public string C { get; }

        public string D { get; }

        public string E { get; }

        public string F { get; }

        public string G { get; }

        public string H { get; }

        public string I { get; }
    }
} // end of namespace