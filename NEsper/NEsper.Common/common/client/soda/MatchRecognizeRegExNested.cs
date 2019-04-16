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
    ///     Atom representing an expression for use in match-recognize.
    ///     <para />
    ///     Event row regular expressions are organized into a tree-like structure with nodes representing sub-expressions.
    /// </summary>
    [Serializable]
    public class MatchRecognizeRegExNested : MatchRecognizeRegEx
    {
        private MatchRecognizeRegExRepeat optionalRepeat;
        private MatchRecogizePatternElementType type;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public MatchRecognizeRegExNested()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">multiplicity</param>
        public MatchRecognizeRegExNested(MatchRecogizePatternElementType type)
        {
            this.type = type;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">multiplicity</param>
        /// <param name="optionalRepeat">repetition</param>
        public MatchRecognizeRegExNested(
            MatchRecogizePatternElementType type,
            MatchRecognizeRegExRepeat optionalRepeat)
        {
            this.type = type;
            this.optionalRepeat = optionalRepeat;
        }

        /// <summary>
        ///     Returns multiplicity.
        /// </summary>
        /// <returns>multiplicity</returns>
        public MatchRecogizePatternElementType Type {
            get => type;
            set => type = value;
        }

        /// <summary>
        ///     Returns the repetition
        /// </summary>
        /// <returns>repetition</returns>
        public MatchRecognizeRegExRepeat OptionalRepeat {
            get => optionalRepeat;
            set => optionalRepeat = value;
        }

        public override void WriteEPL(TextWriter writer)
        {
            writer.Write("(");
            var delimiter = "";
            foreach (var node in Children) {
                writer.Write(delimiter);
                node.WriteEPL(writer);
                delimiter = " ";
            }

            writer.Write(")");
            writer.Write(type.GetText());
            if (optionalRepeat != null) {
                optionalRepeat.WriteEPL(writer);
            }
        }
    }
} // end of namespace