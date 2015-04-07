///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// For use in expression as a placeholder to represent its child nodes.
    /// </summary>
    [Serializable]
    public class ExpressionPlaceholder : ExpressionBase
    {
        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if ((Children == null) || (Children.Count == 0)) {
                return;
            }
            Children[0].ToEPL(writer, Precedence);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.MINIMUM; }
        }
    }
}
