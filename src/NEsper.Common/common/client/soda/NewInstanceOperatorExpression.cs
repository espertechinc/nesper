///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.util;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// The "new instance" operator instantiates a host language object.
    /// <para>
    ///   Set a array dimension value greater zero for new array.
    ///   If the child node is a single {@link ArrayExpression}, the expression is "new array[] {...}".
    ///   If the child node is not a single {@link ArrayExpression}, the expression is "new array[...][...]".
    ///   For 2-dimensionnal array initialization, put {@link ArrayExpression} inside {@link ArrayExpression},
    ///   i.e. the expression is "new array[] {{...}, {...}}".
    /// </para>
    /// </summary>
    [Serializable]
    public class NewInstanceOperatorExpression : ExpressionBase
    {
        private string className;
        private int numArrayDimensions;

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
        /// Ctor.
        /// </summary>
        /// <param name="className">the class name</param>
        /// <param name="numArrayDimensions">dimensions for array initialization.</param>
        public NewInstanceOperatorExpression(
            string className,
            int numArrayDimensions)
        {
            this.className = className;
            this.numArrayDimensions = numArrayDimensions;
        }

        /// <summary>
        /// Returns the class name.
        /// </summary>
        /// <returns>class name</returns>
        public string ClassName {
            get => className;
            set => className = value;
        }

        /// <summary>
        /// Gets or sets the array dimension; with child nodes providing either dimensions or array initialization values.
        /// </summary>
        public int NumArrayDimensions {
            get => numArrayDimensions;
            set => numArrayDimensions = value;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <param name="className">class name to set</param>
        public NewInstanceOperatorExpression SetClassName(string className)
        {
            this.className = className;
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("new ");
            
            if (IdentifierUtil.IsGenericOrNestedTypeName(className)) {
                writer.Write('`');
                writer.Write(className);
                writer.Write('`');
            }
            else {
                writer.Write(className);
            }

            if (numArrayDimensions == 0) {
                writer.Write("(");
                ExpressionBase.ToPrecedenceFreeEPL(Children, writer);
                writer.Write(")");
            }
            else {
                if (Children.Count == 1 && Children[0] is ArrayExpression) {
                    for (int i = 0; i < numArrayDimensions; i++) {
                        writer.Write("[]");
                    }
                    writer.Write(" ");
                    Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                } else {
                    foreach (Expression expression in Children) {
                        writer.Write("[");
                        expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                        writer.Write("]");
                    }
                    if (numArrayDimensions > Children.Count) {
                        writer.Write("[]");
                    }
                }
            }
        }
    }
} // end of namespace