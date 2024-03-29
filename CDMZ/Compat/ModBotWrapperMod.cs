using System;
using CDMZ.EventSystem;

namespace CDMZ.Compat
{
    //Obsolete warning
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
            //AFAICT OnSceneChanged is only ever called once, when the main menu is shown, with game mode NONE
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnSceneChanged), (MainMenuShownEvent evt) => _wrapped.OnSceneChanged(GameMode.None));
            
            //Character spawn
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnCharacterSpawned), (CharacterPostSpawnEvent evt) => _wrapped.OnCharacterSpawned(evt.Character.gameObject));
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnFirstPersonMoverSpawned), (CharacterPostSpawnEvent evt) =>
            {
                if(evt.Character is FirstPersonMover)
                    _wrapped.OnFirstPersonMoverSpawned(evt.Character.gameObject);
            });
            
            //Level editor
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnLevelEditorStarted), (LevelEditorShownEvent evt) => _wrapped.OnLevelEditorStarted());
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnObjectPlacedInLevelEditor), (LevelEditorObjectPlacedEvent evt) => _wrapped.OnObjectPlacedInLevelEditor(evt.PlacedObject.gameObject));
            
            //Projectiles
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnProjectileCreated), (ProjectileCreatedEvent evt) => _wrapped.OnProjectileCreated(evt.Projectile.gameObject));
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnArrowProjectileCreated), (ProjectileCreatedEvent evt) =>
            {
                if(evt.Projectile is ArrowProjectile projectile)
                    _wrapped.OnArrowProjectileCreated(projectile);
            });
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnBulletProjectileCreated), (ProjectileCreatedEvent evt) =>
            {
                if(evt.Projectile is BulletProjectile projectile)
                    _wrapped.OnBulletProjectileCreated(projectile,
                        evt.ProjectileType == ProjectileCreatedEvent.Type.SHRAPNEL,
                        evt.ProjectileType == ProjectileCreatedEvent.Type.FLAME, 
                        evt.ProjectileType == ProjectileCreatedEvent.Type.REPAIR);
            });
            
            //Upgrades
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.OnUpgradesRefreshed), (UpgradesAboutToBeRefreshedEvent evt) => _wrapped.OnUpgradesRefreshed(evt.FirstPersonMover.gameObject, evt.FirstPersonMover.GetComponent<UpgradeCollection>()));
            RegisterEventHandlerIfNeeded(nameof(ModLibrary.Mod.AfterUpgradesRefreshed), (UpgradesRefreshedEvent evt) => _wrapped.AfterUpgradesRefreshed(evt.FirstPersonMover.gameObject, evt.FirstPersonMover.GetComponent<UpgradeCollection>()));
            
            //Call OnModRefreshed as it's sort of ModBot's Enable method
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