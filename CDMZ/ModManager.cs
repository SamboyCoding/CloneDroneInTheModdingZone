using System;
using System.Collections.Generic;
using System.Linq;

namespace CDMZ
{
    public static class ModManager
    {
        internal static List<Mod> Mods = new List<Mod>();
        private static Logger _logger = new Logger("CDMZ|ModManager");
        
        private static List<string> disabledModNames = new List<string>();
        
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

        internal static void EnableAll()
        {
            var toEnable = Mods.Where(m => !disabledModNames.Contains(m.GetModName().ToLowerInvariant()));

            foreach (var mod in toEnable)
            {
                try
                {
                    _logger.Info($"Enabling mod {mod.GetModName()}");
                    mod.Enable();
                    _logger.Debug($"Enabled mod {mod.GetModName()}");
                }
                catch (Exception e)
                {
                    _logger.Exception(e, $"The mod {mod.GetModName()} threw an exception on enable");
                }
            }
        }
    }
}