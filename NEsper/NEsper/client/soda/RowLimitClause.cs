///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Specification object for a row limit.
    /// </summary>
    [Serializable]
    public class RowLimitClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowLimitClause"/> class.
        /// </summary>
        public RowLimitClause()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="numRows">maximum number of rows</param>
        /// <param name="optionalOffsetRows">offset</param>
        /// <param name="numRowsVariable">name of the variable providing the maximum number of rows</param>
        /// <param name="optionalOffsetRowsVariable">name of the variable providing the offset</param>
        public RowLimitClause(int? numRows,
                              int? optionalOffsetRows,
                              string numRowsVariable,
                              string optionalOffsetRowsVariable)
        {
            NumRows = numRows;
            OptionalOffsetRows = optionalOffsetRows;
            NumRowsVariable = numRowsVariable;
            OptionalOffsetRowsVariable = optionalOffsetRowsVariable;
        }

        /// <summary>
        /// Returns the maximum number of rows, or null if using variable.
        /// </summary>
        /// <returns>
        /// max num rows
        /// </returns>
        public int? NumRows { get; set; }

        /// <summary>
        /// Returns the offset, or null if using variable or not using offset.
        /// </summary>
        /// <returns>
        /// offset
        /// </returns>
        public int? OptionalOffsetRows { get; set; }

        /// <summary>
        /// Returns the variable providing maximum number of rows, or null if using
        /// constant.
        /// </summary>
        /// <returns>
        /// max num rows variable
        /// </returns>
        public string NumRowsVariable { get; set; }

        /// <summary>
        /// Returns the name of the variable providing offset values.
        /// </summary>
        /// <returns>
        /// variable name for offset
        /// </returns>
        public string OptionalOffsetRowsVariable { get; set; }

        /// <summary>
        /// Creates a row limit clause.
        /// </summary>
        /// <param name="numRowsVariable">name of the variable providing the maximum number of rows</param>
        /// <returns>
        /// clause
        /// </returns>
        public static RowLimitClause Create(String numRowsVariable)
        {
            return new RowLimitClause(null, null, numRowsVariable, null);
        }

        /// <summary>
        /// Creates a row limit clause.
        /// </summary>
        /// <param name="numRowsVariable">name of the variable providing the maximum number of rows</param>
        /// <param name="offsetVariable">name of the variable providing the offset</param>
        /// <returns>
        /// clause
        /// </returns>
        public static RowLimitClause Create(String numRowsVariable, String offsetVariable)
        {
            return new RowLimitClause(null, null, numRowsVariable, offsetVariable);
        }

        /// <summary>
        /// Creates a row limit clause.
        /// </summary>
        /// <param name="numRows">maximum number of rows</param>
        /// <returns>
        /// clause
        /// </returns>
        public static RowLimitClause Create(int numRows)
        {
            return new RowLimitClause(numRows, null, null, null);
        }

        /// <summary>
        /// Creates a row limit clause.
        /// </summary>
        /// <param name="numRows">maximum number of rows</param>
        /// <param name="offset">offset</param>
        /// <returns>
        /// clause
        /// </returns>
        public static RowLimitClause Create(int numRows, int offset)
        {
            return new RowLimitClause(numRows, offset, null, null);
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            var numRowsVariable = NumRowsVariable;
            if (numRowsVariable != null)
            {
                writer.Write(numRowsVariable);
            }
            else
            {
                var numRows = NumRows;
                if (numRows != null)
                {
                    writer.Write(numRows);
                }
                else
                {
                    writer.Write(Int32.MaxValue);
                }
            }

            if (OptionalOffsetRowsVariable != null)
            {
                writer.Write(" offset ");
                writer.Write(OptionalOffsetRowsVariable);
            }
            else if (OptionalOffsetRows.GetValueOrDefault(0) != 0)
            {
                writer.Write(" offset ");
                writer.Write(OptionalOffsetRows);
            }
        }
    }
}
