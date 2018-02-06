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
	public class SupportBean_IntAlphabetic
	{
        public SupportBean_IntAlphabetic(int a, int b, int c, int d, int e, int f, int g, int h, int i)
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

	    public SupportBean_IntAlphabetic(int a, int b, int c, int d, int e)
	        : this(a,b,c,d,e,-1,-1,-1,-1)
        {
	    }

	    public SupportBean_IntAlphabetic(int a, int b, int c, int d)
	        : this(a, b, c, d, -1)
        {
	    }

	    public SupportBean_IntAlphabetic(int a, int b, int c)
	        : this(a, b, c, -1, -1)
        {
	    }

	    public SupportBean_IntAlphabetic(int a, int b)
	        : this(a, b, -1)
        {
	    }

	    public SupportBean_IntAlphabetic(int a)
	        : this(a, -1)
        {
	    }

        public int A { get; private set; }

        public int B { get; private set; }

        public int C { get; private set; }

        public int D { get; private set; }

        public int E { get; private set; }

        public int F { get; private set; }

        public int G { get; private set; }

        public int H { get; private set; }

        public int I { get; private set; }
	}
} // end of namespace
