using Godot;
using System.Collections.Generic;

public partial class MessagePanelUI : Control
{
	public static MessagePanelUI Instance { get; private set; }

	[Export] private VBoxContainer _messageContainer = null;
	[Export] private ScrollContainer _scrollContainer = null;
	private Queue<string> _messageQueue;

	public override void _Ready()
	{
		Instance = this;
		_messageQueue = new Queue<string>();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public async void EnqueueMessage(string message, Color messageColor)
	{
		LabelSettings messageLabelSettings = new()
        {
			FontColor = messageColor
		};

		Label messageLabel = new()
        {
			Text = message,
			LabelSettings = messageLabelSettings,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};

		_messageQueue.Enqueue(message);
		_messageContainer.AddChild(messageLabel);

		await ToSignal(_scrollContainer.GetVScrollBar(), Range.SignalName.Changed);
		_scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
	}
}
