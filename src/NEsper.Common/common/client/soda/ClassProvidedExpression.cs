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
    [Serializable]
    public class ClassProvidedExpression
    {
        private string _classText;

        public string ClassText {
            get => _classText;
            set => _classText = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ClassProvidedExpression()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="classText">indicates the class text.</param>
        public ClassProvidedExpression(string classText)
        {
            _classText = classText;
        }

        /// <summary>
        /// Print part.
        /// </summary>
        public void ToEPL(TextWriter writer) {
            writer.Write("inlined_class ");
            writer.Write("\"\"\"");
            writer.Write(_classText);
            writer.Write("\"\"\"");
        }

        /// <summary>
        /// Print.
        /// </summary>
        /// <param name="writer">output writer</param>
        /// <param name="classProvidedList">list of class provided expressions</param>
        /// <param name="formatter">formatter for newline-whitespace formatting</param>
        public static void ToEPL(TextWriter writer, IList<ClassProvidedExpression> classProvidedList, EPStatementFormatter formatter) {
            if ((classProvidedList == null) || (classProvidedList.IsEmpty())) {
                return;
            }

            foreach (ClassProvidedExpression part in classProvidedList) {
                if (part.ClassText == null) {
                    continue;
                }
                formatter.BeginExpressionDecl(writer);
                part.ToEPL(writer);
            }
        }
    }
}