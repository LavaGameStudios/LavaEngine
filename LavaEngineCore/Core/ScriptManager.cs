using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LavaEngine.Log;

namespace LavaEngine.Core.ScriptManagerCore;

public class ScriptManager
{
    private Dictionary<Type, MethodInfo> updateMethods = new Dictionary<Type, MethodInfo>();
    private Dictionary<Type, MethodInfo> lateUpdateMethods = new Dictionary<Type, MethodInfo>();
    private Dictionary<Type, MethodInfo> fixedUpdateMethods = new Dictionary<Type, MethodInfo>();
    
    private Dictionary<Type, object> scriptInstances = new Dictionary<Type, object>();

    public Assembly LoadScripts(string projPath, string CompilingMode) 
    {
        Assembly scripts = Assembly.LoadFrom($"{Path.GetDirectoryName(projPath)}/bin/{CompilingMode}/scripts.dll");
        
        
        if (scripts == null)
        {
            new Logger().Error("Can't load scripts linking library. Is it deleted?");
            return null;
        }
        
        return scripts;
    }

    public Type GetScriptClass(Assembly assembly, string class_name)
    {
        Logger logger = new Logger();
        Type target = assembly.GetType(class_name);

        if(target != null)
        {
            return target;
        }
        else
        {
            logger.Error($"{class_name} class is not in your script.");
            return null;
        }
    }

    public MethodInfo GetMethodFromClass(Type classType, string target_method)
    {
        if (classType == null) return null; 
        
        MethodInfo method = classType.GetMethod(target_method);
        if (method == null)
        {
            new Logger().Error($"Method '{target_method}' not found in class '{classType.Name}'.");
            return null;
        }

        return method;
    }

    public object ExecuteStaticMethod(MethodInfo method, params object[] parameters)
    {
        if (method == null) return -1;
        return method.Invoke(null, parameters);
    }

    public object ExecuteMethod(MethodInfo method, object instance, params object[] parameters)
    {
        if (method == null) return null; 

        if (method.IsStatic)
        {
            return method.Invoke(null, parameters);
        }
        else
        {
            if (instance == null)
            {
                new Logger().Error($"Cannot execute instance method '{method.Name}' without an object instance.");
                return null;
            }
            return method.Invoke(instance, parameters);
        }
    }

    public void CacheAllMethods(Assembly assembly, Type excludeClass)
    {
        try
        {
            Type[] allTypes = assembly.GetTypes();

            foreach (Type type in allTypes)
            {
                if (type.IsClass && !type.IsAbstract && type != excludeClass)
                {
                    MethodInfo startMethod = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
                    if (startMethod != null)
                    {
                        if (!scriptInstances.ContainsKey(type))
                            scriptInstances[type] = Activator.CreateInstance(type);
                    }

                    MethodInfo updateMethod = type.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
                    if (updateMethod != null)
                    {
                        updateMethods[type] = updateMethod;
                        if (!scriptInstances.ContainsKey(type))
                            scriptInstances[type] = Activator.CreateInstance(type);
                    }

                    MethodInfo lateUpdateMethod = type.GetMethod("LateUpdate", BindingFlags.Public | BindingFlags.Instance);
                    if (lateUpdateMethod != null)
                    {
                        lateUpdateMethods[type] = lateUpdateMethod;
                        if (!scriptInstances.ContainsKey(type))
                            scriptInstances[type] = Activator.CreateInstance(type);
                    }

                    MethodInfo fixedUpdateMethod = type.GetMethod("FixedUpdate", BindingFlags.Public | BindingFlags.Instance);
                    if (fixedUpdateMethod != null)
                    {
                        fixedUpdateMethods[type] = fixedUpdateMethod;
                        if (!scriptInstances.ContainsKey(type))
                            scriptInstances[type] = Activator.CreateInstance(type);
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            new Logger().Error($"Failed to load some types from assembly. Error mes:{ex}");
        }
    }

    public void CallAllStartMethods()
    {
        foreach (var kvp in scriptInstances)
        {
            MethodInfo startMethod = kvp.Key.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
            if (startMethod != null)
            {
                startMethod.Invoke(kvp.Value, null);
            }
        }
    }

    public void CallAllUpdates()
    {
        foreach (var kvp in updateMethods)
        {
            kvp.Value.Invoke(scriptInstances[kvp.Key], null);
        }
    }

    public void CallAllLateUpdates()
    {
        foreach (var kvp in lateUpdateMethods)
        {
            kvp.Value.Invoke(scriptInstances[kvp.Key], null);
        }
    }

    public void CallAllFixedUpdates()
    {
        foreach (var kvp in fixedUpdateMethods)
        {
            kvp.Value.Invoke(scriptInstances[kvp.Key], null);
        }
    }
}