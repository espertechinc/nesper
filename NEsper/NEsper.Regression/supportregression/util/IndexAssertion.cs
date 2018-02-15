///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.util
{
    public class IndexAssertion
    {
        public IndexAssertion(string hint, string whereClause)
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
            FAFAssertion = fafAssertion;
        }

        public IndexAssertion(string hint, string whereClause, bool unique, IndexAssertionEventSend eventSendAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            IsUnique = unique;
            EventSendAssertion = eventSendAssertion;
        }

        public IndexAssertion(string hint, string whereClause, bool unique, IndexAssertionFAF fafAssertion)
        {
            Hint = hint;
            WhereClause = whereClause;
            IsUnique = unique;
            FAFAssertion = fafAssertion;
        }

        public string Hint { get; private set; }

        public string WhereClause { get; private set; }

        public IndexAssertionEventSend EventSendAssertion { get; private set; }

        public string ExpectedIndexName { get; private set; }

        public string IndexBackingClass { get; private set; }

        public IndexAssertionFAF FAFAssertion { get; private set; }

        public bool IsUnique { get; private set; }

        public Type ExpectedStrategy { get; private set; }
    }
} // end of namespace
