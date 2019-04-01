///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
	public class SupportBean_StringAlphabetic
	{
        public SupportBean_StringAlphabetic(string a, string b, string c, string d, string e, string f, string g, string h, string i)
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

	    public SupportBean_StringAlphabetic(string a, string b, string c, string d, string e)
	        : this(a,b,c,d,e,null,null,null,null)
        {
	    }

	    public SupportBean_StringAlphabetic(string a, string b, string c)
	        : this(a, b, c, null, null)
        {
	    }

	    public SupportBean_StringAlphabetic(string a, string b)
	        : this(a, b, null)
        {
	    }

	    public SupportBean_StringAlphabetic(string a)
	        : this(a, null, null)
        {
	    }

        public string A { get; private set; }

        public string B { get; private set; }

        public string C { get; private set; }

        public string D { get; private set; }

        public string E { get; private set; }

        public string F { get; private set; }

        public string G { get; private set; }

        public string H { get; private set; }

        public string I { get; private set; }
	}
} // end of namespace
