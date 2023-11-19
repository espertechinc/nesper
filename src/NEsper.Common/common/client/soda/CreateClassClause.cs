///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// Clause for creating an application-provided class for use across one or more statements.
    /// </summary>
    public class CreateClassClause
    {
        private ClassProvidedExpression _classProvidedExpression;

        public ClassProvidedExpression ClassProvidedExpression {
            get => _classProvidedExpression;
            set => _classProvidedExpression = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CreateClassClause()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CreateClassClause(string classText)
        {
            _classProvidedExpression = new ClassProvidedExpression(classText);
        }

        /// <summary>
        /// EPL output
        /// </summary>
        /// <param name="writer">writer to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create ");
            _classProvidedExpression.ToEPL(writer);
        }
    }
}