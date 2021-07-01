using System;

namespace com.espertech.esper.regressionlib.support.util
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class IndexAssertion
    {
        public IndexAssertion(
            string hint,
            string whereClause)
        {
            Hint = hint;
            WhereClause = whereClause;
        }

        public IndexAssertion(
            string whereClause,
            string expectedIndexName,
            Type expectedStrategy,
            IndexAssertionEventSend eventSendAssertion)
        {
            WhereClause = whereClause;
            ExpectedIndexName = expectedIndexName;
            EventSendAssertion = eventSendAssertion;
            ExpectedStrategy = expectedStrategy;
        }

        public IndexAssertion(
            string hint,
            string whereClause,
            string expectedIndexName,
            string indexBackingClass,
            IndexAssertionEventSend eventSendAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            ExpectedIndexName = expectedIndexName;
            IndexBackingClass = indexBackingClass;
            EventSendAssertion = eventSendAssertion;
        }

        public IndexAssertion(
            string hint,
            string whereClause,
            string expectedIndexName,
            string indexBackingClass,
            IndexAssertionFAF fafAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            ExpectedIndexName = expectedIndexName;
            IndexBackingClass = indexBackingClass;
            FafAssertion = fafAssertion;
        }

        public IndexAssertion(
            string hint,
            string whereClause,
            bool unique,
            IndexAssertionEventSend eventSendAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            Unique = unique;
            EventSendAssertion = eventSendAssertion;
        }

        public IndexAssertion(
            string hint,
            string whereClause,
            bool unique,
            IndexAssertionFAF fafAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            Unique = unique;
            FafAssertion = fafAssertion;
        }

        public string Hint { get; }

        public string WhereClause { get; }

        public IndexAssertionEventSend EventSendAssertion { get; }

        public string ExpectedIndexName { get; }

        public string IndexBackingClass { get; }

        public IndexAssertionFAF FafAssertion { get; }

        public bool Unique { get; }

        public Type ExpectedStrategy { get; }
    }
} // end of namespace