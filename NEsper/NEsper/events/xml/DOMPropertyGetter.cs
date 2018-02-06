///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Shortcut-getter for DOM underlying objects.
    /// </summary>
    public interface DOMPropertyGetter : EventPropertyGetterSPI
    {
        /// <summary>
        /// Returns a property value as a node.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// value node
        /// </returns>
        XmlNode GetValueAsNode(XmlNode node);

        /// <summary>
        /// Returns a property value as a node.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// value node
        /// </returns>
        XObject GetValueAsNode(XObject node);
    
        /// <summary>
        /// Returns a property value that is indexed as a node array.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// nodes
        /// </returns>
        XmlNode[] GetValueAsNodeArray(XmlNode node);

        /// <summary>
        /// Returns a property value that is indexed as a node array.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// nodes
        /// </returns>
        XObject[] GetValueAsNodeArray(XObject node);

        /// <summary>
        /// Returns a property value as a fragment.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// fragment
        /// </returns>
        Object GetValueAsFragment(XmlNode node);

        /// <summary>
        /// Returns a property value as a fragment.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>
        /// fragment
        /// </returns>
        Object GetValueAsFragment(XObject node);

        ICodegenExpression GetValueAsNodeCodegen(ICodegenExpression value, ICodegenContext context);
        ICodegenExpression GetValueAsNodeArrayCodegen(ICodegenExpression value, ICodegenContext context);
        ICodegenExpression GetValueAsFragmentCodegen(ICodegenExpression value, ICodegenContext context);
    }
}
