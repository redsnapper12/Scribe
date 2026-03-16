using Godot;
using System;
using System.Diagnostics;

public partial class CameraController : Node2D
{
	[Export] private Camera2D Camera { get; set; } = null;

	[ExportGroup("Camera Properties")]
	[Export] private float CameraPanSpeed { get; set; } = 100.0f;
	[Export] private float CameraZoomSpeed { get; set; } = 100.0f;
	[Export] private Vector2 MaxZoom { get; set; } = new Vector2(1.5f, 1.5f);
	[Export] private Vector2 MinZoom { get; set; } = new Vector2(0.5f, 0.5f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 cameraPanInput = Input.GetVector("pan_cam_left", "pan_cam_right", "pan_cam_up", "pan_cam_down");
		float cameraZoomInput = Input.GetAxis("zoom_cam_out", "zoom_cam_in");

		Camera.Position += cameraPanInput * CameraPanSpeed * (float)delta;
		LerpCameraZoom(cameraZoomInput, (float)delta);
	}

	private void LerpCameraZoom(float input, float delta)
	{
		if (input != 0)
		{
			Camera.Zoom = (input > 0) ? 
				Camera.Zoom.Lerp(MaxZoom, CameraZoomSpeed * delta) :   // Zoom in toward MaxZoom (1.5)
				Camera.Zoom.Lerp(MinZoom, CameraZoomSpeed * delta);     // Zoom out toward MinZoom (0.5)
		}
	}
}
