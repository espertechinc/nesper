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
    /// <summary>Filter for use in pattern expressions. </summary>
    [Serializable]
    public class PatternFilterExpr : PatternExprBase
    {
        /// <summary>Ctor. </summary>
        public PatternFilterExpr()
        {
        }

        /// <summary> Ctor. </summary>
        /// <param name="filter">specifies to events to filter out</param>
        public PatternFilterExpr(Filter filter)
            : this(filter, null)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="filter">specifies to events to filter out</param>
        /// <param name="tagName">specifies the name of the tag to assigned to matching events</param>
        public PatternFilterExpr(
            Filter filter,
            string tagName)
        {
            TagName = tagName;
            Filter = filter;
        }

        /// <summary>Returns the tag name. </summary>
        /// <value>tag name.</value>
        public string TagName { get; set; }

        /// <summary>Returns the filter specification. </summary>
        /// <value>filter</value>
        public Filter Filter { get; set; }

        public override PatternExprPrecedenceEnum Precedence
        {
            get { return PatternExprPrecedenceEnum.ATOM; }
        }

        /// <summary>Returns the consume level, if assigned. </summary>
        /// <value>consume level</value>
        public int? OptionalConsumptionLevel { get; set; }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            if (TagName != null)
            {
                writer.Write(TagName);
                writer.Write('=');
            }

            Filter.ToEPL(writer, formatter);
            if (OptionalConsumptionLevel != null)
            {
                writer.Write("@consume");
                if (OptionalConsumptionLevel != 1)
                {
                    writer.Write("(");
                    writer.Write(Convert.ToString(OptionalConsumptionLevel));
                    writer.Write(")");
                }
            }
        }
    }
}