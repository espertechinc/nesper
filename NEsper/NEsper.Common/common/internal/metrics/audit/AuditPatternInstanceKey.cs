///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public class AuditPatternInstanceKey
    {
        private readonly int agentInstanceId;
        private readonly string runtimeURI;
        private readonly int statementId;
        private readonly string text;

        public AuditPatternInstanceKey(
            string runtimeURI,
            int statementId,
            int agentInstanceId,
            string text)
        {
            this.runtimeURI = runtimeURI;
            this.statementId = statementId;
            this.agentInstanceId = agentInstanceId;
            this.text = text;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (AuditPatternInstanceKey) o;

            if (statementId != that.statementId) {
                return false;
            }

            if (agentInstanceId != that.agentInstanceId) {
                return false;
            }

            if (!runtimeURI.Equals(that.runtimeURI)) {
                return false;
            }

            return text.Equals(that.text);
        }

        public override int GetHashCode()
        {
            var result = runtimeURI.GetHashCode();
            result = 31 * result + statementId;
            result = 31 * result + agentInstanceId;
            result = 31 * result + text.GetHashCode();
            return result;
        }
    }
} // end of namespace
