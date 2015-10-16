///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regression.script
{
    public class MyImportedClass {
        public readonly static String VALUE_P00 = "VALUE_P00";

        private readonly String _p00 = VALUE_P00;

        public string P00
        {
            get { return _p00; }
        }
    }
}
