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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Fire-and-forget (on-demand) insert DML.
    /// <para/>
    /// The insert-into clause holds the named window name and column names. The select-clause
    /// list holds the values to be inserted.
    /// </summary>
    public class FireAndForgetInsert : FireAndForgetClause
    {
        private bool _useValuesKeyword = true;
        private IList<IList<Expression>> rows;

        /// <summary>Ctor. </summary>
        /// <param name="useValuesKeyword">whether to use the "values" keyword or whether the syntax is based on select</param>
        public FireAndForgetInsert(bool useValuesKeyword)
        {
            _useValuesKeyword = useValuesKeyword;
        }

        /// <summary>Ctor. </summary>
        public FireAndForgetInsert()
        {
        }

        public void ToEPL(TextWriter writer)
        {
            writer.Write("values ");
            var delimiter = "";
            foreach (var row in rows) {
                writer.Write(delimiter);
                RenderRow(writer, row);
                delimiter = ", ";
            }
        }

        private void RenderRow(
            TextWriter writer,
            IEnumerable<Expression> row)
        {
            writer.Write("(");
            var delimiter = "";
            foreach (var param in row) {
                writer.Write(delimiter);
                delimiter = ", ";
                param.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(")");
        }

        /// <summary>Returns indicator whether to use the values keyword. </summary>
        /// <value>indicator</value>
        public bool IsUseValuesKeyword {
            get => _useValuesKeyword;
            set => _useValuesKeyword = value;
        }

        /// <summary>
        /// Returns the rows. Only applicable when using the "values"-keyword i.e. "values (...row...), (...row...)".
        /// </summary>
        /// <value>rows wherein each row is a list of expressions</value>
        public IList<IList<Expression>> Rows {
            get => rows;
            set => rows = value;
        }
    }
}