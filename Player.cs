using Godot;
using System;
using System.Linq.Expressions;

//TODO: Deactivate stair checkers in the air!!

public enum PlayerState
{
	Idle,
	Walking,
	Jumping,
	Falling
}

public partial class Player : Actor
{
	
	#region References

	[ExportCategory("References")]

	#endregion

	#region Variables

	[Export]
	public PlayerState currentState = PlayerState.Idle;

	[ExportCategory("Movement")]
	[ExportGroup("Horizontal Movement")]

	[Export]
	public float movementSpeed = 20f;
	[Export]
	public float groundTraction = 1f;
	[Export]
	public float airTraction = 0.3f;
	[Export]
	public float highSpeedTractionMultiplier = 0.1f;

	[ExportGroup("Vertical Movement")]

	[ExportSubgroup("Jumping")]
	[Export]
	public int maxJumps = 2;
	int jumps = 2;
	[Export]
	public float jumpHeight = 50f;
	[Export]
	public float maximumCoyoteTime = 0.2f;
	[Export]
	public float maximumJumpBuffer = 0.2f;
	float coyoteTime = 0f;
	float jumpBuffer = 0f;

    #endregion

    public override void _Ready()
    {

		base._Ready();

		coyoteTime = 0f;
		jumpBuffer = 0f;
		Velocity = Vector3.Zero;

    }

    public override void _Process(double delta)
    {

		coyoteTime = Mathf.Max(0f, coyoteTime - (float)delta);
		jumpBuffer = Mathf.Max(0f, jumpBuffer - (float)delta);

		if (Input.IsActionJustPressed("Jump")) jumpBuffer = maximumJumpBuffer;

    }

    public override void _PhysicsProcess(double delta)
    {
        
		Vector3 velocity = Velocity;
		Vector2 inputVector = Input.GetVector("Left", "Right", "Forward", "Back").Normalized();

		Vector3 movementDirection = inputVector.X * Transform.Basis.X + inputVector.Y * Transform.Basis.Z;

		switch(currentState) {

			case PlayerState.Idle:
			{

				jumps = maxJumps;

				if (IsOnFloor()) {

					coyoteTime = maximumCoyoteTime;

				}

				if (coyoteTime <= 0f) {

					currentState = PlayerState.Falling;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);

					if (jumpBuffer > 0f) {

						jumps--;
						velocity.Y = jumpHeight;
						jumpBuffer = 0f;
						coyoteTime = 0f;
						currentState = PlayerState.Jumping;

					} else if (inputVector != Vector2.Zero) {

						currentState = PlayerState.Walking;

					} else {

						velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.1f * groundTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					}

				}				
				
				break;
			}
			case PlayerState.Walking:
			{

				jumps = maxJumps;

				if (IsOnFloor()) coyoteTime = maximumCoyoteTime;

				if (coyoteTime <= 0f) {

					currentState = PlayerState.Falling;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);

					if (jumpBuffer > 0f) {

						jumps--;
						velocity.Y = jumpHeight;
						jumpBuffer = 0f;
						coyoteTime = 0f;
						currentState = PlayerState.Jumping;
					
					} else if (inputVector == Vector2.Zero) {

						currentState = PlayerState.Idle;

					} else {

						velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * groundTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					}
				} 

				break;
			}
			case PlayerState.Jumping:
			{

				velocity.Y += gravity * (float)delta;

				if (IsOnFloor()) {

					currentState = PlayerState.Idle;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);
					if (jumps > 0 && jumpBuffer > 0f) {
						
						jumps--;
						Vector3 maxVel = movementDirection.Normalized() * Mathf.Max(flatVel.Length(), movementSpeed);
						velocity = new(maxVel.X, jumpHeight * 0.7f, maxVel.Z);
						jumpBuffer = 0f;
						currentState = PlayerState.Jumping;

					};

					velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * airTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					if (velocity.Y <= 0f)
					{

						currentState = PlayerState.Falling;

					} else if (Input.IsActionJustReleased("Jump")) {


						velocity.Y *= 0.5f;
						currentState = PlayerState.Falling;

					}

				}

				break;
			}
			case PlayerState.Falling:
			{

				velocity.Y += gravity * (float)delta;

				if (IsOnFloor()) {

					currentState = PlayerState.Idle;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);
					if (jumps > 0 && jumpBuffer > 0f) {
						
						jumps--;
						Vector3 maxVel = movementDirection.Normalized() * Mathf.Max(flatVel.Length(), movementSpeed);
						velocity = new(maxVel.X, jumpHeight * 0.7f, maxVel.Z);
						jumpBuffer = 0f;
						currentState = PlayerState.Jumping;

					};

					velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * airTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

				}

				break;
			}

			default:
			{
				GD.PrintErr("Invalid player state!");
				currentState = PlayerState.Idle;
				break;
			}

		}

		Velocity = velocity;

		MoveAndSlide();

    }

}
