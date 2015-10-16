///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
    [Serializable]
    public class SupportRevisionFull : ISupportRevisionFull
    {
        private readonly String k0;
        private readonly String p0;
        private readonly String p1;
        private readonly String p2;
        private readonly String p3;
        private readonly String p4;
        private readonly String p5;
    
        public SupportRevisionFull(String k0, String p0, String p1, String p2, String p3, String p4, String p5)
        {
            this.k0 = k0;
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
            this.p5 = p5;
        }
    
        public SupportRevisionFull(String k0, String p1, String p5)
        {
            this.k0 = k0;
            this.p0 = null;
            this.p1 = p1;
            this.p2 = null;
            this.p3 = null;
            this.p4 = null;
            this.p5 = p5;
        }

        public string K0
        {
            get { return k0; }
        }

        public string P0
        {
            get { return p0; }
        }

        public string P1
        {
            get { return p1; }
        }

        public string P2
        {
            get { return p2; }
        }

        public string P3
        {
            get { return p3; }
        }

        public string P4
        {
            get { return p4; }
        }

        public string P5
        {
            get { return p5; }
        }
    }
}
