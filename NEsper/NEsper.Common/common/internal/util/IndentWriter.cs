///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Writer that uses an underlying PrintWriter to indent output text for easy reading.
    /// </summary>
    public class IndentWriter
    {
        private readonly TextWriter _writer;
        private readonly int _deltaIndent;
        private int _currentIndent;

        /// <summary> Ctor.</summary>
        /// <param name="writer">to output to
        /// </param>
        /// <param name="startIndent">is the depth of indent to Start
        /// </param>
        /// <param name="deltaIndent">is the number of characters to indent for every incrIndent() call
        /// </param>
        public IndentWriter(
            TextWriter writer,
            int startIndent,
            int deltaIndent)
        {
            if (startIndent < 0) {
                throw new ArgumentException("Invalid Start indent");
            }

            if (deltaIndent < 0) {
                throw new ArgumentException("Invalid delta indent");
            }

            _writer = writer;
            _deltaIndent = deltaIndent;
            _currentIndent = startIndent;
        }

        /// <summary> Increase the indentation one level.</summary>
        public virtual void IncrIndent()
        {
            _currentIndent += _deltaIndent;
        }

        /// <summary> Decrease the indentation one level.</summary>
        public virtual void DecrIndent()
        {
            _currentIndent -= _deltaIndent;
        }

        /// <summary> Print text to the underlying writer.</summary>
        /// <param name="text">to print
        /// </param>
        public virtual void WriteLine(String text)
        {
            int indent = _currentIndent;
            if (indent < 0) {
                indent = 0;
            }

            _writer.WriteLine(Indent.CreateIndent(indent) + text);
        }
    }
}