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
	public class SupportBeanDupProperty
	{
        private readonly String _myProperty;
        private readonly String _MyProperty;
        private readonly String _MYPROPERTY;
        private readonly String _myproperty;

	    public SupportBeanDupProperty(String myProperty, String MyProperty, String MYPROPERTY, String myproperty)
	    {
            this._myProperty = myProperty;
            this._MyProperty = MyProperty;
            this._MYPROPERTY = MYPROPERTY;
	        this._myproperty = myproperty;
	    }

        public String myproperty
        {
            get { return this._myproperty; }
        }

        public String myProperty
        {
            get { return this._myProperty; }
        }

        public String MyProperty
	    {
            get { return this._MyProperty; }
	    }

        public String MYPROPERTY
        {
            get { return this._MYPROPERTY; }
        }

	}
} // End of namespace
