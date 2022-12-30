using StereoKit;

using System;
using System.Collections.Generic;

using VisualScript;

namespace VisualScript;

public sealed class Player
{

	public Player(App _app) {
		App = _app;
		PlayerPos = Matrix.Identity;
		scriptEditor = new(this);
	}

	public readonly App App;
	public readonly ScriptEditor scriptEditor;

	public Matrix PlayerPos;

	public float PlayerSpeed = 5;
	public float RotateSpeed = 40;

	public void MoveStep() {
		if (Platform.KeyboardVisible) {
			return;
		}
		var walkForwaredBack = Input.Controller(Handed.Right).stick.y;
		var walkLeftRight = Input.Controller(Handed.Right).stick.x;
		var rotate = Input.Controller(Handed.Left).stick.x;
		var upDown = 0f;
		if (Input.Key(Key.E).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			upDown = 1;
		}
		if (Input.Key(Key.Q).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			upDown = -1;
		}
		if (Input.Key(Key.W).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			walkForwaredBack = 1;
		}
		if (Input.Key(Key.A).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			walkLeftRight = -1;
		}
		if (Input.Key(Key.D).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			walkLeftRight = 1;
		}
		if (Input.Key(Key.S).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			walkForwaredBack = -1;
		}
		if (Input.Key(Key.X).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			rotate = 1;
		}
		if (Input.Key(Key.Z).IsActive() && !Input.Key(Key.Shift).IsActive()) {
			rotate = -1;
		}
		rotate *= RotateSpeed;
		PlayerPos = Matrix.TR(new Vec3(walkLeftRight, upDown, -walkForwaredBack) * Time.Stepf * PlayerSpeed, Quat.FromAngles(0, -rotate * Time.Stepf, 0)) * PlayerPos;
		Renderer.CameraRoot = PlayerPos;
	}

	public void Step() {
		MoveStep();
		scriptEditor.Step();
	}
}