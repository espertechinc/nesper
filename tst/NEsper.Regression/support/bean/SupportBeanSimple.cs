///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanSimple
    {
        private int _myInt;
        private string _myString;

        public SupportBeanSimple(
            string myString,
            int myInt)
        {
            _myString = myString;
            _myInt = myInt;
        }

        public string MyString {
            get => _myString;
            set => _myString = value;
        }

        public int MyInt {
            get => _myInt;
            set => _myInt = value;
        }
    }
} // end of namespace