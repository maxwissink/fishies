using Sandbox;
using System;
using System.Drawing;
public enum UnitType
{
	[Icon( "do_not_disturb_on" )]
	None,
	[Icon( "emoji_people" )]
	Player,
	[Icon( "set_meal" )]
	Fish
}

public sealed class UnitInfo : Component
{
	/// <summary>
	/// Set the team of the object
	/// </summary>
	[Property]
	public UnitType Team { get; set; }
	/// <summary>
	/// Sets the maximum amount of hp the unit has
	/// </summary>
	[Property]
	[Range( 0.1f, 10f, 0.1f )]
	public float MaxHealth { get; set; } = 5f;

	public float Health { get; private set; }
	public bool IsDead { get; set; } = false;

	public event Action<float> OnDamage;
	public event Action<GameObject> OnDeath;

	Vector3 _lastPos = new Vector3( 0f, 0f, 0f );
	Vector3 _velocity = new Vector3( 0f, 0f, 0f );

	Angles _finalRotation = new Angles();

	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		_velocity = Transform.Position - _lastPos;
		_lastPos = Transform.Position;

		
		TurnAround();
	}

	protected override void OnStart()
	{
		Health = MaxHealth;
	}

	/// <summary>
	/// Damage unit, kills on <= 0, heals on negative input
	/// </summary>
	/// <param name="damage"></param>
	public void Damage(float damage)
	{
		if ( IsDead ) { return; }

		Health = Math.Clamp( Health - damage, 0f, MaxHealth );

		OnDamage?.Invoke(damage);

		if (Health <= 0f ) { Kill(); }
	}

	/// <summary>
	/// Kills the unit
	/// </summary>
	public void Kill()
	{
		Health = 0f;
		IsDead = true;

		OnDeath?.Invoke(GameObject);

		GameObject.DestroyImmediate();
	}
	
	public void TurnAround()
	{
			if ( (_velocity.y > 0f && Team == UnitType.Player) || (_velocity.y < 0f && Team == UnitType.Fish) )
			{
				_finalRotation = new Angles( 180, 90, 90 );
			}
			else if ( (_velocity.y < 0f && Team == UnitType.Player) || (_velocity.y > 0f && Team == UnitType.Fish) )
			{
				_finalRotation = new Angles( 0, 90, 90 );
			}

		if ((_finalRotation.pitch > 90 && Team == UnitType.Fish) || (_finalRotation.pitch < 90 && Team == UnitType.Player) )
		{
			_finalRotation.yaw = 90 + (_velocity.x * 5);
		}
		else
		{
			_finalRotation.yaw = 90 - (_velocity.x * 5);
		}

		Transform.Rotation = _finalRotation;
	}

	
}
