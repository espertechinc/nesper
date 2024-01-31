///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Abstract base class for streams that can be projected via views providing data window, uniqueness or other
    ///     projections
    ///     or deriving further information from streams.
    /// </summary>
    [Serializable]
    public abstract class ProjectedStream : Stream
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ProjectedStream()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="views">is a list of views upon the stream</param>
        /// <param name="optStreamName">is the stream as-name, or null if unnamed</param>
        protected ProjectedStream(
            IList<View> views,
            string optStreamName)
            : base(optStreamName)
        {
            Views = views;
        }

        /// <summary>
        ///     Returns the list of views added to the stream.
        /// </summary>
        /// <returns>list of views</returns>
        public IList<View> Views { get; set; }

        /// <summary>
        ///     Returns true if the stream as unidirectional, for use in unidirectional joins.
        /// </summary>
        /// <returns>true for unidirectional stream, applicable only for joins</returns>
        public bool IsUnidirectional { get; set; }

        /// <summary>
        ///     Returns true if multiple data window shall be treated as a union.
        /// </summary>
        /// <returns>retain union</returns>
        public bool IsRetainUnion { get; set; }

        /// <summary>
        ///     Returns true if multiple data window shall be treated as an intersection.
        /// </summary>
        /// <returns>retain intersection</returns>
        public bool IsRetainIntersection { get; set; }

        /// <summary>
        ///     Represent as textual.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public abstract void ToEPLProjectedStream(
            TextWriter writer,
            EPStatementFormatter formatter);

        /// <summary>
        ///     Represent type as textual non complete.
        /// </summary>
        /// <param name="writer">to output to</param>
        public abstract void ToEPLProjectedStreamType(TextWriter writer);

        /// <summary>
        ///     Adds an un-parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(
            string @namespace,
            string name)
        {
            Views.Add(View.Create(@namespace, name));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(
            string @namespace,
            string name,
            IList<Expression> parameters)
        {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(
            string @namespace,
            string name,
            params Expression[] parameters)
        {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(
            string name,
            params Expression[] parameters)
        {
            Views.Add(View.Create(null, name, parameters));
            return this;
        }

        /// <summary>
        ///     Add a view to the stream.
        /// </summary>
        /// <param name="view">to add</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(View view)
        {
            Views.Add(view);
            return this;
        }

        /// <summary>
        ///     Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter"></param>
        public override void ToEPLStream(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            ToEPLProjectedStream(writer, formatter);
            ToEPLViews(writer, Views);
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            ToEPLProjectedStreamType(writer);

            if (Views != null && Views.Count != 0) {
                writer.Write('.');
                var delimiter = "";
                foreach (var view in Views) {
                    writer.Write(delimiter);
                    writer.Write(view.Namespace);
                    writer.Write(".");
                    writer.Write(view.Name);
                    writer.Write("()");
                    delimiter = ".";
                }
            }
        }

        /// <summary>
        ///     Set to unidirectional.
        /// </summary>
        /// <param name="isUnidirectional">try if unidirectional</param>
        /// <returns>stream</returns>
        public ProjectedStream Unidirectional(bool isUnidirectional)
        {
            IsUnidirectional = isUnidirectional;
            return this;
        }

        /// <summary>
        ///     Renders the views onto the projected stream.
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="views">to render</param>
        protected internal static void ToEPLViews(
            TextWriter writer,
            IList<View> views)
        {
            if (views != null && views.Count != 0) {
                if (views.First().Namespace == null) {
                    writer.Write('#');
                    var delimiter = "";
                    foreach (var view in views) {
                        writer.Write(delimiter);
                        view.ToEPLWithHash(writer);
                        delimiter = "#";
                    }
                }
                else {
                    writer.Write('.');
                    var delimiter = "";
                    foreach (var view in views) {
                        writer.Write(delimiter);
                        view.ToEPL(writer);
                        delimiter = ".";
                    }
                }
            }
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {
            if (IsUnidirectional) {
                writer.Write(" unidirectional");
            }
            else if (IsRetainUnion) {
                writer.Write(" retain-union");
            }
            else if (IsRetainIntersection) {
                writer.Write(" retain-intersection");
            }
        }
    }
} // end of namespace