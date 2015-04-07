///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regression.rowrecog
{
    public class SupportRecogBean
    {
        public SupportRecogBean(string theString)
        {
            TheString = theString;
        }

        public SupportRecogBean(string stringValue, int value)
        {
            TheString = stringValue;
            Value = value;
        }

        public SupportRecogBean(string stringValue, string cat, int value)
        {
            TheString = stringValue;
            Cat = cat;
            Value = value;
        }

        public string TheString { get; set; }

        public string Cat { get; set; }

        public int Value { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return TheString;
        }
    }
}
