using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Session;

namespace Dynamo.Configuration
{
    class ExecutionSession : IExecutionSession
    {
        private IPathManager pathManager;
        private Dictionary<string, object> parameters = new Dictionary<string,object>();
        
        /// <summary>
        /// Constructs a new execution session.
        /// </summary>
        public ExecutionSession(Scheduler.UpdateGraphAsyncTask updateTask, DynamoModel model, string geometryFactoryPath)
        {
            CurrentWorkspacePath = updateTask.TargetedWorkspace.FileName;
            pathManager = model.PathManager;
            parameters[ParameterKeys.GeometryFactory] = geometryFactoryPath;
            parameters[ParameterKeys.MajorVersion] = pathManager.MajorFileVersion;
            parameters[ParameterKeys.MinorVersion] = pathManager.MinorFileVersion;
            parameters[ParameterKeys.NumberFormat] = model.PreferenceSettings.NumberFormat;
            parameters[ParameterKeys.LastExecutionDuration] = new TimeSpan(updateTask.ExecutionEndTime.TickCount - updateTask.ExecutionStartTime.TickCount);
            parameters[ParameterKeys.PackagePaths] = pathManager.PackagesDirectories;
            parameters[ParameterKeys.Logger] = model.Logger;
            parameters[ParameterKeys.NoNetworkMode] = model.NoNetworkMode;
        }

        /// <summary>
        /// File path for the current workspace in execution. Could be null or
        /// empty string if workspace is not yet saved.
        /// </summary>
        public string CurrentWorkspacePath { get; private set; }

        /// <summary>
        /// Returns session parameter value for the given parameter name.
        /// </summary>
        /// <param name="parameter">Name of session parameter</param>
        /// <returns>Session parameter value as object</returns>
        public object GetParameterValue(string parameter)
        {
            return parameters[parameter];
        }

        /// <summary>
        /// Returns list of session parameter keys available in the session.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetParameterKeys()
        {
            return parameters.Keys;
        }

        /// <summary>
        /// A helper method to resolve the given file path. The given file path
        /// will be resolved by searching into the current workspace, core and 
        /// host application installation folders etc.
        /// </summary>
        /// <param name="filepath">Input file path</param>
        /// <returns>True if the file is found</returns>
        public bool ResolveFilePath(ref string filepath)
        {
            if (File.Exists(filepath))
                return true;

            var input = filepath;
            var filename = Path.GetFileName(filepath);
            var worspaceDir = Path.GetDirectoryName(CurrentWorkspacePath);
            filepath = Path.Combine(worspaceDir, filename);
            if (File.Exists(filepath))
                return true;

            if (pathManager == null && pathManager.ResolveLibraryPath(ref filepath))
                return true;

            filepath = input;
            return false;
        }
    }
}
