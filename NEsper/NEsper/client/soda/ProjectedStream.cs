///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Abstract base class for streams that can be projected via views providing data window, uniqueness
    /// or other projections or deriving further information from streams.
    /// </summary>
    [Serializable]
    public abstract class ProjectedStream : Stream
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        protected ProjectedStream()
        {
        }

        /// <summary>
        /// Represent as textual.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public abstract void ToEPLProjectedStream(TextWriter writer, EPStatementFormatter formatter);

        /// <summary>
        /// Represent type as textual non complete.
        /// </summary>
        /// <param name="writer">to output to</param>
        public abstract void ToEPLProjectedStreamType(TextWriter writer);

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="views">is a list of views upon the stream</param>
        /// <param name="optStreamName">is the stream as-name, or null if unnamed</param>
        protected ProjectedStream(IList<View> views, String optStreamName)
            : base(optStreamName)
        {
            Views = views;
        }

        /// <summary>Adds an un-parameterized view to the stream. </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(String @namespace, String name)
        {
            Views.Add(View.Create(@namespace, name));
            return this;
        }

        /// <summary>Adds a parameterized view to the stream. </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(String @namespace, String name, List<Expression> parameters)
        {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>Adds a parameterized view to the stream. </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(String @namespace, String name, params Expression[] parameters)
        {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>Add a view to the stream. </summary>
        /// <param name="view">to add</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(View view)
        {
            Views.Add(view);
            return this;
        }

        /// <summary>Returns the list of views added to the stream. </summary>
        /// <value>list of views</value>
        public IList<View> Views { get; set; }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public override void ToEPLStream(TextWriter writer, EPStatementFormatter formatter)
        {
            ToEPLProjectedStream(writer, formatter);
            ToEPLViews(writer, Views);
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            ToEPLProjectedStreamType(writer);

            if ((Views != null) && (Views.Count != 0))
            {
                writer.Write('.');
                String delimiter = "";
                foreach (View view in Views)
                {
                    writer.Write(delimiter);
                    writer.Write(view.Namespace);
                    writer.Write(".");
                    writer.Write(view.Name);
                    writer.Write("()");
                    delimiter = ".";
                }
            }
        }

        /// <summary>Returns true if the stream as unidirectional, for use in unidirectional joins. </summary>
        /// <value>true for unidirectional stream, applicable only for joins</value>
        public bool IsUnidirectional { get; set; }

        /// <summary>Set to unidirectional. </summary>
        /// <param name="isUnidirectional">try if unidirectional</param>
        /// <returns>stream</returns>
        public ProjectedStream Unidirectional(bool isUnidirectional)
        {
            IsUnidirectional = isUnidirectional;
            return this;
        }

        /// <summary>Returns true if multiple data window shall be treated as a union. </summary>
        /// <value>retain union</value>
        public bool IsRetainUnion { get; set; }

        /// <summary>Returns true if multiple data window shall be treated as an intersection. </summary>
        /// <value>retain intersection</value>
        public bool IsRetainIntersection { get; set; }

        /// <summary>Renders the views onto the projected stream. </summary>
        /// <param name="writer">to render to</param>
        /// <param name="views">to render</param>
        protected internal static void ToEPLViews(TextWriter writer, IList<View> views)
        {
            if ((views != null) && (views.Count != 0))
            {
                writer.Write('.');
                String delimiter = "";
                foreach (View view in views)
                {
                    writer.Write(delimiter);
                    view.ToEPL(writer);
                    delimiter = ".";
                }
            }
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {
            if (IsUnidirectional)
            {
                writer.Write(" unidirectional");
            }
            else if (IsRetainUnion)
            {
                writer.Write(" retain-union");
            }
            else if (IsRetainIntersection)
            {
                writer.Write(" retain-intersection");
            }
        }
    }
}