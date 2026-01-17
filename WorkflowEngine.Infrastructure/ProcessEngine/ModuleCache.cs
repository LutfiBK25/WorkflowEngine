using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

namespace WorkflowEngine.Infrastructure.ProcessEngine;

public class ModuleCache
{
    private readonly Dictionary<Guid, Dictionary<Guid, Module>> _applicationModules;
    private readonly object _lock = new object(); // To protect since its a singleton


    public ModuleCache()
    {
        _applicationModules = new Dictionary<Guid, Dictionary<Guid, Module>>();
    }


    /// <summary>
    /// Gets all modules for a specific application
    /// </summary>
    public Dictionary<Guid, Module> GetModulesForApplication(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return new Dictionary<Guid, Module>(modules); // Return a copy for thread safety
            }
            return new Dictionary<Guid, Module>();
        }
    }

    /// <summary>
    /// Gets a specific module by ID from a specific application
    /// </summary>
    public Module? GetModule(Guid applicationId, Guid moduleId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                modules.TryGetValue(moduleId, out var module);
                return module;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets all ProcessModules for an application
    /// </summary>
    public IEnumerable<ProcessModule> GetProcessModules(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Values.OfType<ProcessModule>().ToList();
            }
            return Enumerable.Empty<ProcessModule>();
        }
    }

    /// <summary>
    /// Gets all DatabaseActionModules for an application
    /// </summary>
    public IEnumerable<DatabaseActionModule> GetDatabaseActionModules(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Values.OfType<DatabaseActionModule>().ToList();
            }
            return Enumerable.Empty<DatabaseActionModule>();
        }
    }

    /// <summary>
    /// Gets all DialogActionModules for an application
    /// </summary>
    public IEnumerable<DialogActionModule> GetDialogActionModules(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Values.OfType<DialogActionModule>().ToList();
            }
            return Enumerable.Empty<DialogActionModule>();
        }
    }

    /// <summary>
    /// Gets all FieldModules for an application
    /// </summary>
    public IEnumerable<FieldModule> GetFieldModules(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Values.OfType<FieldModule>().ToList();
            }
            return Enumerable.Empty<FieldModule>();
        }
    }

    /// <summary>
    /// Gets a module by name from a specific application
    /// </summary>
    public Module? GetModuleByName(Guid applicationId, string moduleName)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Values.FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            }
            return null;
        }
    }


    /// <summary>
    /// Adds or updates a module in the cache for a specific application
    /// </summary>
    public void AddOrUpdateModule(Guid applicationId, Module module)
    {
        lock (_lock)
        {
            if (!_applicationModules.ContainsKey(applicationId))
            {
                _applicationModules[applicationId] = new Dictionary<Guid, Module>();
            }

            _applicationModules[applicationId][module.Id] = module;
        }
    }

    /// <summary>
    /// Loads all modules for an application (replaces existing cache for that app)
    /// Note: If a module has childer it must be included thier child collections (like ProcessModule.Details)
    /// </summary>
    public void LoadApplicationModules(Guid applicationId, IEnumerable<Module> modules)
    {
        lock (_lock)
        {
            _applicationModules[applicationId] = modules.ToDictionary(m => m.Id, m => m);
        }
    }


    /// <summary>
    /// Gets all application IDs that have cached modules
    /// </summary>
    public IEnumerable<Guid> GetCachedApplicationIds()
    {
        lock (_lock)
        {
            return _applicationModules.Keys.ToList();
        }
    }

    /// <summary>
    /// Checks if an application has cached modules
    /// </summary>
    public bool HasApplication(Guid applicationId)
    {
        lock (_lock)
        {
            return _applicationModules.ContainsKey(applicationId);
        }
    }

    /// <summary>
    /// Clears all modules for a specific application
    /// </summary>
    public void ClearApplication(Guid applicationId)
    {
        lock (_lock)
        {
            _applicationModules.Remove(applicationId);
        }
    }


    /// <summary>
    /// Removes a specific module from the cache
    /// </summary>
    public bool RemoveModule(Guid applicationId, Guid moduleId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Remove(moduleId);
            }
            return false;
        }
    }

    /// <summary>
    /// Clears the entire cache
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _applicationModules.Clear();
        }
    }

    /// <summary>
    /// Gets the count of modules for a specific application
    /// </summary>
    public int GetModuleCount(Guid applicationId)
    {
        lock (_lock)
        {
            if (_applicationModules.TryGetValue(applicationId, out var modules))
            {
                return modules.Count;
            }
            return 0;
        }
    }



}
