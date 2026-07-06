////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Thanks to Glenn Fiedler for this RK4 implementation article:
	/// https://gafferongames.com/post/integration_basics/
	/// </summary>
	[System.Serializable]
	public struct Rk4Spring3
	{
		public Vector3 currentPosition;
		public Vector3 targetPosition;

		/// <summary>
		/// Higher values return to the target position faster.
		/// </summary>
		public float stiffness;
		/// <summary>
		/// Higher values reduce bounciness and settle at the target position faster.
		/// e.g. a value of zero will bounce back and forth for a long time (indefinitely?)
		/// </summary>
		public float damping;

		public Rk4Spring3(float stiffness, float damping)
		{
			currentPosition = default;
			targetPosition = default;
			this.stiffness = stiffness;
			this.damping = damping;
			currentVelocity = default;
		}

		public void Update(float deltaTime)
		{
			while (deltaTime > Rk4Spring.MAX_TIMESTEP)
			{
				PrivateUpdate(Rk4Spring.MAX_TIMESTEP);
				deltaTime -= Rk4Spring.MAX_TIMESTEP;
			}

			if (deltaTime > 0.0f)
			{
				PrivateUpdate(deltaTime);
			}
		}

		private void PrivateUpdate(float deltaTime)
		{
			Rk4Derivative3 a, b, c, d;
			a = Evaluate(0.0f, new Rk4Derivative3());
			b = Evaluate(deltaTime * 0.5f, a);
			c = Evaluate(deltaTime * 0.5f, b);
			d = Evaluate(deltaTime, c);

			Vector3 velocity = 1.0f / 6.0f *
				(a.velocity + (2.0f * (b.velocity + c.velocity)) + d.velocity);

			Vector3 acceleration = 1.0f / 6.0f *
				(a.acceleration + (2.0f * (b.acceleration + c.acceleration)) + d.acceleration);

			currentPosition += velocity * deltaTime;
			currentVelocity += acceleration * deltaTime;
		}

		private Rk4Derivative3 Evaluate(float deltaTime, Rk4Derivative3 initialDerivative)
		{
			Vector3 position = currentPosition + (initialDerivative.velocity * deltaTime);
			Vector3 velocity = currentVelocity + (initialDerivative.acceleration * deltaTime);

			Rk4Derivative3 output;
			output.velocity = velocity;
			output.acceleration = (stiffness * (targetPosition - position)) - (damping * velocity);
			return output;
		}

		private Vector3 currentVelocity;

		private struct Rk4Derivative3
		{
			public Vector3 velocity;
			public Vector3 acceleration;
		}
	}
}
