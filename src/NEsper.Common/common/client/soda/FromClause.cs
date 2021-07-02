///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     The from-clause names the streams to select upon.
    ///     <para />
    ///     The most common projected stream is a filter-based stream which is created by <seealso cref="FilterStream" />.
    ///     <para />
    ///     Multiple streams can be joined by adding each stream individually.
    ///     <para />
    ///     Outer joins are also handled by this class. To create an outer join consisting of 2 streams,
    ///     add one <seealso cref="OuterJoinQualifier" /> that defines the outer join relationship between the 2 streams. The
    ///     outer joins between
    ///     N streams, add N-1 <seealso cref="OuterJoinQualifier" /> qualifiers.
    /// </summary>
    [Serializable]
    public class FromClause
    {
        private IList<OuterJoinQualifier> outerJoinQualifiers;
        private IList<Stream> streams;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public FromClause()
        {
            streams = new List<Stream>();
            outerJoinQualifiers = new List<OuterJoinQualifier>();
        }

        /// <summary>
        ///     Ctor for an outer join between two streams.
        /// </summary>
        /// <param name="streamOne">first stream in outer join</param>
        /// <param name="outerJoinQualifier">type of outer join and fields joined on</param>
        /// <param name="streamTwo">second stream in outer join</param>
        public FromClause(
            Stream streamOne,
            OuterJoinQualifier outerJoinQualifier,
            Stream streamTwo)
            : this(streamOne)
        {
            Add(streamTwo);
            outerJoinQualifiers.Add(outerJoinQualifier);
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamsList">is zero or more streams in the from-clause.</param>
        public FromClause(params Stream[] streamsList)
        {
            streams = new List<Stream>();
            outerJoinQualifiers = new List<OuterJoinQualifier>();
            streams.AddAll(streamsList);
        }

        /// <summary>
        ///     Returns the list of streams in the from-clause.
        /// </summary>
        /// <returns>list of streams</returns>
        public IList<Stream> Streams
        {
            get => streams;
            set => streams = value;
        }

        /// <summary>
        ///     Returns the outer join descriptors, if this is an outer join, or an empty list if
        ///     none of the streams are outer joined.
        /// </summary>
        /// <returns>list of outer join qualifiers</returns>
        public IList<OuterJoinQualifier> OuterJoinQualifiers
        {
            get => outerJoinQualifiers;
            set => outerJoinQualifiers = value;
        }

        /// <summary>
        ///     Creates an empty from-clause to which one adds streams via the add methods.
        /// </summary>
        /// <returns>empty from clause</returns>
        public static FromClause Create()
        {
            return new FromClause();
        }

        /// <summary>
        ///     Creates a from-clause that lists 2 projected streams joined via outer join.
        /// </summary>
        /// <param name="stream">first stream in outer join</param>
        /// <param name="outerJoinQualifier">qualifies the outer join</param>
        /// <param name="streamSecond">second stream in outer join</param>
        /// <returns>from clause</returns>
        public static FromClause Create(
            Stream stream,
            OuterJoinQualifier outerJoinQualifier,
            Stream streamSecond)
        {
            return new FromClause(stream, outerJoinQualifier, streamSecond);
        }

        /// <summary>
        ///     Creates a from clause that selects from a single stream.
        ///     <para />
        ///     Use <seealso cref="FilterStream" /> to create filter-based streams to add.
        /// </summary>
        /// <param name="streams">is one or more streams to add to the from clause.</param>
        /// <returns>from clause</returns>
        public static FromClause Create(params Stream[] streams)
        {
            return new FromClause(streams);
        }

        /// <summary>
        ///     Adds a stream.
        ///     <para />
        ///     Use <seealso cref="FilterStream" /> to add filter-based streams.
        /// </summary>
        /// <param name="stream">to add</param>
        /// <returns>from clause</returns>
        public FromClause Add(Stream stream)
        {
            streams.Add(stream);
            return this;
        }

        /// <summary>
        ///     Adds an outer join descriptor that defines how the streams are related via outer joins.
        ///     <para />
        ///     For joining N streams, add N-1 outer join qualifiers.
        /// </summary>
        /// <param name="outerJoinQualifier">is the type of outer join and the fields in the outer join</param>
        /// <returns>from clause</returns>
        public FromClause Add(OuterJoinQualifier outerJoinQualifier)
        {
            outerJoinQualifiers.Add(outerJoinQualifier);
            return this;
        }

        /// <summary>
        ///     Renders the from-clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public virtual void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            ToEPLOptions(writer, formatter, true);
        }

        /// <summary>
        ///     Renders the from-clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="includeFrom">flag whether to add the "from" literal</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public virtual void ToEPLOptions(
            TextWriter writer,
            EPStatementFormatter formatter,
            bool includeFrom)
        {
            if (streams.IsEmpty()) {
                return;
            }
            
            var delimiter = "";
            if (includeFrom)
            {
                formatter.BeginFrom(writer);
                writer.Write("from");
            }

            if (outerJoinQualifiers == null || outerJoinQualifiers.Count == 0)
            {
                var first = true;
                foreach (var stream in streams)
                {
                    writer.Write(delimiter);
                    formatter.BeginFromStream(writer, first);
                    first = false;
                    stream.ToEPL(writer, formatter);
                    delimiter = ",";
                }
            }
            else
            {
                if (outerJoinQualifiers.Count != streams.Count - 1)
                {
                    throw new ArgumentException(
                        "Number of outer join qualifiers must be one less then the number of streams.");
                }

                var first = true;
                for (var i = 0; i < streams.Count; i++)
                {
                    var stream = streams[i];
                    formatter.BeginFromStream(writer, first);
                    first = false;
                    stream.ToEPL(writer, formatter);

                    if (i > 0)
                    {
                        var qualCond = outerJoinQualifiers[i - 1];
                        if (qualCond.Left != null)
                        {
                            writer.Write(" on ");
                            qualCond.Left.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                            writer.Write(" = ");
                            qualCond.Right.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);

                            if (qualCond.AdditionalProperties.Count > 0)
                            {
                                foreach (var pair in qualCond.AdditionalProperties)
                                {
                                    writer.Write(" and ");
                                    pair.Left.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                                    writer.Write(" = ");
                                    pair.Right.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                                }
                            }
                        }
                    }

                    if (i < streams.Count - 1)
                    {
                        var qualType = outerJoinQualifiers[i];
                        writer.Write(" ");
                        if (qualType.Type != OuterJoinType.INNER)
                        {
                            writer.Write(qualType.Type.GetText());
                            writer.Write(" outer");
                        }
                        else
                        {
                            writer.Write(qualType.Type.GetText());
                        }

                        writer.Write(" join");
                    }
                }
            }
        }
    }
} // end of namespace