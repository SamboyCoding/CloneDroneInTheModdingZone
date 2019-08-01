using System;
using System.Collections.Generic;
using System.Linq;

namespace CDMZ
{
    public static class ModManager
    {
        internal static List<Mod> Mods = new List<Mod>();
        private static Logger _logger = new Logger("CDMZ|ModConstruct");
        
        internal static void ConstructAll()
        {
            if(Mods.Count != 0) throw new InvalidOperationException("Cannot construct mods when they've already been constructed");

            foreach (var type in ModTypeManager.AllModClasses)
            {
                try
                {
                    _logger.Info($"Constructing mod of type {type}");
                    var mod = (Mod) Activator.CreateInstance(type);
                    Mods.Add(mod);
                    _logger.Debug($"Constructed mod: {mod.GetModName()}");
                }
                catch (Exception e)
                {
                    _logger.Exception(e, $"The mod type {type} threw an exception during construction");
                }
            }
        }
    }
}