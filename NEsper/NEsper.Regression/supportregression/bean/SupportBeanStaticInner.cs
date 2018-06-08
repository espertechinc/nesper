///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.bean
{
    public class SupportBeanStaticInner
    {
        public SupportBeanStaticInnerTwo InsideTwo
        {
            get { return new SupportBeanStaticInnerTwo(); }
        }

        public static string GetMyString()
        {
            return "hello";
        }
    }
}