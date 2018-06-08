///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.supportunit.bean
{
	public class SupportBeanPropertyNames
	{
    	public String GetA(String key) { return ""; }
    	public String GetAB(String key) { return ""; }
    	public String GetABC(String key) { return ""; }
    	public String Geta(String key) { return ""; }
    	public String Getab(String key) { return ""; }
    	public String Getabc(String key) { return ""; }
    	public String GetFooBah(String key) { return ""; }
        public String Get(String key) { return ""; }

	    public string A
	    {
	        get { return ""; }
	    }

	    public string AB
	    {
	        get { return ""; }
	    }

	    public string ABC
	    {
	        get { return ""; }
	    }

	    public string a
	    {
	        get { return ""; }
	    }

	    public string ab
	    {
	        get { return ""; }
	    }

	    public string abc
	    {
	        get { return ""; }
	    }

	    public string FooBah
	    {
	        get { return ""; }
	    }

        public int[] Array
        {
            get { return new int[0]; }
        }
        
        public String GetIndexed(int i) { return ""; }

	    public String Get() { return ""; }    
	}
}
