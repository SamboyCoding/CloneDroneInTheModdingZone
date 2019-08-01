using System.Linq;
using CDMZ.EventSystem;
using UnityEngine;

namespace CDMZ
{
    public static class EventListener
    {
        public static void Init()
        {
            EventBus.Instance.Register((CharacterPreSpawnEvent e) =>
            {
                //I don't like mk1 bows
                if (!e.IsPlayerSpawn && e.enemyType == EnemyType.Bowman1)
                    e.Cancel();
            });
            
            EventBus.Instance.Register((PreDamageEvent e) =>
            {
                //Player godmode
                if (e.Damagee.IsMainPlayer())
                    e.Cancel();
            });
            
            EventBus.Instance.Register((MainMenuShownEvent evt) =>
            {
//                var log = new Logger("RaptorDump");
//                var prefab = EnemyFactory.Instance.Enemies.First(e => e.EnemyType == EnemyType.FireRaptor).EnemyPrefab;
//                
//                log.Debug(prefab.gameObject.ToString());
//
//                foreach (var component in prefab.GetComponents<Component>())
//                {
//                    log.Debug("    Component: " + component);
//                }
//
//                var model = prefab.GetComponent<FirstPersonMover>().CharacterModelPrefab;
//                log.Debug(model.UpperAnimator?.ToString());
//                log.Debug(model.LegsAnimator?.ToString());
//                log.Debug(model.HipsTransform?.ToString());
//                log.Debug(model.TorsoTransform?.ToString());
//                log.Debug(model.SpineTransform?.ToString());
//                log.Debug(model.TorsoBodyPart?.ToString());
//                log.Debug(model.LegsTransform?.ToString());
//                log.Debug(model.FloorCollider?.ToString());
//                log.Debug(model.LowerLegRight?.ToString());
//                log.Debug(model.LowerLegLeft?.ToString());
//                log.Debug(model.MouthSpawnPoint?.ToString());
//                log.Debug(model.ChestSpawnPoint?.ToString());
//                log.Debug(model.BombSpawnPoint?.ToString());
//                log.Debug(model.SummonAllySpawnPoint?.ToString());
//                log.Debug(model.RiderContainer?.ToString());
//                log.Debug(model.HeadTopAttachPoint?.ToString());
//                log.Debug(model.PatternColor.ToString());
//                log.Debug(model.SecondPatternColor.ToString());
//                log.Debug(model.DefaultPatternOverrideColor.ToString());
//                log.Debug(model.CenterOfChestOverride?.ToString());
//                log.Debug(model.SummonSpawnLight?.ToString());
//                log.Debug(model.WeaponModels?.ToString());
//                log.Debug(model.ArrowHolder?.ToString());
//                log.Debug(model.JetpackVFX?.ToString());
//                log.Debug(model.DeactivateOnDeath?.ToString());
//                log.Debug(model.DeactivateOnFlameBreath?.ToString());
//                log.Debug(model.ConstructionProps?.ToString());

            });
        }
    }
}