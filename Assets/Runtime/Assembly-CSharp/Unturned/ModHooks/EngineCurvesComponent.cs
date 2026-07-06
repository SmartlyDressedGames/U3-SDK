////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EngineCurvesComponent : MonoBehaviour
	{
		[Tooltip("Maps normalized engine RPM to torque multiplier.\nIdle RPM is zero and max RPM is one on the X axis.")]
		public AnimationCurve engineRpmToTorqueCurve;

		[Tooltip("Maps normalized engine RPM difference to torque multiplier.\n-1 X is expected < actual, +1 X is expected > actual.")]
		public AnimationCurve engineRpmMismatchTorqueReductionCurve;

		public bool useEngineRpmMismatchTorqueReductionCurve;
	}
}
