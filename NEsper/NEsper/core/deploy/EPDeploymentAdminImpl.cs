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
using System.Net;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

using Module = com.espertech.esper.client.deploy.Module;

namespace com.espertech.esper.core.deploy
{
    /// <summary>Deployment administrative implementation.</summary>
    public class EPDeploymentAdminImpl : EPDeploymentAdminSPI
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPAdministratorSPI _epService;
        private readonly IReaderWriterLock _eventProcessingRwLock;
        private readonly IResourceManager _resourceManager;
        private readonly DeploymentStateService _deploymentStateService;
        private readonly StatementEventTypeRef _statementEventTypeRef;
        private readonly EventAdapterService _eventAdapterService;
        private readonly StatementIsolationService _statementIsolationService;
        private readonly FilterService _filterService;
        private readonly TimeZoneInfo _timeZone;
        private readonly ConfigurationEngineDefaults.UndeployRethrowPolicy _undeployRethrowPolicy;
        private readonly ILockable _iLock;

        public EPDeploymentAdminImpl(
            EPAdministratorSPI epService,
            ILockManager lockManager,
            IReaderWriterLock eventProcessingRWLock,
            IResourceManager resourceManager,
            DeploymentStateService deploymentStateService,
            StatementEventTypeRef statementEventTypeRef,
            EventAdapterService eventAdapterService,
            StatementIsolationService statementIsolationService,
            FilterService filterService,
            TimeZoneInfo timeZone,
            ConfigurationEngineDefaults.UndeployRethrowPolicy undeployRethrowPolicy)
        {
            _iLock = lockManager.CreateDefaultLock();
            _epService = epService;
            _resourceManager = resourceManager;
            _eventProcessingRwLock = eventProcessingRWLock;
            _deploymentStateService = deploymentStateService;
            _statementEventTypeRef = statementEventTypeRef;
            _eventAdapterService = eventAdapterService;
            _statementIsolationService = statementIsolationService;
            _filterService = filterService;
            _timeZone = timeZone;
            _undeployRethrowPolicy = undeployRethrowPolicy;
        }

        public Module Read(Stream stream, string uri)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Reading module from input stream");
            }
            return EPLModuleUtil.ReadInternal(stream, uri);
        }

        public Module Read(FileInfo file)
        {
            var absolutePath = Path.GetFullPath(file.Name);

            Log.Debug("Reading resource '{0}'", absolutePath);

            using (var stream = File.OpenRead(absolutePath))
            {
                return EPLModuleUtil.ReadInternal(stream, absolutePath);
            }
        }

        public Module Read(Uri url)
        {
            Log.Debug("Reading resource from url: {0}", url);

            using (var webClient = new WebClient())
            {
                using (var stream = webClient.OpenRead(url))
                {
                    return EPLModuleUtil.ReadInternal(stream, url.ToString());
                }
            }
        }

        public Module Read(string resource)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Reading resource '" + resource + "'");
            }
            return EPLModuleUtil.ReadResource(
                resource, 
                _eventAdapterService.EngineImportService,
                _resourceManager);
        }

        public DeploymentResult Deploy(Module module, DeploymentOptions options, string assignedDeploymentId)
        {
            using (_iLock.Acquire())
            {
                if (_deploymentStateService.GetDeployment(assignedDeploymentId) != null)
                {
                    throw new ArgumentException(
                        "Assigned deployment id '" + assignedDeploymentId + "' is already in use");
                }
                return DeployInternal(module, options, assignedDeploymentId, DateTimeEx.GetInstance(_timeZone));
            }
        }

        public DeploymentResult Deploy(Module module, DeploymentOptions options)
        {
            using (_iLock.Acquire())
            {
                string deploymentId = _deploymentStateService.NextDeploymentId;
                return DeployInternal(module, options, deploymentId, DateTimeEx.GetInstance(_timeZone));
            }
        }

        private DeploymentResult DeployInternal(
            Module module,
            DeploymentOptions options,
            string deploymentId,
            DateTimeEx addedDate)
        {
            if (options == null)
            {
                options = new DeploymentOptions();
            }

            options.DeploymentLockStrategy.Acquire(_eventProcessingRwLock);
            try
            {
                return DeployInternalLockTaken(module, options, deploymentId, addedDate);
            }
            finally
            {
                options.DeploymentLockStrategy.Release(_eventProcessingRwLock);
            }
        }

        private DeploymentResult DeployInternalLockTaken(
            Module module,
            DeploymentOptions options,
            string deploymentId,
            DateTimeEx addedDate)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Deploying module " + module);
            }
            IList<string> imports;
            if (module.Imports != null)
            {
                foreach (string imported in module.Imports)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Adding import " + imported);
                    }
                    _epService.Configuration.AddImport(imported);
                }
                imports = new List<string>(module.Imports);
            }
            else
            {
                imports = Collections.GetEmptyList<string>();
            }

            if (options.IsCompile)
            {
                var exceptionsX = new List<DeploymentItemException>();
                foreach (ModuleItem item in module.Items)
                {
                    if (item.IsCommentOnly)
                    {
                        continue;
                    }

                    try
                    {
                        _epService.CompileEPL(item.Expression);
                    }
                    catch (Exception ex)
                    {
                        exceptionsX.Add(new DeploymentItemException(ex.Message, item.Expression, ex, item.LineNumber));
                    }
                }

                if (!exceptionsX.IsEmpty())
                {
                    throw BuildException("Compilation failed", module, exceptionsX);
                }
            }

            if (options.IsCompileOnly)
            {
                return null;
            }

            var exceptions = new List<DeploymentItemException>();
            var statementNames = new List<DeploymentInformationItem>();
            var statements = new List<EPStatement>();
            var eventTypesReferenced = new HashSet<string>();

            foreach (ModuleItem item in module.Items)
            {
                if (item.IsCommentOnly)
                {
                    continue;
                }

                string statementName = null;
                Object userObject = null;
                if (options.StatementNameResolver != null || options.StatementUserObjectResolver != null)
                {
                    var ctx = new StatementDeploymentContext(item.Expression, module, item, deploymentId);
                    statementName = options.StatementNameResolver != null
                        ? options.StatementNameResolver.GetStatementName(ctx)
                        : null;
                    userObject = options.StatementUserObjectResolver != null
                        ? options.StatementUserObjectResolver.GetUserObject(ctx)
                        : null;
                }

                try
                {
                    EPStatement stmt;
                    if (options.IsolatedServiceProvider == null)
                    {
                        stmt = _epService.CreateEPL(item.Expression, statementName, userObject);
                    }
                    else
                    {
                        EPServiceProviderIsolated unit =
                            _statementIsolationService.GetIsolationUnit(options.IsolatedServiceProvider, -1);
                        stmt = unit.EPAdministrator.CreateEPL(item.Expression, statementName, userObject);
                    }
                    statementNames.Add(new DeploymentInformationItem(stmt.Name, stmt.Text));
                    statements.Add(stmt);

                    string[] types = _statementEventTypeRef.GetTypesForStatementName(stmt.Name);
                    if (types != null)
                    {
                        eventTypesReferenced.AddAll(types);
                    }
                }
                catch (EPException ex)
                {
                    exceptions.Add(new DeploymentItemException(ex.Message, item.Expression, ex, item.LineNumber));
                    if (options.IsFailFast)
                    {
                        break;
                    }
                }
            }

            if (!exceptions.IsEmpty())
            {
                if (options.IsRollbackOnFail)
                {
                    Log.Debug("Rolling back intermediate statements for deployment");
                    foreach (EPStatement stmt in statements)
                    {
                        try
                        {
                            stmt.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Failed to destroy created statement during rollback: " + ex.Message, ex);
                        }
                    }
                    EPLModuleUtil.UndeployTypes(
                        eventTypesReferenced, _statementEventTypeRef, _eventAdapterService, _filterService);
                }
                string text = "Deployment failed";
                if (options.IsValidateOnly)
                {
                    text = "Validation failed";
                }
                throw BuildException(text, module, exceptions);
            }

            if (options.IsValidateOnly)
            {
                Log.Debug("Rolling back created statements for validate-only");
                foreach (EPStatement stmt in statements)
                {
                    try
                    {
                        stmt.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("Failed to destroy created statement during rollback: " + ex.Message, ex);
                    }
                }
                EPLModuleUtil.UndeployTypes(
                    eventTypesReferenced, _statementEventTypeRef, _eventAdapterService, _filterService);
                return null;
            }

            DeploymentInformationItem[] deploymentInfoArr = statementNames.ToArray();
            var desc = new DeploymentInformation(
                deploymentId, module, addedDate, DateTimeEx.GetInstance(_timeZone), deploymentInfoArr,
                DeploymentState.DEPLOYED);
            _deploymentStateService.AddUpdateDeployment(desc);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Module " + module + " was successfully deployed.");
            }
            return new DeploymentResult(desc.DeploymentId, statements.AsReadOnlyList(), imports);
        }

        private DeploymentActionException BuildException(
            string msg,
            Module module,
            List<DeploymentItemException> exceptions)
        {
            string message = msg;
            if (module.Name != null)
            {
                message += " in module '" + module.Name + "'";
            }
            if (module.Uri != null)
            {
                message += " in module url '" + module.Uri + "'";
            }
            if (exceptions.Count > 0)
            {
                message += " in expression '" + GetAbbreviated(exceptions[0].Expression) + "' : " +
                           exceptions[0].Message;
            }
            return new DeploymentActionException(message, exceptions);
        }

        private string GetAbbreviated(string expression)
        {
            if (expression.Length < 60)
            {
                return ReplaceNewline(expression);
            }
            string subtext = expression.Substring(0, 50) + "...(" + expression.Length + " chars)";
            return ReplaceNewline(subtext);
        }

        private string ReplaceNewline(string text)
        {
            text = text.RegexReplaceAll("\\n", " ");
            text = text.RegexReplaceAll("\\t", " ");
            text = text.RegexReplaceAll("\\r", " ");
            return text;
        }

        public Module Parse(string eplModuleText)
        {
            return EPLModuleUtil.ParseInternal(eplModuleText, null);
        }

        public UndeploymentResult UndeployRemove(string deploymentId)
        {
            using (_iLock.Acquire())
            {
                return UndeployRemoveInternal(deploymentId, new UndeploymentOptions());
            }
        }

        public UndeploymentResult UndeployRemove(string deploymentId, UndeploymentOptions undeploymentOptions)
        {
            using (_iLock.Acquire())
            {
                return UndeployRemoveInternal(
                    deploymentId, undeploymentOptions ?? new UndeploymentOptions());
            }
        }

        public UndeploymentResult Undeploy(string deploymentId)
        {
            using (_iLock.Acquire())
            {
                return UndeployInternal(deploymentId, new UndeploymentOptions());
            }
        }

        public UndeploymentResult Undeploy(string deploymentId, UndeploymentOptions undeploymentOptions)
        {
            using (_iLock.Acquire())
            {
                return UndeployInternal(
                    deploymentId, undeploymentOptions ?? new UndeploymentOptions());
            }
        }

        public string[] Deployments
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _deploymentStateService.Deployments;
                }
            }
        }

        public DeploymentInformation GetDeployment(string deploymentId)
        {
            using (_iLock.Acquire())
            {
                return _deploymentStateService.GetDeployment(deploymentId);
            }
        }

        public DeploymentInformation[] DeploymentInformation
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _deploymentStateService.AllDeployments;
                }
            }
        }

        public DeploymentOrder GetDeploymentOrder(ICollection<Module> modules, DeploymentOrderOptions options)
        {
            using (_iLock.Acquire())
            {
                if (options == null)
                {
                    options = new DeploymentOrderOptions();
                }
                string[] deployments = _deploymentStateService.Deployments;

                var proposedModules = new List<Module>();
                proposedModules.AddAll(modules);

                var availableModuleNames = new HashSet<string>();
                foreach (Module proposedModule in proposedModules)
                {
                    if (proposedModule.Name != null)
                    {
                        availableModuleNames.Add(proposedModule.Name);
                    }
                }

                // Collect all uses-dependencies of existing modules
                var usesPerModuleName = new Dictionary<string, ISet<string>>();
                foreach (string deployment in deployments)
                {
                    DeploymentInformation info = _deploymentStateService.GetDeployment(deployment);
                    if (info == null)
                    {
                        continue;
                    }
                    if ((info.Module.Name == null) || (info.Module.Uses == null))
                    {
                        continue;
                    }
                    ISet<string> usesSet = usesPerModuleName.Get(info.Module.Name);
                    if (usesSet == null)
                    {
                        usesSet = new HashSet<string>();
                        usesPerModuleName.Put(info.Module.Name, usesSet);
                    }
                    usesSet.AddAll(info.Module.Uses);
                }

                // Collect uses-dependencies of proposed modules
                foreach (Module proposedModule in proposedModules)
                {

                    // check uses-dependency is available
                    if (options.IsCheckUses)
                    {
                        if (proposedModule.Uses != null)
                        {
                            foreach (string uses in proposedModule.Uses)
                            {
                                if (availableModuleNames.Contains(uses))
                                {
                                    continue;
                                }
                                if (IsDeployed(uses))
                                {
                                    continue;
                                }
                                string message = "Module-dependency not found";
                                if (proposedModule.Name != null)
                                {
                                    message += " as declared by module '" + proposedModule.Name + "'";
                                }
                                message += " for uses-declaration '" + uses + "'";
                                throw new DeploymentOrderException(message);
                            }
                        }
                    }

                    if ((proposedModule.Name == null) || (proposedModule.Uses == null))
                    {
                        continue;
                    }
                    ISet<string> usesSet = usesPerModuleName.Get(proposedModule.Name);
                    if (usesSet == null)
                    {
                        usesSet = new HashSet<string>();
                        usesPerModuleName.Put(proposedModule.Name, usesSet);
                    }
                    usesSet.AddAll(proposedModule.Uses);
                }

                var proposedModuleNames = new Dictionary<string, ISet<int>>().WithNullSupport();
                int count = 0;
                foreach (Module proposedModule in proposedModules)
                {
                    ISet<int> moduleNumbers = proposedModuleNames.Get(proposedModule.Name);
                    if (moduleNumbers == null)
                    {
                        moduleNumbers = new SortedSet<int>();
                        proposedModuleNames.Put(proposedModule.Name, moduleNumbers);
                    }
                    moduleNumbers.Add(count);
                    count++;
                }

                var graph = new DependencyGraph(proposedModules.Count, false);
                int fromModule = 0;
                foreach (Module proposedModule in proposedModules)
                {
                    if ((proposedModule.Uses == null) || (proposedModule.Uses.IsEmpty()))
                    {
                        fromModule++;
                        continue;
                    }
                    var dependentModuleNumbers = new SortedSet<int>();
                    foreach (string use in proposedModule.Uses)
                    {
                        ISet<int> moduleNumbers = proposedModuleNames.Get(use);
                        if (moduleNumbers == null)
                        {
                            continue;
                        }
                        dependentModuleNumbers.AddAll(moduleNumbers);
                    }
                    dependentModuleNumbers.Remove(fromModule);
                    graph.AddDependency(fromModule, dependentModuleNumbers);
                    fromModule++;
                }

                if (options.IsCheckCircularDependency)
                {
                    var circular = graph.FirstCircularDependency;
                    if (circular != null)
                    {
                        string message = "";
                        string delimiter = "";
                        foreach (int i in circular)
                        {
                            message += delimiter;
                            message += "module '" + proposedModules[i].Name + "'";
                            delimiter = " uses (depends on) ";
                        }
                        throw new DeploymentOrderException(
                            "Circular dependency detected in module uses-relationships: " + message);
                    }
                }

                var reverseDeployList = new List<Module>();
                var ignoreList = new HashSet<int>();
                while (ignoreList.Count < proposedModules.Count)
                {
                    // secondary sort according to the order of listing
                    ICollection<int> rootNodes = new SortedSet<int>(
                        new StandardComparer<int>((o1, o2) => -1*o1.CompareTo(o2)));

                    rootNodes.AddAll(graph.GetRootNodes(ignoreList));

                    if (rootNodes.IsEmpty())
                    {
                        // circular dependency could cause this
                        for (int i = 0; i < proposedModules.Count; i++)
                        {
                            if (!ignoreList.Contains(i))
                            {
                                rootNodes.Add(i);
                                break;
                            }
                        }
                    }

                    foreach (int root in rootNodes)
                    {
                        ignoreList.Add(root);
                        reverseDeployList.Add(proposedModules[root]);
                    }
                }

                reverseDeployList.Reverse();
                return new DeploymentOrder(reverseDeployList);
            }
        }

        public bool IsDeployed(string moduleName)
        {
            using (_iLock.Acquire())
            {
                DeploymentInformation[] infos = _deploymentStateService.AllDeployments;
                if (infos == null)
                {
                    return false;
                }
                foreach (DeploymentInformation info in infos)
                {
                    if ((info.Module.Name != null) && (info.Module.Name.Equals(moduleName)))
                    {
                        return info.State == DeploymentState.DEPLOYED;
                    }
                }
                return false;
            }
        }

        public DeploymentResult ReadDeploy(Stream stream, string moduleURI, string moduleArchive, Object userObject)
        {
            using (_iLock.Acquire())
            {
                Module module = EPLModuleUtil.ReadInternal(stream, moduleURI);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }

        public DeploymentResult ReadDeploy(string resource, string moduleURI, string moduleArchive, Object userObject)
        {
            using (_iLock.Acquire())
            {
                Module module = Read(resource);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }

        public DeploymentResult ParseDeploy(string eplModuleText)
        {
            using (_iLock.Acquire())
            {
                return ParseDeploy(eplModuleText, null, null, null);
            }
        }

        public DeploymentResult ParseDeploy(string buffer, string moduleURI, string moduleArchive, Object userObject)
        {
            using (_iLock.Acquire())
            {
                Module module = EPLModuleUtil.ParseInternal(buffer, moduleURI);
                return DeployQuick(module, moduleURI, moduleArchive, userObject);
            }
        }

        public void Add(Module module, string assignedDeploymentId)
        {
            using (_iLock.Acquire())
            {
                if (_deploymentStateService.GetDeployment(assignedDeploymentId) != null)
                {
                    throw new ArgumentException(
                        "Assigned deployment id '" + assignedDeploymentId + "' is already in use");
                }
                AddInternal(module, assignedDeploymentId);
            }
        }

        public string Add(Module module)
        {
            using (_iLock.Acquire())
            {
                string deploymentId = _deploymentStateService.NextDeploymentId;
                AddInternal(module, deploymentId);
                return deploymentId;
            }
        }

        private void AddInternal(Module module, string deploymentId)
        {

            var desc = new DeploymentInformation(
                deploymentId, module, DateTimeEx.GetInstance(_timeZone), DateTimeEx.GetInstance(_timeZone),
                new DeploymentInformationItem[0], DeploymentState.UNDEPLOYED);
            _deploymentStateService.AddUpdateDeployment(desc);
        }

        public DeploymentResult Deploy(string deploymentId, DeploymentOptions options)
        {
            using (_iLock.Acquire())
            {
                DeploymentInformation info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }
                if (info.State == DeploymentState.DEPLOYED)
                {
                    throw new DeploymentStateException(
                        "Module by deployment id '" + deploymentId + "' is already in deployed state");
                }
                GetDeploymentOrder(Collections.SingletonList(info.Module), null);
                return DeployInternal(info.Module, options, deploymentId, info.AddedDate);
            }
        }

        public void Remove(string deploymentId)
        {
            using (_iLock.Acquire())
            {
                DeploymentInformation info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }
                if (info.State == DeploymentState.DEPLOYED)
                {
                    throw new DeploymentStateException(
                        "Deployment by id '" + deploymentId + "' is in deployed state, please undeploy first");
                }
                _deploymentStateService.Remove(deploymentId);
            }
        }

        private UndeploymentResult UndeployRemoveInternal(string deploymentId, UndeploymentOptions options)
        {
            using (_iLock.Acquire())
            {
                DeploymentInformation info = _deploymentStateService.GetDeployment(deploymentId);
                if (info == null)
                {
                    throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
                }

                UndeploymentResult result;
                if (info.State == DeploymentState.DEPLOYED)
                {
                    result = UndeployRemoveInternal(info, options);
                }
                else
                {
                    result = new UndeploymentResult(
                        deploymentId, Collections.GetEmptyList<DeploymentInformationItem>());
                }
                _deploymentStateService.Remove(deploymentId);
                return result;
            }
        }

        private UndeploymentResult UndeployInternal(string deploymentId, UndeploymentOptions undeploymentOptions)
        {
            undeploymentOptions.DeploymentLockStrategy.Acquire(_eventProcessingRwLock);
            try
            {
                return UndeployInternalLockTaken(deploymentId, undeploymentOptions);
            }
            finally
            {
                undeploymentOptions.DeploymentLockStrategy.Release(_eventProcessingRwLock);
            }
        }

        private UndeploymentResult UndeployInternalLockTaken(
            string deploymentId,
            UndeploymentOptions undeploymentOptions)
        {
            DeploymentInformation info = _deploymentStateService.GetDeployment(deploymentId);
            if (info == null)
            {
                throw new DeploymentNotFoundException("Deployment by id '" + deploymentId + "' could not be found");
            }
            if (info.State == DeploymentState.UNDEPLOYED)
            {
                throw new DeploymentStateException(
                    "Deployment by id '" + deploymentId + "' is already in undeployed state");
            }

            UndeploymentResult result = UndeployRemoveInternal(info, undeploymentOptions);
            var updated = new DeploymentInformation(
                deploymentId, info.Module, info.AddedDate, DateTimeEx.GetInstance(_timeZone),
                new DeploymentInformationItem[0], DeploymentState.UNDEPLOYED);
            _deploymentStateService.AddUpdateDeployment(updated);
            return result;
        }

        private UndeploymentResult UndeployRemoveInternal(
            DeploymentInformation info,
            UndeploymentOptions undeploymentOptions)
        {
            var reverted = new DeploymentInformationItem[info.Items.Length];
            for (int i = 0; i < info.Items.Length; i++)
            {
                reverted[i] = info.Items[info.Items.Length - 1 - i];
            }

            var revertedStatements = new List<DeploymentInformationItem>();
            if (undeploymentOptions.IsDestroyStatements)
            {
                var referencedTypes = new HashSet<string>();

                Exception firstExceptionEncountered = null;

                foreach (DeploymentInformationItem item in reverted)
                {
                    EPStatement statement = _epService.GetStatement(item.StatementName);
                    if (statement == null)
                    {
                        Log.Debug("Deployment id '" + info.DeploymentId + "' statement name '" + item + "' not found");
                        continue;
                    }
                    referencedTypes.AddAll(_statementEventTypeRef.GetTypesForStatementName(statement.Name));
                    if (statement.IsDisposed)
                    {
                        continue;
                    }
                    try
                    {
                        statement.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Unexpected exception destroying statement: " + ex.Message, ex);
                        if (firstExceptionEncountered == null)
                        {
                            firstExceptionEncountered = ex;
                        }
                    }
                    revertedStatements.Add(item);
                }
                EPLModuleUtil.UndeployTypes(
                    referencedTypes, _statementEventTypeRef, _eventAdapterService, _filterService);
                revertedStatements.Reverse();

                if (firstExceptionEncountered != null &&
                    _undeployRethrowPolicy ==
                    ConfigurationEngineDefaults.UndeployRethrowPolicy.RETHROW_FIRST)
                {
                    throw firstExceptionEncountered;
                }
            }

            return new UndeploymentResult(info.DeploymentId, revertedStatements);
        }

        private DeploymentResult DeployQuick(Module module, string moduleURI, string moduleArchive, Object userObject)
        {
            module.Uri = moduleURI;
            module.ArchiveName = moduleArchive;
            module.UserObject = userObject;
            GetDeploymentOrder(Collections.SingletonList(module), null);
            return Deploy(module, null);
        }
    }
} // end of namespace
