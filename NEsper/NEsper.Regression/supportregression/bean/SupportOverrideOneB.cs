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
    public class SupportOverrideOneB : SupportOverrideOne
    {
        override public String Val
        {
            get { return valOneB; }
        }

        private String valOneB;

        public SupportOverrideOneB(String valOneB, String valOne, String valBase)
            : base(valOne, valBase)
        {
            this.valOneB = valOneB;
        }
    }
}
