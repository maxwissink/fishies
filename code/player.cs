using Sandbox.Utility;
using System;

public sealed class Player : Component
{
    /// <summary>
    /// Drag the camera componend in here
    /// </summary>
    [Property]
    [Category("Components")]
    public GameObject Camera {get;set;}
    /// <summary>
    /// Drag the CharacterController in here
    /// </summary>
    [Property]
    [Category("Components")]
    public CharacterController Controller {get;set;}
    /// <summary>
    /// Sets the default walk speed (units per second)
    /// </summary>
    [Property]
    [Category("Stats")]
    [Range( 0f, 400f, 1f)]
    public float WalkSpeed {get;set;} = 120f;
    /// <summary>
    /// Sets the default running speed (units per second)
    /// </summary>
    [Property]
    [Category("Stats")]
    [Range( 0f, 800f, 1f)]
    public float RunSpeed {get;set;} = 250f;
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
	Vector3 _MoutPosReal = new Vector3(0f,0f,0f);
	[Property]
	[Category( "Stats" )]
	[Range( 0.1f, 3f, 0.1f )]
	public float BiteDelay { get; set; } = 1f;
	[Property]
	[Category( "Stats" )]
	public float Size { get; set; } = 1f;

	public Vector3 MouthWorldPosition => Transform.Local.PointToWorld( MouthPosition );
	TimeSince _lastBite;

	Vector3 _lastPos = new Vector3( 0f, 0f, 0f );
	Vector3 _velocity = new Vector3( 0f, 0f, 0f );



	protected override void OnStart()
	{
		_MoutPosReal = MouthPosition;
		base.OnStart();
	}

	protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if ( Controller == null ) return;

        var wishVelocity = Input.AnalogMove.Normal * WalkSpeed;

        Controller.Accelerate( wishVelocity );

		Controller.ApplyFriction( 2f );

		Controller.Move();
		if ( _lastBite > BiteDelay ) Bite();

		_velocity = Transform.Position - _lastPos;
		_lastPos = Transform.Position;


		TurnAround();

	}

	protected override void DrawGizmos()
	{

		Gizmo.Draw.LineSphere( MouthPosition, BiteRange );

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

				Log.Info( "Biting fish!" );
				unitInfo.Damage( BiteDamage );
				if ( unitInfo.IsDead )
				{
					Grow( unitInfo.MaxHealth / 10 );
				}
				_lastBite = 0f;
			}
		}

	}

	public void Grow( float amount = 0.1f )
	{
		Size += amount;
		BiteDamage += amount;
		Transform.Scale = Size;
	}

	public void TurnAround()
	{
		if ( _velocity.y > 0f )
		{
			MouthPosition = new Vector3( _MoutPosReal.x, -_MoutPosReal.y, _MoutPosReal.z);
		}
		else if ( _velocity.y < 0f )
		{
			MouthPosition = new Vector3( _MoutPosReal.x, _MoutPosReal.y, _MoutPosReal.z );
		}
	}

}
