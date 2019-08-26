using System;
using CDMZ.EventSystem;

namespace CDMZ.Compat
{
    //Obsolete
#pragma warning disable 618
    public class ModBotWrapperMod : Mod
    {
        private ModLibrary.Mod _wrapped;

        internal ModBotWrapperMod(ModLibrary.Mod wrapped)
        {
            _wrapped = wrapped;
        }

        public override string GetModName()
        {
            return $"MBC [{_wrapped.GetModName()}]";
        }

        public override string GetModDescription()
        {
            return _wrapped.GetModDescription();
        }

        public override string GetVersion()
        {
            return "[Compat]";
        }

        public override void Enable()
        {
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnCharacterSpawned), ((CharacterPostSpawnEvent evt) => _wrapped.OnCharacterSpawned(evt.Character.gameObject)));
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnFirstPersonMoverSpawned), (CharacterPostSpawnEvent evt) => _wrapped.OnFirstPersonMoverSpawned(evt.Character.gameObject));
            
            if(ReflectionHelper.ClassOverridesMethod(_wrapped.GetType(), nameof(ModLibrary.Mod.OnModRefreshed)))
                _wrapped.OnModRefreshed();
        }

        public override void Disable()
        {
            //No-op, mod bot doesn't really do disabling
        }


        /// <summary>
        /// Let's try NOT to clutter up the event bus with unneeded event listeners.
        /// </summary>
        /// <param name="methodName">The name of the method to check for an override</param>
        /// <param name="handler">The handler to register if the method is overridden</param>
        private void RegisterEventHandlerIfNeeded<T>(string methodName, Action<T> handler) where T: Event
        {
            if (ReflectionHelper.ClassOverridesMethod(_wrapped.GetType(), methodName))
            {
                EventBus.Instance.Register(handler);
            }
        }
    }
}