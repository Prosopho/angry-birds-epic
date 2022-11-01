using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.GameDatas.Battle.Skills
{
	public class HotSkill : SkillBattleDataBase
	{
		protected float m_Percent;

		protected float m_Fixed;

		protected float m_HotChance;

		protected bool m_All;

		protected bool m_Self;

		protected float m_InvokersHealth;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			base.Model.SkillParameters.TryGetValue("health_in_percent", out m_Percent);
			base.Model.SkillParameters.TryGetValue("hot_chance", out m_HotChance);
			m_All = base.Model.SkillParameters.ContainsKey("all");
			m_Self = base.Model.SkillParameters.ContainsKey("self");
			if (base.Model.SkillParameters.ContainsKey("invoker"))
			{
				m_InvokersHealth = 1f;
			}
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DebugLog.Log("Trigger support skill: " + base.Model.Balancing.NameId + "; Target: " + target.CombatantName);
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
				ICombatant target2 = default(ICombatant);
				m_Targets.AddRange(battle.m_CombatantsByInitiative.Where((ICombatant c) => c.CombatantFaction == target2.CombatantFaction).ToList());
			}
			SpawnVisualEffects(VisualEffectSpawnTiming.Start, m_VisualEffectSetting);
			yield return new WaitForSeconds(source.CombatantView.PlaySupportAnimation());
			foreach (ICombatant skillTarget in m_Targets)
			{
				if (m_Percent != 0f && Random.value <= m_HotChance / 100f)
				{
					skillTarget.CombatantView.PlayCheerCharacter();
					List<float> valueList = new List<float> { m_Percent, m_InvokersHealth };
					BattleEffectGameData effect = new BattleEffectGameData(source, skillTarget, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.OnDealDamagePerTurn,
							EffectType = BattleEffectType.DoHeal,
							Values = valueList,
							AfflicionType = base.Model.Balancing.EffectType,
							Duration = base.Model.Balancing.EffectDuration,
							EffectAssetId = base.Model.Balancing.EffectIconAssetId,
							EffectAtlasId = base.Model.Balancing.EffectIconAtlasId
						}
					}, base.Model.Balancing.EffectDuration, battle, base.Model.Balancing.AssetId, SkillEffectTypes.Blessing, GetLocalizedName(), base.Model.SkillNameId);
					effect.AddEffect(true);
				}
			}
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", (!(m_InvokersHealth > 0f)) ? m_Percent.ToString("0") : Mathf.RoundToInt(invoker.ModifiedHealth * m_Percent / 100f).ToString("0"));
			dictionary.Add("{value_2}", base.Model.Balancing.EffectDuration.ToString("0"));
			dictionary.Add("{value_3}", m_HotChance.ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
