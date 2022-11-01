using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class ChanceToDodge : SkillBattleDataBase
	{
		protected float m_DamageReduction = 100f;

		protected float m_Chance;

		protected bool m_All;

		protected bool m_Self;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			base.Model.SkillParameters.TryGetValue("chance", out m_Chance);
			base.Model.SkillParameters.TryGetValue("damage_reduction_percent", out m_DamageReduction);
			m_All = base.Model.SkillParameters.ContainsKey("all");
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger set bonus skill: " + base.Model.Balancing.NameId + "; Target: " + target.CombatantName);
			m_Source = source;
			m_InitialTarget = target;
			if (!m_All)
			{
				if (m_Self)
				{
					m_Targets = new List<ICombatant> { source };
					m_InitialTarget = source;
				}
				else
				{
					m_Targets = new List<ICombatant> { target };
				}
			}
			else
			{
				m_Targets = new List<ICombatant>();
				m_Targets.AddRange(battle.m_CombatantsPerFaction[target.CombatantFaction]);
			}
			foreach (ICombatant target2 in m_Targets)
			{
				List<float> list = new List<float>();
				EffectTriggerType effectTrigger = ((m_DamageReduction != 100f) ? EffectTriggerType.OnReceiveDamage : EffectTriggerType.BeforeReceiveDamage);
				BattleEffectType effectType = ((m_DamageReduction == 100f) ? BattleEffectType.Evade : BattleEffectType.ChanceToReduceDamage);
				list.Add(m_Chance);
				list.Add(m_DamageReduction);
				BattleEffectGameData battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = effectTrigger,
						EffectType = effectType,
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
			dictionary.Add("{value_1}", m_Chance.ToString("0"));
			dictionary.Add("{value_2}", m_DamageReduction.ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
