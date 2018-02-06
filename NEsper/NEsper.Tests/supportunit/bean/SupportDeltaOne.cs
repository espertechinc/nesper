///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportunit.bean
{
    [Serializable]
    public class SupportDeltaOne
    {
        private readonly String k0;
        private readonly String p1;
        private readonly String p5;
    
        public SupportDeltaOne(String k0, String p1, String p5)
        {
            this.k0 = k0;
            this.p1 = p1;
            this.p5 = p5;
        }

        public string K0
        {
            get { return k0; }
        }

        public string P1
        {
            get { return p1; }
        }

        public string P5
        {
            get { return p5; }
        }
    }
}
