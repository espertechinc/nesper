///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// ParameterModel lets the user specify the way that the parameter
    /// model works for backend repositories.  ADO.NET allows providers
    /// to specify the manner in which parameters work.  This causes a
    /// great deal of ambiguity in how to deal with them in code.  This
    /// class allows the client to determine how to bind parameters to
    /// the ADO.NET provider.
    /// </summary>

    public class ParameterModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterModel"/> class.
        /// </summary>
        public ParameterModel()
        {
            this.m_prefix = "@";
            this.m_style = ParameterStyle.Named;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterModel"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="style">The style.</param>
        public ParameterModel(String prefix, ParameterStyle style)
        {
            this.m_prefix = prefix;
            this.m_style = style;
        }

        private string m_prefix;

        /// <summary>
        /// Gets or sets the prefix used before a parameter.
        /// </summary>
        /// <value>The parameter prefix.</value>
        public string Prefix
        {
            get { return m_prefix; }
            set { m_prefix = value; }
        }

        private ParameterStyle m_style;

        /// <summary>
        /// Gets or sets the parameter style.
        /// </summary>
        /// <value>The parameter style.</value>
        public ParameterStyle Style
        {
            get { return m_style; }
            set { m_style = value; }
        }

        /// <summary>
        /// Gets the formatted version of the named paramter.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public String GetNamedParameter( int index )
        {
            return String.Format("{0}arg{1}", m_prefix, index);
        }

        /// <summary>
        /// Creates the db command.
        /// </summary>
        /// <param name="parseFragments">The parse fragments.</param>
        /// <returns></returns>
        public String CreateDbCommand(IEnumerable<PlaceholderParser.Fragment> parseFragments)
        {
            int parameterCount = 0;

            StringBuilder buffer = new StringBuilder();
            foreach (PlaceholderParser.Fragment fragment in parseFragments)
            {
                if (!fragment.IsParameter)
                {
                    buffer.Append(fragment.Value);
                }
                else if ( m_style == ParameterStyle.Positional )
                {
                    buffer.Append(m_prefix);
                }
                else if ( m_style == ParameterStyle.Named )
                {
                    buffer.Append(GetNamedParameter(parameterCount++));
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Creates the a pseudo sql command that replaces parameters with
        /// question marks.  The question marks can then be parsed at the
        /// cache and converted back into native ADO.NET parameters.
        /// </summary>
        /// <param name="parseFragments">The parse fragments.</param>
        /// <returns></returns>
        public String CreatePseudoCommand(IEnumerable<PlaceholderParser.Fragment> parseFragments)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (PlaceholderParser.Fragment fragment in parseFragments)
            {
                if (fragment.IsParameter)
                {
                    buffer.Append('?');
                }
                else
                {
                    buffer.Append(fragment.Value);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Creates the db parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public void CreateDbParameters(DbCommand command, IEnumerable<String> parameterNames)
        {
            int parameterCount = 0;

            command.Parameters.Clear();

            foreach (string parameterName in parameterNames)
            {
                if (m_style == ParameterStyle.Positional)
                {
                    DbParameter paramObj = command.CreateParameter();
                    command.Parameters.Add(paramObj);
                }
                else if (m_style == ParameterStyle.Named)
                {
                    DbParameter paramObj = command.CreateParameter();
                    paramObj.ParameterName = GetNamedParameter(parameterCount++);
                    command.Parameters.Add(paramObj);
                }
            }
        }

        /// <summary>
        /// Creates the db parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parseFragments">The parse fragments.</param>
        public void CreateDbParameters( DbCommand command, IEnumerable<PlaceholderParser.Fragment> parseFragments )
        {
            int parameterCount = 0;

            command.Parameters.Clear();

            foreach (PlaceholderParser.Fragment fragment in parseFragments)
            {
                if (!fragment.IsParameter)
                {
                }
                else if (m_style == ParameterStyle.Positional)
                {
                    DbParameter paramObj = command.CreateParameter();
                    command.Parameters.Add(paramObj);
                }
                else if (m_style == ParameterStyle.Named)
                {
                    DbParameter paramObj = command.CreateParameter();
                    paramObj.ParameterName = GetNamedParameter(parameterCount++);
                    command.Parameters.Add(paramObj);
                }
            }
        }
    }

    public enum ParameterStyle
    {
        /// <summary>
        /// Provider expects parameters to be named.
        /// </summary>
        Named,
        /// <summary>
        /// Provider expects parameters to be positional.
        /// </summary>
        Positional
    }
}
