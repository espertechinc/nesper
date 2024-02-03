///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanString
    {
        private string _theString;

        public SupportBeanString(string theString)
        {
            _theString = theString;
        }

        public string TheString {
            get => _theString;
            set => _theString = value;
        }

        public static SupportBeanString GetInstance()
        {
            return new SupportBeanString(null);
        }

        public override string ToString()
        {
            return "SupportBeanString string=" + _theString;
        }
    }
} // end of namespace