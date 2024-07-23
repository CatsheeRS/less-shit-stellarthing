using Godot;
using System;

namespace stellarthing;

public partial class Player : CharacterBody3D {
	[Export]
	public int Speed { get; set; } = 400;
	[Export]
	public double RunningThingy { get; set; } = 1.25;
	[Export]
	public Node3D Model { get; set; }
	AnimationPlayer modelAnimator;
    //[Export]
    //public AnimatedSprite2D Sprite { get; set; }
    //[Export]
    //public Sprite2D Preview { get; set; }
    //[Export]
    //public RayCast2D PointingRaycast { get; set; }
    //[Export]
    //public Control PauseThingLmao { get; set; }

    public override void _Ready()
    {
        modelAnimator = Model.GetNode<AnimationPlayer>("animation_player");
    }

    public override void _PhysicsProcess(double delta)
	{
		// pausing :D
		//if (Input.IsActionJustPressed("pause")) {
		//	PauseThingLmao.Visible = true;
		//	GetTree().Paused = true;
		//}

		// movement
		float run = Input.IsActionPressed("run") ? (float)RunningThingy : 1.0f;
		
		Vector3 dir = Vector3.Zero;
		if (Input.IsActionPressed("move_left")) dir.X -= 1;
		if (Input.IsActionPressed("move_right")) dir.X += 1;
		if (Input.IsActionPressed("move_up")) dir.Z -= 1;
		if (Input.IsActionPressed("move_down")) dir.Z += 1;

		dir = dir.Normalized();
        Velocity = dir * Speed * new Vector3(run, 0, run);
		MoveAndSlide();

		if (!dir.IsZeroApprox()) {
			Model.Basis = Basis.LookingAt(dir);
		}
	}
}
