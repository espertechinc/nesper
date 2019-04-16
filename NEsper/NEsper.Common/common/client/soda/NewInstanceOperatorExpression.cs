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
    /// The "new instance" operator instantiates a host language object.
    /// </summary>
    public class NewInstanceOperatorExpression : ExpressionBase
    {
        private string className;

        /// <summary>
        /// Ctor.
        /// </summary>
        public NewInstanceOperatorExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// <para /></summary>
        /// <param name="className">the class name</param>
        public NewInstanceOperatorExpression(string className)
        {
            this.className = className;
        }

        /// <summary>
        /// Returns the class name.
        /// </summary>
        /// <returns>class name</returns>
        public string ClassName {
            get => className;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <param name="className">class name to set</param>
        public void SetClassName(string className)
        {
            this.className = className;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("new ");
            writer.Write(className);
            writer.Write("(");
            ExpressionBase.ToPrecedenceFreeEPL(this.Children, writer);
            writer.Write(")");
        }
    }
} // end of namespace