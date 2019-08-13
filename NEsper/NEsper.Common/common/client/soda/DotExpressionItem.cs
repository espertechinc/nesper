///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Dot-expresson item is for use in "(inner_expression).dot_expression".
    /// <para/>
    /// Each item represent an individual chain item and may either be a method name with method parameters,
    /// or a (nested) property name typically with an empty list of parameters or for mapped properties a
    /// non-empty list of parameters.
    /// </summary>
    [Serializable]
    public class DotExpressionItem
    {
        /// <summary>Ctor. </summary>
        public DotExpressionItem()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="name">the property (or nested property) or method name</param>
        /// <param name="parameters">are optional and should only be provided if this chain item is a method;Parameters are expressions for parameters to the method (use only for methods and not for properties unless mapped property). </param>
        /// <param name="isProperty">true if this is a nested property name</param>
        public DotExpressionItem(
            String name,
            IList<Expression> parameters,
            bool isProperty)
        {
            Name = name;
            IsProperty = isProperty;
            Parameters = parameters;
        }

        /// <summary>Gets or sets the method name or nested property name. </summary>
        /// <value>method name or nested property name</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets method parameters or parameters for mapped properties or empty list if this item represents a simple nested property.
        /// </summary>
        /// <value>parameter expressions</value>
        public IList<Expression> Parameters { get; set; }

        /// <summary>Returns true if this dot-item is a property name. </summary>
        /// <value>true for property, false for method</value>
        public bool IsProperty { get; set; }

        /// <summary>RenderAny to EPL. </summary>
        /// <param name="chain">chain to render</param>
        /// <param name="writer">writer to output to</param>
        /// <param name="prefixDot">indicator whether to prefix with "."</param>
        protected internal static void Render(
            IList<DotExpressionItem> chain,
            TextWriter writer,
            bool prefixDot)
        {
            var delimiterOuter = prefixDot ? "." : "";
            foreach (var item in chain)
            {
                writer.Write(delimiterOuter);
                writer.Write(item.Name);

                if (!item.IsProperty || !item.Parameters.IsEmpty())
                {
                    writer.Write("(");
                    if (!item.Parameters.IsEmpty())
                    {
                        String delimiter = "";
                        foreach (var param in item.Parameters)
                        {
                            writer.Write(delimiter);
                            delimiter = ",";
                            param.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                        }
                    }

                    writer.Write(")");
                }

                delimiterOuter = ".";
            }
        }
    }
}