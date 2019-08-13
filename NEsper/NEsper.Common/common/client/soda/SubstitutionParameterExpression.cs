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
    /// Represents a substitution parameter
    /// </summary>
    [Serializable]
    public class SubstitutionParameterExpression : ExpressionBase
    {
        private string optionalName;
        private string optionalType;

        /// <summary>
        /// Ctor.
        /// </summary>
        public SubstitutionParameterExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="optionalName">name of the substitution parameter or null if none provided</param>
        /// <param name="optionalType">type of the substitution parameter or null if none provided</param>
        public SubstitutionParameterExpression(
            string optionalName,
            string optionalType)
        {
            this.optionalName = optionalName;
            this.optionalType = optionalType;
        }

        /// <summary>
        /// Returns the name when provided
        /// </summary>
        /// <returns>name</returns>
        public string OptionalName
        {
            get => optionalName;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("?");
            if (optionalName != null)
            {
                writer.Write(":");
                writer.Write(optionalName);
                if (optionalType != null)
                {
                    writer.Write(":");
                    writer.Write(optionalType);
                }
            }
            else if (optionalType != null)
            {
                writer.Write("::");
                writer.Write(optionalType);
            }
        }

        /// <summary>
        /// Sets the name
        /// </summary>
        /// <param name="optionalName">name</param>
        public void SetOptionalName(string optionalName)
        {
            this.optionalName = optionalName;
        }

        /// <summary>
        /// Returns the type when provided
        /// </summary>
        /// <returns>type</returns>
        public string OptionalType
        {
            get => optionalType;
        }

        /// <summary>
        /// Sets the type
        /// </summary>
        /// <param name="optionalType">type</param>
        public void SetOptionalType(string optionalType)
        {
            this.optionalType = optionalType;
        }
    }
} // end of namespace