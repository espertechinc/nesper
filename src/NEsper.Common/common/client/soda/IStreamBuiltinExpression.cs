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
    /// Returns true for insert stream and false for remove stream, same as the "istream()" builtin function.
    /// </summary>
    [Serializable]
    public class IStreamBuiltinExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public IStreamBuiltinExpression()
        {
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("istream()");
        }
    }
} // end of namespace