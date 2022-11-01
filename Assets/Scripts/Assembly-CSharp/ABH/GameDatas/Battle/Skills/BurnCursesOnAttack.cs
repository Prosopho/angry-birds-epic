using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class BurnCursesOnAttack : SkillBattleDataBase
	{
		protected float m_BonusDamageInPercent;

		protected float m_RemoveCurses;

		protected float m_Chance;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			base.Model.SkillParameters.TryGetValue("bonus_damage_in_percent", out m_BonusDamageInPercent);
			base.Model.SkillParameters.TryGetValue("chance", out m_Chance);
			base.Model.SkillParameters.TryGetValue("remove_effects", out m_RemoveCurses);
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger set bonus skill: " + base.Model.Balancing.NameId + "; Target: " + target.CombatantName);
			m_Source = source;
			m_InitialTarget = target;
			m_Targets = new List<ICombatant> { target };
			foreach (ICombatant target2 in m_Targets)
			{
				List<float> list = new List<float>();
				list.Add(m_BonusDamageInPercent);
				list.Add(m_Chance);
				list.Add(m_RemoveCurses);
				BattleEffectGameData battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.BeforeDealDamage,
						EffectType = BattleEffectType.BurnCurses,
						Values = list,
						AfflicionType = base.Model.Balancing.EffectType,
						Duration = base.Model.Balancing.EffectDuration,
						EffectAssetId = base.Model.Balancing.EffectIconAssetId,
						EffectAtlasId = base.Model.Balancing.EffectIconAtlasId
					}
				}, base.Model.Balancing.EffectDuration, battle, base.Model.Balancing.AssetId, SkillEffectTypes.SetPassive, GetLocalizedName(), base.Model.SkillNameId);
				battleEffectGameData.AddEffect(true);
			}
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DoActionInstant(battle, source, target);
			yield break;
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_BonusDamageInPercent.ToString("0"));
			dictionary.Add("{value_3}", m_Chance.ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
