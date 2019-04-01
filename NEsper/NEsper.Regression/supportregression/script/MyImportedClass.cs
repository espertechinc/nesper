///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.supportregression.script
{
    public class MyImportedClass {
        public static readonly string VALUE_P00 = "VALUE_P00";
        private readonly string p00 = VALUE_P00;
    
        public MyImportedClass() {
        }
    
        public string GetP00() {
            return p00;
        }
    }
} // end of namespace
