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
    public class ISupportAImplSuperGImpl : ISupportAImplSuperG
    {
        override public String G
        {
            get { return valueG; }
        }
        override public String A
        {
            get { return valueA; }
        }
        override public String BaseAB
        {
            get { return valueBaseAB; }
        }
        private String valueG;
        private String valueA;
        private String valueBaseAB;

        public ISupportAImplSuperGImpl(String valueG, String valueA, String valueBaseAB)
        {
            this.valueG = valueG;
            this.valueA = valueA;
            this.valueBaseAB = valueBaseAB;
        }
    }
}
