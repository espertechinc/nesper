///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    /// Context partition identifier for category context.
    /// </summary>
    [Serializable]
    public class ContextPartitionIdentifierCategory : ContextPartitionIdentifier
    {
        /// <summary>Ctor. </summary>
        public ContextPartitionIdentifierCategory()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="label">of category</param>
        public ContextPartitionIdentifierCategory(string label)
        {
            Label = label;
        }

        /// <summary>Returns the category label. </summary>
        /// <value>label</value>
        public string Label { get; set; }

        public override bool CompareTo(ContextPartitionIdentifier other)
        {
            if (!(other is ContextPartitionIdentifierCategory)) {
                return false;
            }

            return Label.Equals(((ContextPartitionIdentifierCategory) other).Label);
        }

        public override string ToString()
        {
            return "ContextPartitionIdentifierCategory{" +
                   "label='" +
                   Label +
                   '\'' +
                   '}';
        }
    }
}