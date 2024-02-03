///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.compat
{
    public class StringJoiner
    {
        private string _delimiter;
        private bool _includeDelimiter;
        private readonly StringBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringJoiner"/> class.
        /// </summary>
        /// <param name="delimiter">The delimiter.</param>
        public StringJoiner(string delimiter)
        {
            _delimiter = delimiter;
            _includeDelimiter = false;
            _builder = new StringBuilder();
        }

        /// <summary>
        /// Gets or sets the delimiter.
        /// </summary>
        /// <value>
        /// The delimiter.
        /// </value>
        public string Delimiter
        {
            get => _delimiter;
            set => _delimiter = value;
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(string value)
        {
            if (_includeDelimiter)
            {
                _builder.Append(_delimiter);
            }

            _builder.Append(value);
            _includeDelimiter = true;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}