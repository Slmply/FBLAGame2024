using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody2D
{
	[Export]
	public float WalkSpeed = 350.0f;
	[Export]
	public float AccelSpeed = 100.0f;
	[Export]
	public float DecelSpeed = 100.0f;
	[Export]
	public float AccelMultiplier = 30.0f;
	[Export]
	public float DodgeSpeed = 800.0f;
	public Timer dodgeTimer = null;
	public AnimationPlayer anmPlayer;
	protected Area2D pickupArea;
	protected Throwable heldItem;
	[Export]
	public Node2D holdPoint;
	public Boolean stunned = false;
	protected Timer iTimer = null;

	[Signal]
	public delegate void onHitEventHandler();

	public override void _Ready()
	{
		dodgeTimer = (Timer)GetNode("DodgeTimer");
		anmPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		pickupArea = GetNode<Area2D>("PickupArea");
		iTimer = GetNode<Timer>("InvincibilityTimer");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Interact") && dodgeTimer.IsStopped())
		{


			if (heldItem == null)
			{
				Node2D[] items = pickupArea.GetOverlappingBodies().ToArray<Node2D>();

				foreach (Node2D n in items)
				{

					if (n is Throwable)
					{
						Throwable i = (Throwable)n;
							if (i.holder == null) {
								heldItem = i;
							
							i.holder = this;
							break;
						}
						
					}
				}
			}
			else
			{
				heldItem.lob(Velocity.Normalized());
				heldItem = null;
			}



		}
	}

	public void hit() {
		if (!stunned && dodgeTimer.IsStopped() && iTimer.IsStopped()) {
			stunned = true;
			anmPlayer.Play("Hit");
			EmitSignal(SignalName.onHit);

			if (heldItem != null) {
				heldItem.drop(Velocity.Normalized());
				heldItem = null;
			}
		}
	}

	public void animFinished(string anim) {
		if (anim == "Hit") {
			stunned = false;
			iTimer.Start();
		}
	}



	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.

		if (stunned) {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, DecelSpeed * (float)delta * AccelMultiplier);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, DecelSpeed * (float)delta * AccelMultiplier);

			Velocity = velocity;

			GetNode<Sprite2D>("Sprite2D").FlipH = (Velocity.X == 0) ? GetNode<Sprite2D>("Sprite2D").FlipH : (Velocity.X > 0) ? false : true;

			MoveAndSlide();
			return;
		}

		Vector2 direction = Input.GetVector("Move_Left", "Move_Right", "Move_Up", "Move_Down");
		direction = direction.Normalized();
		if (dodgeTimer.IsStopped())
		{
			if (direction != Vector2.Zero)
			{
				velocity.X = Mathf.MoveToward(Velocity.X, direction.X * WalkSpeed, AccelSpeed * (float)delta * AccelMultiplier);
				velocity.Y = Mathf.MoveToward(Velocity.Y, direction.Y * WalkSpeed, AccelSpeed * (float)delta * AccelMultiplier);
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, DecelSpeed * (float)delta * AccelMultiplier);
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, DecelSpeed * (float)delta * AccelMultiplier);
			}
		}
		else
		{
			if (direction != Vector2.Zero)
			{
				velocity.X = Mathf.MoveToward(Velocity.X, direction.X * WalkSpeed, AccelSpeed * (float)delta * AccelMultiplier / 2.0f);
				velocity.Y = Mathf.MoveToward(Velocity.Y, direction.Y * WalkSpeed, AccelSpeed * (float)delta * AccelMultiplier / 2.0f);
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, DecelSpeed * (float)delta * AccelMultiplier / 2.0f);
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, DecelSpeed * (float)delta * AccelMultiplier / 2.0f);
			}
		}


		if (Input.IsActionJustPressed("Dodge") && dodgeTimer.IsStopped() && Velocity.Length() >= 0.5f)
		{
			velocity.X = direction.X * DodgeSpeed;
			velocity.Y = direction.Y * DodgeSpeed;
			dodgeTimer.Start();
			GetNode<AudioStreamPlayer>("DodgeSound").Play();

			if (heldItem != null) {
				heldItem.drop(Velocity.Normalized());
				heldItem = null;
			}
		}



		if (!dodgeTimer.IsStopped() || anmPlayer.CurrentAnimation == "Roll")
		{
			anmPlayer.Play("Roll");
		}
		else if (Velocity.Length() > 0.5)
		{
			anmPlayer.Play("Run");
		}
		else
		{
			anmPlayer.Play("Idle");
		}

		Velocity = velocity;

		GetNode<Sprite2D>("Sprite2D").FlipH = (Velocity.X == 0) ? GetNode<Sprite2D>("Sprite2D").FlipH : (Velocity.X > 0) ? false : true;

		MoveAndSlide();
	}
}
