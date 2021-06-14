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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Context dimension descriptor for a start-and-end temporal (single instance) or initiated-terminated (overlapping)
    ///     context
    /// </summary>
    [Serializable]
    public class ContextDescriptorInitiatedTerminated : ContextDescriptor
    {
        private ContextDescriptorCondition endCondition;
        private IList<Expression> optionalDistinctExpressions;
        private bool overlapping;
        private ContextDescriptorCondition startCondition;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ContextDescriptorInitiatedTerminated()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="startCondition">the condition that starts/initiates a context partition</param>
        /// <param name="endCondition">the condition that ends/terminates a context partition</param>
        /// <param name="overlapping">true for overlapping contexts</param>
        /// <param name="optionalDistinctExpressions">list of distinct-value expressions, can be null</param>
        public ContextDescriptorInitiatedTerminated(
            ContextDescriptorCondition startCondition,
            ContextDescriptorCondition endCondition,
            bool overlapping,
            IList<Expression> optionalDistinctExpressions)
        {
            this.startCondition = startCondition;
            this.endCondition = endCondition;
            this.overlapping = overlapping;
            this.optionalDistinctExpressions = optionalDistinctExpressions;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="startCondition">the condition that starts/initiates a context partition</param>
        /// <param name="endCondition">the condition that ends/terminates a context partition</param>
        /// <param name="overlapping">true for overlapping contexts</param>
        public ContextDescriptorInitiatedTerminated(
            ContextDescriptorCondition startCondition,
            ContextDescriptorCondition endCondition,
            bool overlapping)
        {
            this.startCondition = startCondition;
            this.endCondition = endCondition;
            this.overlapping = overlapping;
        }

        /// <summary>
        ///     Returns the condition that starts/initiates a context partition
        /// </summary>
        /// <returns>start condition</returns>
        public ContextDescriptorCondition InitCondition
        {
            get => startCondition;
            set => startCondition = value;
        }

        /// <summary>
        ///     Returns the condition that ends/terminates a context partition
        /// </summary>
        /// <returns>end condition</returns>
        public ContextDescriptorCondition TermCondition
        {
            get => endCondition;
            set => endCondition = value;
        }

        public ContextDescriptorCondition StartCondition
        {
            get => startCondition;
            set => startCondition = value;
        }

        public ContextDescriptorCondition EndCondition
        {
            get => endCondition;
            set => endCondition = value;
        }

        /// <summary>
        ///     Returns true for overlapping context, false for non-overlapping.
        /// </summary>
        /// <returns>overlap indicator</returns>
        public bool IsOverlapping
        {
            get => overlapping;
            set => overlapping = value;
        }

        /// <summary>
        ///     Returns the list of expressions providing distinct keys, if any
        /// </summary>
        /// <returns>distinct expressions</returns>
        public IList<Expression> OptionalDistinctExpressions
        {
            get => optionalDistinctExpressions;
            set => optionalDistinctExpressions = value;
        }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write(overlapping ? "initiated by " : "start ");
            if (optionalDistinctExpressions != null && optionalDistinctExpressions.Count > 0)
            {
                writer.Write("distinct(");
                var delimiter = "";
                foreach (var expression in optionalDistinctExpressions)
                {
                    writer.Write(delimiter);
                    expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }

                writer.Write(") ");
            }

            startCondition.ToEPL(writer, formatter);
            if (!(endCondition is ContextDescriptorConditionNever))
            {
                writer.Write(" ");
                writer.Write(overlapping ? "terminated " : "end ");
                endCondition.ToEPL(writer, formatter);
            }
        }
    }
} // end of namespace