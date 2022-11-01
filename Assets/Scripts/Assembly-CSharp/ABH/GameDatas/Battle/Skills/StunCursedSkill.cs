using System;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.GameDatas.Battle.Skills
{
	public class StunCursedSkill : AttackSkillTemplate
	{
		private float m_DamageMod;

		private float m_ChargeTurns;

		private float m_AttackCount;

		private float m_StunChanceVsDebuff;

		private float m_StunDuration;

		private bool m_UseSkillAssetId;

		private bool m_StunVsWet;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			float value = 0f;
			base.Model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			base.Model.SkillParameters.TryGetValue("charge", out m_ChargeTurns);
			base.Model.SkillParameters.TryGetValue("attack_count", out m_AttackCount);
			base.Model.SkillParameters.TryGetValue("chance_to_stun_cursed", out m_StunChanceVsDebuff);
			base.Model.SkillParameters.TryGetValue("duration", out m_StunDuration);
			m_UseSkillAssetId = base.Model.SkillParameters.ContainsKey("use_skill_asset");
			ActionsAfterDamageDealt.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
			{
				ShareableActionPart(battle, source, target, false);
			});
		}

		public override void ShareableActionPart(BattleGameData battle, ICombatant source, ICombatant target, bool isShared)
		{
			if (target.CurrrentEffects.Values.Count((BattleEffectGameData eff) => eff.m_EffectType == SkillEffectTypes.Curse) > 0 && !(m_StunChanceVsDebuff / 100f < UnityEngine.Random.value))
			{
				DebugLog.Log("Stunned");
				string text = "Stun";
				List<float> list = new List<float>();
				list.Add(1f);
				List<float> values = list;
				BattleEffectGameData battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
						EffectType = BattleEffectType.Stun,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values,
						Duration = (int)m_StunDuration,
						EffectAssetId = ((!m_UseSkillAssetId) ? text : base.Model.Balancing.EffectIconAssetId),
						EffectAtlasId = ((!m_UseSkillAssetId) ? "Skills_Generic" : base.Model.Balancing.EffectIconAtlasId)
					}
				}, (int)m_StunDuration, battle, text, SkillEffectTypes.Curse, GetLocalizedName(), base.Model.SkillNameId);
				battleEffectGameData.AddEffect(true);
				battleEffectGameData.EffectRemovedAction = delegate(BattleEffectGameData e)
				{
					e.m_Target.CombatantView.PlayIdle();
				};
				battleEffectGameData.m_Target.CombatantView.PlayStunnedAnimation();
			}
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			dictionary.Add("{value_5}", string.Empty + m_ChargeTurns);
			dictionary.Add("{value_3}", string.Empty + m_StunChanceVsDebuff);
			dictionary.Add("{value_4}", string.Empty + m_AttackCount);
			dictionary.Add("{value_2}", string.Empty + m_StunDuration);
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
