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
    public class EPStatementFormatter
    {
        private string SYSTEM_NEWLINE = Environment.NewLine;
        private const string SPACE = " ";

        private readonly bool _isNewline;
        private readonly string _newlineString;

        private string _delimiter;

        public EPStatementFormatter()
        {
            _isNewline = false;
            _newlineString = " ";
        }

        public EPStatementFormatter(bool newline)
        {
            _isNewline = newline;
            _newlineString = SYSTEM_NEWLINE;
        }

        public EPStatementFormatter(string newlineString)
        {
            _isNewline = true;
            _newlineString = newlineString;
        }

        public void BeginContext(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginAnnotation(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginExpressionDecl(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginInsertInto(
            TextWriter writer,
            bool topLevel)
        {
            WriteDelimiter(writer, topLevel);
        }

        public void BeginFromStream(
            TextWriter writer,
            bool first)
        {
            WriteDelimiter(writer, !first);
        }

        public void BeginWhere(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginGroupBy(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginHaving(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOutput(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOrderBy(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginLimit(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginFor(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOnTrigger(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginSelect(
            TextWriter writer,
            bool topLevel)
        {
            if (topLevel) {
                WriteDelimiter(writer, topLevel);
            }

            SetDelimiter();
        }

        public void BeginMerge(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginFrom(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginMergeWhere(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginMergeWhenMatched(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginMergeAction(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOnSet(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOnDelete(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginOnUpdate(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        private void SetDelimiter()
        {
            if (_isNewline) {
                _delimiter = _newlineString;
            }
            else {
                _delimiter = SPACE;
            }
        }

        private void WriteDelimiter(TextWriter writer)
        {
            if (_delimiter != null) {
                writer.Write(_delimiter);
            }

            SetDelimiter();
        }

        private void WriteDelimiter(
            TextWriter writer,
            bool topLevel)
        {
            if (_delimiter != null) {
                if (!topLevel) {
                    writer.Write(SPACE);
                }
                else {
                    writer.Write(_delimiter);
                }
            }

            SetDelimiter();
        }

        public void BeginCreateDataFlow(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateVariable(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginUpdate(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateWindow(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateContext(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateSchema(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateIndex(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginDataFlowSchema(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginDataFlowOperator(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginDataFlowOperatorDetails(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void EndDataFlowOperatorConfig(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void EndDataFlowOperatorDetails(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateExpression(TextWriter writer)
        {
            WriteDelimiter(writer);
        }

        public void BeginCreateTable(TextWriter writer)
        {
            WriteDelimiter(writer);
        }
    }
}