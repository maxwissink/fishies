using Sandbox.Services;
using Sandbox.Utility;
using System;
using System.Timers;

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
	[Category("Components")]
	public GameObject particleEffect { get; set; }
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 400f, 1f )]
	public float WalkSpeed { get; set; } = 50f;
	[Property]
	[Category("Stats")]
	[Range(0.1f, 1f, 0.1f)]
	public float Randomness { get; set; } = 1f;
	[Property]
	[Category( "Stats" )]
	[Range( 1f, 50f, 1f )]
	public float BiteRange { get; set; } = 25f;
	/// <summary>
	/// The damage every bite deals
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0.5f, 5f, 0.5f )]
	public float BiteDamage { get; set; } = 1f;
	[Property]
	[Category( "Components" )]
	public Vector3 MouthPosition { get; set; }
	Vector3 _MoutPosReal = new Vector3( 0f, 0f, 0f );
	[Property]
	[Category( "Stats" )]
	[Range( 0.1f, 3f, 0.1f )]
	public float BiteDelay { get; set; } = 1f;
	[Property]
	[Category( "Stats" )]
	public float Size { get; set; } = 1f;
	[Property]
	[Category( "Stats" )]
	public int SmellRange { get; set; } = 50;

	public Vector3 MouthWorldPosition => Transform.Local.PointToWorld( MouthPosition );
	public int Kills { get; set; } = 0;

	TimeSince _lastBite;
	Vector3 _lastPos = new Vector3( 0f, 0f, 0f );
	Vector3 _velocity = new Vector3( 0f, 0f, 0f );
	float _maxHealth;

	Vector3 _travelDirection = new Vector3( new Random().Float( -50f, 50f ), new Random().Float( -50f, 50f ), 0f );


	protected override void OnStart()
	{
		_MoutPosReal = MouthPosition;
		base.OnStart();
	}

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

		if ( _lastBite > BiteDelay ) Bite();
		SmellSmaller();

		_velocity = Transform.Position - _lastPos;
		_lastPos = Transform.Position;
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

	public void SmellSmaller()
	{
		var biteTrace = Scene.Trace
			.Sphere( SmellRange * Size, Transform.Position, Transform.Position )
			.WithoutTags( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( biteTrace.Hit )
		{
			if ( biteTrace.GameObject.Components.TryGet<UnitInfo>( out var unitInfo ) )
			{
				if ( unitInfo.Components.Get<Fish>()?.Size > Size ) return; //check if target is bigger and if bigger dont bite
				if ( unitInfo.GameObject.Parent.Components.Get<Player>()?.Size > Size ) return; //check if target is bigger and if bigger dont bite
				_travelDirection = unitInfo.Transform.Position - Transform.Position;
			}
		}
	}

	protected override void DrawGizmos()
	{

		Gizmo.Draw.LineSphere( MouthPosition, BiteRange );
		Gizmo.Draw.LineSphere( new Vector3(0,0,0), SmellRange );

		if ( !Gizmo.IsSelected ) return;
		base.DrawGizmos();

		Gizmo.Draw.LineSphere( MouthPosition, BiteRange );
	}

	public void Bite()
	{
		var biteTrace = Scene.Trace
			.Sphere( BiteRange * Size, MouthWorldPosition, MouthWorldPosition )
			.WithoutTags( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( biteTrace.Hit )
		{
			if ( biteTrace.GameObject.Components.TryGet<UnitInfo>( out var unitInfo ) )
			{
				if (unitInfo.Components.Get<Fish>()?.Size > Size) return; //check if target is bigger and if bigger dont bite
				if ( unitInfo.GameObject.Parent.Components.Get<Player>()?.Size > Size ) return; //check if target is bigger and if bigger dont bite
				unitInfo.Damage( BiteDamage );
				BleedTarget( unitInfo.Transform.Position );
				if ( unitInfo.IsDead )
				{
					Kills += unitInfo.Points;
					UnitInfo.Damage( -unitInfo.Points );
					if ( _maxHealth < UnitInfo.Health ) _maxHealth = UnitInfo.Health;
					if ( UnitInfo.Health == _maxHealth ) Grow( unitInfo.Components.Get<Fish>()?.Size/10 );
				}
				_lastBite = 0f;
			}
		}

	}

	public void BleedTarget(Vector3 position)
	{
		particleEffect.Clone(position).Transform.Rotation = new Angles( Vector3.GetAngle(position, Transform.Position),0, 0);
	}

	public void Grow( float? amount = 0.1f )
	{
		if ( !amount.HasValue ) return;
		Size += (float)amount;
		BiteDamage += (float)amount;
		Transform.Scale = Size;
	}

	public void TurnAround()
	{
		if ( _velocity.y > 0f )
		{
			MouthPosition = new Vector3( -_MoutPosReal.x, _MoutPosReal.y, _MoutPosReal.z );
		}
		else if ( _velocity.y < 0f )
		{
			MouthPosition = new Vector3( _MoutPosReal.x, _MoutPosReal.y, _MoutPosReal.z );
		}
	}
}
