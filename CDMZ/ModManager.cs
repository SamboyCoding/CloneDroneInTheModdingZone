using System;
using System.Collections.Generic;
using System.Linq;
using CDMZ.Compat;

namespace CDMZ
{
    public static class ModManager
    {
        internal static List<Mod> Mods = new List<Mod>();
        internal static List<Type> ConstructErroredMods = new List<Type>();
        internal static List<Mod> EnableErroredMods = new List<Mod>();
        
        private static Logger _logger = new Logger("CDMZ|ModManager");
        private static List<string> disabledModNames = new List<string>();

        internal static void ConstructAll()
        {
            if(Mods.Count != 0) throw new InvalidOperationException("Cannot construct mods when they've already been constructed");

            foreach (var type in ReflectionHelper.AllModClasses)
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
                    ConstructErroredMods.Add(type);
                }
            }
        }

        internal static void ExecuteEnableForEnabledMods()
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
                    EnableErroredMods.Add(mod);
                }
            }
        }

#pragma warning disable 618
        public static void LoadModBotMods()
        {
            foreach (var type in ReflectionHelper.AllSubclassesOf(typeof(ModLibrary.Mod))) {
                try
                {
                    _logger.Info($"Constructing compat-mode mod {type}");

                    var m = (ModLibrary.Mod) Activator.CreateInstance(type);
                    var wrapper = new ModBotWrapperMod(m);
                    Mods.Add(wrapper);

                    _logger.Debug($"Constructed compat-mode mod: {wrapper.GetModName()}");
                }
                catch (Exception e)
                {
                    _logger.Exception(e, $"The compat-mode mod type {type} threw an exception during construction");
                    ConstructErroredMods.Add(type);
                }
            }
        }
        
#pragma warning restore 618
    }
}