///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.Xsl;

namespace com.espertech.esper.compat.xml
{
    public interface IXPathVariableResolver
    {
        /// <summary>
        /// Resolves the variable that is identified by the specified information.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IXsltContextVariable ResolveVariable(string prefix, string name);
    }
}
