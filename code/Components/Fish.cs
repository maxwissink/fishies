using Sandbox.Utility;
using System;

public sealed class Fish : Component
{
	/// <summary>
	/// Drag the UnitInfo in here
	/// </summary>
	[Property]
	[Category("Components")]
	public UnitInfo UnitInfo { get; set; }
	/// <summary>
	/// Drag the CharacterController in here
	/// </summary>
	[Property]
	[Category( "Components" )]
	public CharacterController Controller { get; set; }
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 400f, 1f )]
	public float WalkSpeed { get; set; } = 50f;
	[Property]
	[Category("Stats")]
	[Range(0.1f, 1f, 0.1f)]
	public float Randomness { get; set; } = 1f;

	Vector3 _travelDirection = new Vector3( new Random().Float( -50f, 50f ), new Random().Float( -50f, 50f ), 0f );

	protected override void OnFixedUpdate()
	{

		if ( Controller == null ) return;

		if (GameObject.Transform.Position.Distance(new Vector3(0,0,0)) < 1000) {
			Controller.Accelerate( SetRandomness() );
		}else
		{
			Controller.Accelerate( ReturnToCenter() );
		}

		Controller.ApplyFriction( 0.5f );

		Controller.Move();

		GameObject.Transform.Position = new Vector3( Transform.Position.x, Transform.Position.y, 0 );

		GameObject.Transform.LocalScale = 1f + (UnitInfo.MaxHealth/5);
	}

	public Vector3 SetRandomness()
	{
		if ( new Random().Float( 0f, 100f ) < Randomness)
		{
			_travelDirection = new Vector3( new Random().Float( -WalkSpeed, WalkSpeed ), new Random().Float( -WalkSpeed, WalkSpeed ), 0f );

		}
		return _travelDirection;
	}

	public Vector3 ReturnToCenter()
	{
		_travelDirection = -GameObject.Transform.Position.ClampLength(50);

		return _travelDirection;
	}
}
