using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if !NETFRAMEWORK
using System.Runtime.Loader;
#endif
using SpieRibbon.Contracts;

namespace SpieRibbon.Core
{
    /// <summary>
    /// Discovers and loads discipline modules from the per-version Modules folder.
    /// A module that fails to load is skipped with a collected warning - it never takes down
    /// the other modules or the toolbox itself.
    /// </summary>
    public class ModuleLoader
    {
        private readonly string _modulesDir;
        private bool _resolverHooked;

        public List<string> Warnings { get; } = new List<string>();

        public ModuleLoader(string modulesDir)
        {
            _modulesDir = modulesDir;
        }

        public List<LoadedModule> LoadAll(ISpieHost host)
        {
            var result = new List<LoadedModule>();

            if (!Directory.Exists(_modulesDir))
                return result; // No modules deployed yet - toolbox still opens, just empty.

            HookDependencyResolver();

            foreach (string dllPath in Directory.GetFiles(_modulesDir, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = LoadModuleAssembly(dllPath);
                }
                catch (Exception ex)
                {
                    Warnings.Add(string.Format("Could not load '{0}': {1}",
                        Path.GetFileName(dllPath), ex.Message));
                    continue;
                }

                foreach (Type type in GetModuleTypes(assembly))
                {
                    try
                    {
                        var module = (ISpieModule)Activator.CreateInstance(type);
                        result.Add(new LoadedModule
                        {
                            Name = module.Name,
                            Groups = module.BuildGroups(host) ?? new List<ToolGroup>()
                        });
                    }
                    catch (Exception ex)
                    {
                        Warnings.Add(string.Format("Module '{0}' failed to initialise: {1}",
                            type.FullName, ex.Message));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Loads a module into the SAME context as the host so its reference to Contracts.dll
        /// resolves to the host's already-loaded copy (same type identity - required for the
        /// ISpieModule cast). On .NET 8 (Revit 2025+) the host lives in its own
        /// AssemblyLoadContext; a plain Assembly.LoadFrom would land in the default context and
        /// give the module a *different* Contracts identity. On .NET Framework (Revit 2024)
        /// there is one load context, so LoadFrom is correct.
        /// </summary>
        private static Assembly LoadModuleAssembly(string path)
        {
#if NETFRAMEWORK
            return Assembly.LoadFrom(path);
#else
            return HostLoadContext().LoadFromAssemblyPath(path);
#endif
        }

#if !NETFRAMEWORK
        private static AssemblyLoadContext HostLoadContext()
        {
            return AssemblyLoadContext.GetLoadContext(typeof(ModuleLoader).Assembly)
                   ?? AssemblyLoadContext.Default;
        }
#endif

        private Assembly ResolveFromModulesDir(string simpleName)
        {
            string candidate = Path.Combine(_modulesDir, simpleName + ".dll");
            if (!File.Exists(candidate))
                return null;

            try { return LoadModuleAssembly(candidate); }
            catch { return null; }
        }

        /// <summary>
        /// Resolves module dependencies (e.g. ClosedXML) that live in the Modules folder but
        /// aren't on Revit's normal probing path. Hooks the right resolution event per framework.
        /// </summary>
        private void HookDependencyResolver()
        {
            if (_resolverHooked)
                return;

#if NETFRAMEWORK
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                ResolveFromModulesDir(new AssemblyName(args.Name).Name);
#else
            HostLoadContext().Resolving += (context, name) =>
                ResolveFromModulesDir(name.Name);
#endif
            _resolverHooked = true;
        }

        private static IEnumerable<Type> GetModuleTypes(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // A dependency DLL sitting in the folder may not fully load - take what we can.
                types = ex.Types.Where(t => t != null).ToArray();
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }

            return types.Where(t => t != null && !t.IsAbstract && !t.IsInterface &&
                                    typeof(ISpieModule).IsAssignableFrom(t));
        }
    }
}
