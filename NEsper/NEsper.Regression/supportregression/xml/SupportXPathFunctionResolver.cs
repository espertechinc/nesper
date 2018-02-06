///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;
using System.Xml.Xsl;
using com.espertech.esper.compat.xml;

namespace com.espertech.esper.supportregression.xml
{
    public class SupportXPathFunctionResolver : IXPathFunctionResolver
    {
        public IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
        {
            return null;
        }
    }
}
