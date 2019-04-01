///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Context descriptor for categories.
    /// </summary>
    public class ContextDescriptorCategoryItem : ContextDescriptor
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ContextDescriptorCategoryItem()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="expression">category expression</param>
        /// <param name="label">category label</param>
        public ContextDescriptorCategoryItem(Expression expression,
                                             String label)
        {
            Expression = expression;
            Label = label;
        }

        /// <summary>Returns the category expression. </summary>
        /// <value>expression</value>
        public Expression Expression { get; set; }

        /// <summary>Returns the category label </summary>
        /// <value>category label</value>
        public string Label { get; set; }

        #region ContextDescriptor Members

        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write("group ");
            Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(" as ");
            writer.Write(Label);
        }

        #endregion
    }
}