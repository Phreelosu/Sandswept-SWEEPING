using System;
using System.Linq;
using RoR2.CharacterAI;
using RoR2.Navigation;
using Sandswept.Enemies.BrassBell.States;

namespace Sandswept.Enemies.BrassBell
{
    [ConfigSection("Enemies :: Brass Bell")]
    public class BrassBell : EnemyBase<BrassBell>
    {
        [ConfigField("Director Credit Cost", "", 20)]
        public static int directorCreditCost;

        public override DirectorCardCategorySelection family => null;
        public override MonsterCategory cat => MonsterCategory.BasicMonsters;
        public static GameObject BellShotProjectile;
        public static GameObject BellShotPrep;
        public static GameObject BellChimeEffect;

        public override void LoadPrefabs()
        {
            prefab = Main.assets.LoadAsset<GameObject>("BellConstructBody.prefab");
            prefabMaster = Main.assets.LoadAsset<GameObject>("BellConstructMaster.prefab");

            BellShotProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.BellBall, "BellShotProjectile");
            GameObject ghost = PrefabAPI.InstantiateClone(Paths.GameObject.BellBallGhost, "BellShotGhost");
            ghost.GetComponent<ProjectileGhostController>().inheritScaleFromProjectile = true;

            BellShotProjectile.EditComponent<ProjectileController>((x) => {
                x.ghostPrefabAddress = null;
                x.ghostPrefab = ghost;
            });

            BellShotPrep = PrefabAPI.InstantiateClone(Paths.GameObject.PreppedBellBall, "BellShotPrep");

            BellShotProjectile.transform.localScale *= 0.5f;
            ghost.transform.localScale *= 0.5f;
            BellShotPrep.transform.localScale *= 0.5f;

            ContentAddition.AddProjectile(BellShotProjectile);

            BellChimeEffect = Main.assets.LoadAsset<GameObject>("BrassChimeEffect.prefab");
        }

        public override void PostCreation()
        {
            base.PostCreation();
            RegisterEnemy(prefab, prefabMaster, null, MonsterCategory.BasicMonsters, false);

            DirectorCardHolder holder = new();
            holder.Card = card;
            holder.MonsterCategory = MonsterCategory.BasicMonsters;
            holder.MonsterCategorySelectionWeight = 1;

            DirectorAPI.Helpers.AddNewMonsterToStagesWhere(holder, true, (x) => {
                if (x.CustomStageName == "itobservatory_wormsworms") {
                    return true;
                }

                Stage[] stages = new DirectorAPI.Stage[] {
                    DirectorAPI.Stage.AbyssalDepths, DirectorAPI.Stage.AbyssalDepthsSimulacrum,
                    DirectorAPI.Stage.WetlandAspect,
                    DirectorAPI.Stage.TreebornColony, DirectorAPI.Stage.GoldenDieback,
                    DirectorAPI.Stage.LuridBasilica,
                    DirectorAPI.Stage.RepurposedCrater,
                    DirectorAPI.Stage.SirensCall,
                    DirectorAPI.Stage.SkyMeadow, DirectorAPI.Stage.SkyMeadowSimulacrum,
                    DirectorAPI.Stage.TenebrousQuagmire,
                    DirectorAPI.Stage.WinnowingOfPredation,
                    DirectorAPI.Stage.AbandonedAqueduct, DirectorAPI.Stage.AbandonedAqueductSimulacrum,
                    DirectorAPI.Stage.ArtifactReliquary_AbandonedAqueduct_Theme,
                    DirectorAPI.Stage.ArtifactReliquary_AphelianSanctuary_Theme,
                    DirectorAPI.Stage.PretendersPrecipice,
                };

                return stages.Contains(x.stage);
            });
        }

        public override void Modify()
        {
            base.Modify();

            master.bodyPrefab = prefab;
            body.baseNameToken.Add("Brass Bell");

            var locator = body.GetComponent<SkillLocator>();

            ReplaceSkill(locator.primary, BarrageSkill.instance);
            ReplaceSkill(locator.secondary, ChimeSkill.instance);

            // body.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(ArdentWispDeath));
            // body.GetComponent<CharacterDeathBehavior>().deathStateMachine.initialStateType = new(typeof(ArdentWispSpawn));

            LanguageAPI.Add(body.baseNameToken.Replace("_NAME", "_LORE"),
            """
            
            """);
        }

        public override void AddDirectorCard()
        {
            base.AddDirectorCard();
            card.selectionWeight = 1;
            card.spawnCard = csc;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
        }

        public override void AddSpawnCard()
        {
            base.AddSpawnCard();
            csc.directorCreditCost = directorCreditCost;
            csc.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            csc.hullSize = HullClassification.Human;
            csc.nodeGraphType = MapNodeGroup.GraphType.Air;
            csc.sendOverNetwork = true;
            csc.prefab = prefabMaster;
            csc.name = "cscBrassBell";
        }

        public override void SetUpIDRS()
        {
            CollapseIDRS();
        }
    }
}