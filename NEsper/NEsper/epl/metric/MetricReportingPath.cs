///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.metric
{
    /// <summary>
    /// Global bool for enabling and disable metrics reporting.
    /// </summary>
    public class MetricReportingPath
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static bool IsMetricsEnabledValue;

        /// <summary>Public access. </summary>
        public static bool IsMetricsEnabled
        {
            get { return IsMetricsEnabledValue; }
            set
            {
                if (value)
                {
                    Log.Info("Metrics reporting has been enabled, this setting takes affect for all engine instances at engine initialization time.");
                }
                else
                {
                    Log.Debug("Metrics reporting has been disabled, this setting takes affect for all engine instances at engine initialization time.");
                }
                IsMetricsEnabledValue = value;
            }
        }
    }
}
