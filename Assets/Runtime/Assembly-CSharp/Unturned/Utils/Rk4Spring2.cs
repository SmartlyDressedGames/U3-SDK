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
	public struct Rk4Spring2
	{
		public Vector2 currentPosition;
		public Vector2 targetPosition;

		/// <summary>
		/// Higher values return to the target position faster.
		/// </summary>
		public float stiffness;
		/// <summary>
		/// Higher values reduce bounciness and settle at the target position faster.
		/// e.g. a value of zero will bounce back and forth for a long time (indefinitely?)
		/// </summary>
		public float damping;

		public Rk4Spring2(float stiffness, float damping)
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
			Rk4Derivative2 a, b, c, d;
			a = Evaluate(0.0f, new Rk4Derivative2());
			b = Evaluate(deltaTime * 0.5f, a);
			c = Evaluate(deltaTime * 0.5f, b);
			d = Evaluate(deltaTime, c);

			Vector2 velocity = 1.0f / 6.0f *
				(a.velocity + (2.0f * (b.velocity + c.velocity)) + d.velocity);

			Vector2 acceleration = 1.0f / 6.0f *
				(a.acceleration + (2.0f * (b.acceleration + c.acceleration)) + d.acceleration);

			currentPosition += velocity * deltaTime;
			currentVelocity += acceleration * deltaTime;
		}

		private Rk4Derivative2 Evaluate(float deltaTime, Rk4Derivative2 initialDerivative)
		{
			Vector2 position = currentPosition + (initialDerivative.velocity * deltaTime);
			Vector2 velocity = currentVelocity + (initialDerivative.acceleration * deltaTime);

			Rk4Derivative2 output;
			output.velocity = velocity;
			output.acceleration = (stiffness * (targetPosition - position)) - (damping * velocity);
			return output;
		}

		private Vector2 currentVelocity;

		private struct Rk4Derivative2
		{
			public Vector2 velocity;
			public Vector2 acceleration;
		}
	}
}
