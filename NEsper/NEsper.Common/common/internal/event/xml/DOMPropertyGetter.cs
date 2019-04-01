///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Shortcut-getter for DOM underlying objects.
    /// </summary>
    public interface DOMPropertyGetter : EventPropertyGetterSPI
    {
        /// <summary>
        ///     Returns a property value as a node.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>value node</returns>
        XmlNode GetValueAsNode(XmlNode node);

        /// <summary>
        ///     Returns a property value that is indexed as a node array.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>nodes</returns>
        XmlNode[] GetValueAsNodeArray(XmlNode node);

        /// <summary>
        ///     Returns a property value as a fragment.
        /// </summary>
        /// <param name="node">to evaluate</param>
        /// <returns>fragment</returns>
        object GetValueAsFragment(XmlNode node);

        CodegenExpression GetValueAsNodeCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);

        CodegenExpression GetValueAsNodeArrayCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);

        CodegenExpression GetValueAsFragmentCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);
    }
} // end of namespace