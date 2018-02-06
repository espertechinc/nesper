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
    public class SupportOverrideOne : SupportOverrideBase
    {
        override public String Val
        {
            get { return valOne; }
        }

        private String valOne;

        public SupportOverrideOne(String valOne, String valBase)
            : base(valBase)
        {
            this.valOne = valOne;
        }
    }
}
