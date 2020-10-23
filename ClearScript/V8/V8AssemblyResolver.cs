using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// V8 assembly resolver
    /// </summary>
    internal static class V8AssemblyResolver
    {
        /// <summary>
        /// Synchronizer of V8 assembly resolver initialization
        /// </summary>
        private static readonly object _initializationSynchronizer = new object();

        /// <summary>
        /// Flag indicating whether the V8 assembly resolver is initialized
        /// </summary>
        private static bool _initialized;


        /// <summary>
        /// Initialize a V8 assembly resolver
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializationSynchronizer)
            {
                if (_initialized)
                {
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    string baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
                    if (string.IsNullOrEmpty(baseDirectoryPath))
                    {
                        // `PrivateBinPath` property is empty in test scenarios, so
                        // need to use the `BaseDirectory` property
                        baseDirectoryPath = currentDomain.BaseDirectory;
                    }

                    string platform = Environment.Is64BitProcess ? "x64" : "x86";
                    string assemblyFileName = "ClearScriptV8.dll";
                    string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platform);
                    string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
                    bool assemblyFileExists = File.Exists(assemblyFilePath);

                    if (!assemblyFileExists)
                    {
                        return;
                    }

                    if (!NativeMethods.SetDllDirectory(assemblyDirectoryPath))
                    {
                        throw new InvalidOperationException(string.Format(
                            "Failed to add the '{0}' directory to the search path " +
                            "used to locate DLLs for the application.",
                            assemblyDirectoryPath
                        ));
                    }
                }

                _initialized = true;
            }
        }
    }
}