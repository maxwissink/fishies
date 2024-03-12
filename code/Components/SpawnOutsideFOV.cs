using Sandbox;
using System;
using static Sandbox.PhysicsContact;

public sealed class SpawnOutsideFOV : Component
{
	/// <summary>
	/// Drag the first type of unit you want to spawn here
	/// </summary>
	[Property]
	[Category("Spawnlist")]
	public GameObject Fish1 { get; set; }
	[Property]
	[Category("Player")]
	public GameObject Player { get; set; }

	private Player _player;

	protected override void OnStart()
	{
		_player = Player.Components.Get<Player>();
	}

	protected override void OnUpdate()
	{
		if ( GameObject.Children.Count() >= 35 ) { return; }

		GameObject SpawnThis = Fish1.Clone( RandomOutsideFOV() );
		GameObject.Children.Add(SpawnThis);
		
		UnitInfo SpawnedInfo = SpawnThis.Components.Get<UnitInfo>();
		//Log.Info( _player.Size + " / " + (1 + ((_player.Size - 1) / 2) ));
		SpawnThis.Components.Get<Fish>().Grow((float)(_player.Size - 1)/2);
		SpawnedInfo.Health = Player.Children.FirstOrDefault().Components.Get<UnitInfo>().Health / 2;

		SpawnedInfo.OnDeath += RemoveFromList;
	}

	public void RemoveFromList(GameObject gameObject)
	{
		GameObject.Children.Remove(gameObject);
	}

	public Vector3 RandomOutsideFOV()
	{
		Vector3 random = new Random().VectorInCircle(1000);
		Vector3 target = random;
		// Get a direction vector from the scene camera to target
		Vector3 direction = (target - Scene.Camera.Transform.Position).Normal;
		// Will be a number from -1 to 1
		float dot = Scene.Camera.Transform.Rotation.Forward.Dot( direction );
		double fovDot = (Math.Cos( ((Scene.Camera.FieldOfView / 2f) * Math.PI) / 180f ) * 100f ) / 100f;
		if ( dot < fovDot ) {
			return target;
		}
		return RandomOutsideFOV();
		

	}
}
