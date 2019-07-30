using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CDMZ
{
    public class ModTypeManager
    {
        private static List<Assembly> _modAssemblies = new List<Assembly>();

        public static List<Assembly> AllGameRelatedAssemblies
        {
            get
            {
                var ret = new List<Assembly> {Assembly.GetExecutingAssembly(), typeof(SplashScreenFlowManager).Assembly};
                ret.AddRange(_modAssemblies);
                return ret;
            }
        }

        public static List<Type> AllGameRelatedTypes
        {
            get
            {
                return AllGameRelatedAssemblies
                    .SelectMany(a => a.GetTypes())
                    .ToList();
            }
        }

        public static List<Type> AllDirectSubclassesOf(Type t)
        {
            return AllGameRelatedTypes
                .Where(type => type.BaseType == t)
                .ToList();
        }

        public static List<Type> AllSubclassesOf(Type t)
        {
            return AllGameRelatedTypes
                .Where(type => t.IsAssignableFrom(type))
                .ToList();
        }
    }
}