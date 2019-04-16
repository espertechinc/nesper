///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Expression returning a property value.
    /// </summary>
    [Serializable]
    public class PropertyValueExpression : ExpressionBase
    {
        private string propertyName;

        /// <summary>
        /// Ctor.
        /// </summary>
        public PropertyValueExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the name of the property</param>
        public PropertyValueExpression(string propertyName)
        {
            this.propertyName = propertyName.Trim();
        }

        /// <summary>
        /// Returns the property name.
        /// </summary>
        /// <returns>name of the property</returns>
        public string PropertyName {
            get => propertyName;
        }

        /// <summary>
        /// Sets the property name.
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        public void SetPropertyName(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(propertyName);
        }
    }
} // end of namespace