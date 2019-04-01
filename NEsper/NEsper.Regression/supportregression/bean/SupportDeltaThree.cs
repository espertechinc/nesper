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
    [Serializable]
    public class SupportDeltaThree
    {
        private readonly String k0;
        private readonly String p0;
        private readonly String p4;
    
        public SupportDeltaThree(String k0, String p0, String p4)
        {
            this.k0 = k0;
            this.p0 = p0;
            this.p4 = p4;
        }

        public string K0
        {
            get { return k0; }
        }

        public string P0
        {
            get { return p0; }
        }

        public string P4
        {
            get { return p4; }
        }
    }
}
