///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

namespace com.espertech.esper.compat.xml
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Gets the declaration (if any) from the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static XmlDeclaration GetDeclaration(this XmlDocument document)
        {
            foreach (var node in document.ChildNodes)
            {
                if ( node is XmlDeclaration ) {
                    return (XmlDeclaration) node;
                }
            }

            return null;
        }
    }
}
