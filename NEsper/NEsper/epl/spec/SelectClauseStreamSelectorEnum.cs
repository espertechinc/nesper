///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client.soda;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Enumeration for representing select-clause selection of the remove stream or the insert stream, or both.
    /// </summary>

    public enum SelectClauseStreamSelectorEnum
    {
        /// <summary> Indicates selection of the remove stream only.</summary>
        RSTREAM_ONLY,
        /// <summary> Indicates selection of the insert stream only.</summary>
        ISTREAM_ONLY,
        /// <summary> Indicates selection of both the insert and the remove stream.  </summary>
        RSTREAM_ISTREAM_BOTH
    }

    public static class SelectClauseStreamSelectorEnumExtensions
    {
        public static bool IsSelectsRStream(this SelectClauseStreamSelectorEnum selector)
        {
            return selector != SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
        }

        public static bool IsSelectsIStream(this SelectClauseStreamSelectorEnum selector)
        {
            return selector != SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
        }

        /// <summary>Maps the SODA-selector to the internal representation</summary>
        /// <param name="selector">is the SODA-selector to map</param>
        /// <returns>internal stream selector</returns>
        public static SelectClauseStreamSelectorEnum MapFromSODA(this StreamSelector selector)
        {
            switch (selector)
            {
                case StreamSelector.ISTREAM_ONLY:
                    return SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
                case StreamSelector.RSTREAM_ONLY:
                    return SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
                case StreamSelector.RSTREAM_ISTREAM_BOTH:
                    return SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
                default:
                    throw new ArgumentException("Invalid selector '" + selector + "' encountered");
            }
        }

        /// <summary>Maps the internal stream selector to the SODA-representation</summary>
        /// <param name="selector">is the internal selector to map</param>
        /// <returns>SODA stream selector</returns>
        public static StreamSelector MapFromSODA(this SelectClauseStreamSelectorEnum selector)
        {
            switch (selector)
            {
                case SelectClauseStreamSelectorEnum.ISTREAM_ONLY:
                    return StreamSelector.ISTREAM_ONLY;
                case SelectClauseStreamSelectorEnum.RSTREAM_ONLY:
                    return StreamSelector.RSTREAM_ONLY;
                case SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH:
                    return StreamSelector.RSTREAM_ISTREAM_BOTH;
                default:
                    throw new ArgumentException("Invalid selector '" + selector + "' encountered");
            }
        }
    }
}
