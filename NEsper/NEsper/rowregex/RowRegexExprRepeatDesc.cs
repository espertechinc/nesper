///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.rowregex
{
    [Serializable]
    public class RowRegexExprRepeatDesc : MetaDefItem
    {
        public RowRegexExprRepeatDesc(ExprNode lower, ExprNode upper, ExprNode single)
        {
            Lower = lower;
            Upper = upper;
            Single = single;
        }

        public ExprNode Lower { get; private set; }

        public ExprNode Upper { get; private set; }

        public ExprNode Single { get; private set; }

        public void ToExpressionString(TextWriter writer)
        {
            writer.Write("{");
            if (Single != null)
            {
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(Single));
            }
            else
            {
                if (Lower != null)
                {
                    writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(Lower));
                }
                writer.Write(",");
                if (Upper != null)
                {
                    writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(Upper));
                }
            }
            writer.Write("}");
        }
    }
} // end of namespace
