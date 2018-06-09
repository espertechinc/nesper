///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Abstract base class for streams that can be projected via views providing data window, uniqueness or other projections
    /// or deriving further information from streams.
    /// </summary>
    [Serializable]
    public abstract class ProjectedStream : Stream
    {
        private IList<View> _views;

        /// <summary>Ctor.</summary>
        public ProjectedStream()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="views">is a list of views upon the stream</param>
        /// <param name="optStreamName">is the stream as-name, or null if unnamed</param>
        protected ProjectedStream(IList<View> views, string optStreamName)
            : base(optStreamName)
        {
            _views = views;
        }

        /// <summary>
        /// Renders the views onto the projected stream.
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="views">to render</param>
        internal static void ToEPLViews(TextWriter writer, IList<View> views)
        {
            if ((views != null) && (views.Count != 0))
            {
                if (views.First().Namespace == null)
                {
                    writer.Write('#');
                    string delimiter = "";
                    foreach (View view in views)
                    {
                        writer.Write(delimiter);
                        view.ToEPLWithHash(writer);
                        delimiter = "#";
                    }
                }
                else
                {
                    writer.Write('.');
                    string delimiter = "";
                    foreach (View view in views)
                    {
                        writer.Write(delimiter);
                        view.ToEPL(writer);
                        delimiter = ".";
                    }
                }
            }
        }

        /// <summary>
        /// Represent as textual.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public abstract void ToEPLProjectedStream(TextWriter writer, EPStatementFormatter formatter);

        /// <summary>
        /// Represent type as textual non complete.
        /// </summary>
        /// <param name="writer">to output to</param>
        public abstract void ToEPLProjectedStreamType(TextWriter writer);

        /// <summary>
        /// Adds an un-parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(string @namespace, string name)
        {
            _views.Add(View.Create(@namespace, name));
            return this;
        }

        /// <summary>
        /// Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(string @namespace, string name, List<Expression> parameters)
        {
            _views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        /// Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(string @namespace, string name, params Expression[] parameters)
        {
            _views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        /// Adds a parameterized view to the stream.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(string name, params Expression[] parameters)
        {
            _views.Add(View.Create(null, name, parameters));
            return this;
        }

        /// <summary>
        /// Add a view to the stream.
        /// </summary>
        /// <param name="view">to add</param>
        /// <returns>stream</returns>
        public ProjectedStream AddView(View view)
        {
            _views.Add(view);
            return this;
        }

        /// <summary>
        /// Returns the list of views added to the stream.
        /// </summary>
        /// <value>list of views</value>
        public IList<View> Views
        {
            get { return _views; }
            set { this._views = value; }
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public override void ToEPLStream(TextWriter writer, EPStatementFormatter formatter)
        {
            ToEPLProjectedStream(writer, formatter);
            ToEPLViews(writer, _views);
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            ToEPLProjectedStreamType(writer);

            if ((_views != null) && (_views.Count != 0))
            {
                writer.Write('.');
                string delimiter = "";
                foreach (View view in _views)
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

        /// <summary>
        /// Returns true if the stream as unidirectional, for use in unidirectional joins.
        /// </summary>
        /// <value>true for unidirectional stream, applicable only for joins</value>
        public bool IsUnidirectional { get; set; }

        /// <summary>
        /// Set to unidirectional.
        /// </summary>
        /// <param name="isUnidirectional">try if unidirectional</param>
        /// <returns>stream</returns>
        public ProjectedStream Unidirectional(bool isUnidirectional)
        {
            this.IsUnidirectional = isUnidirectional;
            return this;
        }

        /// <summary>
        /// Returns true if multiple data window shall be treated as a union.
        /// </summary>
        /// <value>retain union</value>
        public bool IsRetainUnion { get; set; }

        /// <summary>
        /// Returns true if multiple data window shall be treated as an intersection.
        /// </summary>
        /// <value>retain intersection</value>
        public bool IsRetainIntersection { get; set; }

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
} // end of namespace
