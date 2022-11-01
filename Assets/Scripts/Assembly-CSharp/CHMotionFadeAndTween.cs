using System;
using System.Collections.Generic;
using UnityEngine;

public class CHMotionFadeAndTween : MonoBehaviour
{
	public enum TimingTypes
	{
		Duration,
		UnitsPerSecond
	}

	public Action Finished;

	public Action<bool> Paused;

	public Transform m_StartTransform;

	public Vector3 m_StartOffset;

	public Transform m_EndTransform;

	public Vector3 m_EndOffset;

	public TimingTypes m_Timing;

	public float m_DurationInSeconds;

	public float m_SpeedInUnitsSecond;

	public float m_FadeTimeInSeconds;

	public bool m_IsPlayOnStart;

	public float m_StartDelayInSeconds;

	public bool m_IsLooping;

	public AnimationCurve m_AnimationCurve;

	public List<AnimationCurveForAxis> m_AnimationCurvesPerAxis = new List<AnimationCurveForAxis>();

	private List<MeshRenderer> m_MeshRenderers;

	private float m_StartTimestamp;

	private Vector3 m_StartKey;

	private Vector3 m_EndKey;

	private float m_KeyDuration;

	private bool m_Inverted;

	private float m_DistanceToleranz;

	public bool IsPlaying { get; private set; }

	public bool IsPaused { get; private set; }

	public float MovementDuration
	{
		get
		{
			if (!IsPlaying)
			{
				return 0f;
			}
			return m_KeyDuration;
		}
	}

	public void Start()
	{
		m_MeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
		if (m_IsPlayOnStart)
		{
			Invoke("Play", m_StartDelayInSeconds);
		}
		if (m_FadeTimeInSeconds > 0f)
		{
			SetAlphaOfAllChilds(0f);
		}
	}

	public void Reset()
	{
		if (m_StartTransform != null)
		{
			base.transform.position = m_StartTransform.position + m_StartOffset;
		}
	}

	public float GetMoveDistance()
	{
		if (m_StartTransform != null)
		{
			m_StartKey = m_StartTransform.position + m_StartOffset;
		}
		else
		{
			m_StartKey = base.transform.position + m_StartOffset;
		}
		if (m_EndTransform != null)
		{
			m_EndKey = m_EndTransform.position + m_EndOffset;
		}
		else
		{
			m_EndKey = base.transform.position + m_EndOffset;
		}
		return Vector3.Distance(m_StartKey, m_EndKey);
	}

	public CHMotionFadeAndTween InvertCurves(bool invert)
	{
		m_Inverted = invert;
		return this;
	}

	public void Play()
	{
		if (m_StartTransform != null)
		{
			m_StartKey = m_StartTransform.position + m_StartOffset;
		}
		else
		{
			m_StartKey = base.transform.position + m_StartOffset;
		}
		if (m_EndTransform != null)
		{
			m_EndKey = m_EndTransform.position + m_EndOffset;
		}
		else
		{
			m_EndKey = base.transform.position + m_EndOffset;
		}
		if (m_Timing == TimingTypes.Duration)
		{
			m_KeyDuration = m_DurationInSeconds;
		}
		else if (m_Timing == TimingTypes.UnitsPerSecond)
		{
			m_KeyDuration = Vector3.Distance(m_StartKey, m_EndKey) / m_SpeedInUnitsSecond;
		}
		if (m_KeyDuration <= 0f)
		{
			DebugLog.Warn("Cannot start CHMotionTween on gameobject " + base.gameObject.name + ". The duration of the animation is " + m_KeyDuration + ". TimingType is set to " + m_Timing);
		}
		m_StartTimestamp = Time.time;
		IsPlaying = true;
		IsPaused = false;
	}

	public void Stop()
	{
		IsPlaying = false;
		IsPaused = false;
		base.transform.position = m_StartKey;
		if (Finished != null)
		{
			Finished();
		}
	}

	public void Resume()
	{
		IsPlaying = true;
		IsPaused = false;
		if (Paused != null)
		{
			Paused(false);
		}
	}

	public void Pause()
	{
		IsPlaying = false;
		IsPaused = true;
		if (Paused != null)
		{
			Paused(true);
		}
	}

	private void Update()
	{
		if (!IsPlaying)
		{
			return;
		}
		float num = (Time.time - m_StartTimestamp) / m_KeyDuration;
		if (num >= 1f)
		{
			if (m_IsLooping)
			{
				num = 0f;
				Play();
			}
			else
			{
				num = 1f;
				IsPlaying = false;
				if (Finished != null)
				{
					Finished();
				}
			}
		}
		Vector3 a = Vector3.one * num;
		if (m_AnimationCurve != null && m_AnimationCurvesPerAxis.Count <= 0)
		{
			a = Vector3.one * m_AnimationCurve.Evaluate(num);
		}
		else if (m_AnimationCurvesPerAxis.Count > 0)
		{
			a = Vector3.zero;
			foreach (AnimationCurveForAxis item in m_AnimationCurvesPerAxis)
			{
				switch (item.Axis)
				{
				case Axis.X:
					a.x = item.Curve.Evaluate(num);
					break;
				case Axis.Y:
					a.y = item.Curve.Evaluate(num);
					break;
				case Axis.Z:
					a.z = item.Curve.Evaluate(num);
					break;
				}
			}
		}
		if (m_Inverted)
		{
			a = new Vector3(a.y, a.x, a.z);
		}
		base.transform.position = m_StartKey + Vector3.Scale(a, m_EndKey - m_StartKey);
		if (m_FadeTimeInSeconds > 0f)
		{
			float alphaOfAllChilds = 1f;
			float num2 = Time.time - m_StartTimestamp;
			if (num2 <= m_FadeTimeInSeconds)
			{
				alphaOfAllChilds = num2 / m_FadeTimeInSeconds;
			}
			float num3 = m_KeyDuration - num2;
			if (num3 <= m_FadeTimeInSeconds)
			{
				alphaOfAllChilds = num3 / m_FadeTimeInSeconds;
			}
			SetAlphaOfAllChilds(alphaOfAllChilds);
		}
	}

	private void SetAlphaOfAllChilds(float m_FadeValue)
	{
		foreach (MeshRenderer meshRenderer in m_MeshRenderers)
		{
			Color color = meshRenderer.material.color;
			color.a = m_FadeValue;
			meshRenderer.material.color = color;
		}
	}
}
