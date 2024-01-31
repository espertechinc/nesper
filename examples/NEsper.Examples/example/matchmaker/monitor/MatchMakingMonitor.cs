///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

using NEsper.Examples.MatchMaker.eventbean;

namespace NEsper.Examples.MatchMaker.monitor
{
    public class MatchMakingMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const double PROXIMITY_RANGE = 1;

        private readonly HashSet<int> _existingUsers = new HashSet<int>();
        private readonly EPRuntime _runtime;

        private EPStatement _locateOther;
        private readonly MatchAlertListener _matchAlertListener;
        private readonly int _mobileUserId;

        public MatchMakingMonitor(EPRuntime runtime, MatchAlertListener matchAlertListener)
        {
            _matchAlertListener = matchAlertListener;
            _runtime = runtime;

            // Get called for any user showing up
            var factory = CompileDeploy("every user=" + typeof (MobileUserBean).FullName);
            factory.Events += HandleFactoryEvents;
        }

        public MatchMakingMonitor(EPRuntime runtime, MobileUserBean mobileUser, MatchAlertListener matchAlertListener)
        {
            _runtime = runtime;
            _matchAlertListener = matchAlertListener;
            _mobileUserId = mobileUser.UserId;

            // Create patterns that listen to other users
            SetupPatterns(mobileUser);

            // Listen to my own location changes so my data is up-to-date
            EPStatement locationChange = CompileDeploy(
                "every myself=" + typeof (MobileUserBean).FullName +
                "(UserId=" + mobileUser.UserId + ")");

            locationChange.Events +=
                delegate(Object sender, UpdateEventArgs e) {
                    // When my location changed, re-establish pattern
                    _locateOther.RemoveAllEventHandlers();
                    var myself = (MobileUserBean) e.NewEvents[0]["myself"];
                    SetupPatterns(myself);
                };
        }

        private void HandleFactoryEvents(Object sender, UpdateEventArgs e)
        {
            var user = (MobileUserBean) e.NewEvents[0]["user"];

            // No action if user already known
            if (_existingUsers.Contains(user.UserId)) {
                return;
            }

            Log.Debug(".update New user encountered, user=" + user.UserId);

            _existingUsers.Add(user.UserId);
            new MatchMakingMonitor(_runtime, user, _matchAlertListener);
        }

        private void SetupPatterns(MobileUserBean mobileUser)
        {
            double locXLow = mobileUser.LocationX - PROXIMITY_RANGE;
            double locXHigh = mobileUser.LocationX + PROXIMITY_RANGE;
            double locYLow = mobileUser.LocationY - PROXIMITY_RANGE;
            double locYHigh = mobileUser.LocationY + PROXIMITY_RANGE;

            _locateOther = CompileDeploy(
                "every other=" + typeof (MobileUserBean).FullName +
                "(locationX in [" + locXLow + ":" + locXHigh + "]," +
                "locationY in [" + locYLow + ":" + locYHigh + "]," +
                "myGender='" + mobileUser.PreferredGender + "'," +
                "myAgeRange='" + mobileUser.PreferredAgeRange + "'," +
                "myHairColor='" + mobileUser.PreferredHairColor + "'," +
                "preferredGender='" + mobileUser.MyGender + "'," +
                "preferredAgeRange='" + mobileUser.MyAgeRange + "'," +
                "preferredHairColor='" + mobileUser.MyHairColor + "'" +
                ")");

            _locateOther.Events +=
                delegate(Object sender, UpdateEventArgs e)
                {
                    var other = (MobileUserBean) e.NewEvents[0]["other"];
                    var alert = new MatchAlertBean(other.UserId, _mobileUserId);
                    _matchAlertListener.Emitted(alert);
                };
        }
        
        private EPStatement CompileDeploy(
            string epl)
        {
            var args = new CompilerArguments();
            args.Path.Add(_runtime.RuntimePath);
            args.Options.AccessModifierNamedWindow = env => NameAccessModifier.PUBLIC;
            args.Configuration.Compiler.ByteCode.IsAllowSubscriber = true;

            var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
            var deployment = _runtime.DeploymentService.Deploy(compiled);
            return deployment.Statements[0];
        }
    }
}
