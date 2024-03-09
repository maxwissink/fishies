using Sandbox;

public sealed class SpawnOutsideFOV : Component
{
	/// <summary>
	/// Drag the first type of unit you want to spawn here
	/// </summary>
	[Property]
	[Category("Spawnlist")]
	public GameObject Fish1 { get; set; }
	protected override void OnUpdate()
	{
		
	}
}
