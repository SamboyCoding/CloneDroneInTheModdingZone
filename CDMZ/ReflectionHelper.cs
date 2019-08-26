using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CDMZ.Compat;

namespace CDMZ
{
    public static class ReflectionHelper
    {
        public static List<Assembly> ModAssemblies = new List<Assembly>();

        public static IEnumerable<Assembly> AllGameRelatedAssemblies
        {
            get
            {
                var ret = new List<Assembly> {Assembly.GetExecutingAssembly(), typeof(SplashScreenFlowManager).Assembly};
                ret.AddRange(ModAssemblies);
                return ret;
            }
        }

        public static IEnumerable<Type> AllGameRelatedTypes => AllGameRelatedAssemblies
            .SelectMany(a => a.GetTypes())
            .ToList();

        public static IEnumerable<Type> AllModClasses => AllSubclassesOf(typeof(Mod)).Where(t => !t.IsAbstract && t != typeof(ModBotWrapperMod));

        public static List<Type> AllDirectSubclassesOf(Type t)
        {
            return AllGameRelatedTypes
                .Where(type => type.BaseType == t)
                .ToList();
        }

        public static List<Type> AllSubclassesOf(Type t)
        {
            return AllGameRelatedTypes
                .Where(t.IsAssignableFrom)
                .ToList();
        }

        public static bool ClassOverridesMethod(Type t, string name)
        {
            var method = t.GetMethod(name);
            if (method == null) return false;

            return method.DeclaringType == t;
        }
    }
}