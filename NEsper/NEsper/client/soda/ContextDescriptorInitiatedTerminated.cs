///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Context dimension descriptor for a start-and-end temporal (single instance) or initiated-terminated (overlapping) context
    /// </summary>
    public class ContextDescriptorInitiatedTerminated : ContextDescriptor
    {
        /// <summary>Ctor.</summary>
        public ContextDescriptorInitiatedTerminated()
        {
        }

        /// <summary>
        /// Ctor.
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
            StartCondition = startCondition;
            EndCondition = endCondition;
            IsOverlapping = overlapping;
            OptionalDistinctExpressions = optionalDistinctExpressions;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="startCondition">the condition that starts/initiates a context partition</param>
        /// <param name="endCondition">the condition that ends/terminates a context partition</param>
        /// <param name="overlapping">true for overlapping contexts</param>
        public ContextDescriptorInitiatedTerminated(
            ContextDescriptorCondition startCondition,
            ContextDescriptorCondition endCondition,
            bool overlapping)
        {
            StartCondition = startCondition;
            EndCondition = endCondition;
            IsOverlapping = overlapping;
        }

        /// <summary>
        /// Returns the condition that starts/initiates a context partition
        /// </summary>
        /// <value>start condition</value>
        public ContextDescriptorCondition StartCondition { get; set; }

        /// <summary>
        /// Returns the condition that ends/terminates a context partition
        /// </summary>
        /// <value>end condition</value>
        public ContextDescriptorCondition EndCondition { get; set; }

        /// <summary>
        /// Returns true for overlapping context, false for non-overlapping.
        /// </summary>
        /// <value>overlap indicator</value>
        public bool IsOverlapping { get; set; }

        /// <summary>
        /// Returns the list of expressions providing distinct keys, if any
        /// </summary>
        /// <value>distinct expressions</value>
        public IList<Expression> OptionalDistinctExpressions { get; set; }

        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write(IsOverlapping ? "initiated by " : "start ");
            if (OptionalDistinctExpressions != null && OptionalDistinctExpressions.Count > 0)
            {
                writer.Write("Distinct(");
                string delimiter = "";
                foreach (Expression expression in OptionalDistinctExpressions)
                {
                    writer.Write(delimiter);
                    expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    delimiter = ", ";
                }
                writer.Write(") ");
            }
            StartCondition.ToEPL(writer, formatter);
            if (!(EndCondition is ContextDescriptorConditionNever))
            {
                writer.Write(" ");
                writer.Write(IsOverlapping ? "terminated " : "end ");
                EndCondition.ToEPL(writer, formatter);
            }
        }
    }
} // end of namespace
