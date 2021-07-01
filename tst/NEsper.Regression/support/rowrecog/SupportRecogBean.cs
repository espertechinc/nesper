///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.rowrecog
{
    [Serializable]
    public class SupportRecogBean
    {
        public SupportRecogBean(string theString)
        {
            TheString = theString;
        }

        public SupportRecogBean(
            string theString,
            int value)
        {
            TheString = theString;
            Value = value;
        }

        public SupportRecogBean(
            string theString,
            string cat,
            int value)
        {
            TheString = theString;
            Cat = cat;
            Value = value;
        }

        public string TheString { get; set; }

        public string Cat { get; set; }

        public int Value { get; set; }

        public override string ToString()
        {
            return TheString;
        }
    }
} // end of namespace