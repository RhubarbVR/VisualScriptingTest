using Microsoft.Maui.Devices.Sensors;

using StereoKit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VisualScript
{
	public struct Impluses
	{
	}

	public abstract class NodeBase : PosedWorldObject
	{
		public virtual bool IsInputEvent => false;
		public virtual bool IsOutputEvent => false;

		protected override void Init() {
			base.Init();
			if (!IsInputEvent) {
				nodeInputs.Add(new NodeInput(this) {
					Name = "FlowIN",
					ButtonType = typeof(Impluses),
					Flow = true
				});
			}
			if (!IsOutputEvent) {
				nodeOutputs.Add(new NodeOutput<Impluses>(this) {
					Name = "FlowOUT",
					ButtonType = typeof(Impluses),
					Flow = true
				});
			}
		}

		public abstract class NodeButton : IDisposable
		{
			protected NodeBase _node;

			public bool NodeHasFlow => _node.HasFlow;

			public bool Flow { get; set; }

			public string Name { get; set; }
			public Type ButtonType { get; set; }

			public bool IsConnectedTo => _connectedTo is not null;

			private NodeButton _connectedTo;
			public Vec3 lastPos = Vec3.Zero;

			protected abstract bool AllowConnect(NodeButton nodeButton);

			private void Clear() {
				_connectedTo = null;
			}

			public NodeButton ConnectedTo
			{
				get => _connectedTo;
				set {
					if (!AllowConnect(value)) {
						return;
					}
					if (value is not null) {
						value.OnDispos += Clear;
					}
					if (_connectedTo is not null) {
						_connectedTo.OnDispos -= Clear;
					}
					_connectedTo = value;
					if(value is null) {
						return;
					}
					if (value.ConnectedTo != this) {
						value.ConnectedTo = this;
					}
				}
			}
			public abstract void InvokeNode();


			public void InvokeFlow() {
				_connectedTo?.InvokeNode();
			}

			public event Action OnDispos;

			public void Dispose() {
				OnDispos?.Invoke();
			}

			public abstract void Step(int yPos);
		}

		public abstract class NodeOutputBase : NodeButton
		{
			public NodeOutputBase(NodeBase app) {
				_node = app;
			}

			public override void Step(int yPos) {
				UI.PushTint(ButtonType.GetTypeColor());
				var location = new Vec3(-0.1f, -yPos * 0.05f, 0);
				lastPos = (Matrix.T(location) * _node.Pos.ToMatrix()).Translation;
				if (UI.ButtonAt("", location, new Vec2(0.04f))) {
					if (_node.App.Player.scriptEditor.NodeButton == this) {
						_node.App.Player.scriptEditor.IsInput = false;
						_node.App.Player.scriptEditor.TypeTargetReadType = null;
						_node.App.Player.scriptEditor.NodeButton = null;
					}
					else if (_node.App.Player.scriptEditor.IsInput) {
						if (_node.App.Player.scriptEditor.NodeButton is null) {
							_node.App.Player.scriptEditor.IsInput = false;
							_node.App.Player.scriptEditor.TypeTargetReadType = ButtonType;
							_node.App.Player.scriptEditor.NodeButton = this;
						}
						else {
							if (Flow) {
								if (ConnectedTo?.ConnectedTo is not null) {
									ConnectedTo.ConnectedTo = null;
								}
							}
							_node.App.Player.scriptEditor.NodeButton.ConnectedTo = _node.App.Player.scriptEditor.NodeButton.ConnectedTo == this ? null : (NodeButton)this;
							_node.App.Player.scriptEditor.IsInput = true;
							_node.App.Player.scriptEditor.TypeTargetReadType = null;
							_node.App.Player.scriptEditor.NodeButton = null;
						}
					}
					else {
						_node.App.Player.scriptEditor.IsInput = false;
						_node.App.Player.scriptEditor.TypeTargetReadType = ButtonType;
						_node.App.Player.scriptEditor.NodeButton = this;
					}
				}
				UI.PopTint();
			}

			protected override bool AllowConnect(NodeButton nodeButton) {
				if(nodeButton is null) {
					return true;
				}
				if (nodeButton.Flow != Flow) {
					return false;
				}
				if (nodeButton.ButtonType != ButtonType) {
					return false;
				}
				if (typeof(NodeOutput<>).IsAssignableFrom(nodeButton.GetType())) {
					return false;
				}
				return true;
			}

			public override void InvokeNode() {
				_node?.InvokeNode();
			}
		}

		public sealed class NodeOutput<T> : NodeOutputBase
		{
			public NodeOutput(NodeBase app) : base(app) {
			}

			public T Value { get; set; }

		}

		public sealed class NodeInput : NodeButton
		{
			public NodeInput(NodeBase app) {
				_node = app;
			}

			public bool TargetNodeHasFlow => ConnectedTo?.NodeHasFlow ?? true;

			public T GetValue<T>() {
				return ConnectedTo is NodeOutput<T> value ? value.Value : default;
			}

			protected override bool AllowConnect(NodeButton nodeButton) {
				if (nodeButton is null) {
					return true;
				}
				if (nodeButton.Flow != Flow) {
					return false;
				}
				if (nodeButton.ButtonType != ButtonType) {
					return false;
				}
				if (typeof(NodeInput) == nodeButton.GetType()) {
					return false;
				}
				return true;
			}

			public override void Step(int yPos) {
				var color = ButtonType.GetTypeColor();
				UI.PushTint(color);
				var location = new Vec3(0.1f + 0.04f, -yPos * 0.05f, 0);
				lastPos = (Matrix.T(location) * _node.Pos.ToMatrix()).Translation;
				if (ConnectedTo is NodeOutputBase node) {
					Lines.Add(location + new Vec3(-0.04f, -0.04f, 0) / 2, (Matrix.T(node.lastPos + new Vec3(0.04f, -0.04f, 0) / 2) * _node.Pos.ToMatrix().Inverse).Translation, color, 0.01f);
				}
				if (UI.ButtonAt("", location, new Vec2(0.04f))) {
					if (_node.App.Player.scriptEditor.NodeButton == this) {
						_node.App.Player.scriptEditor.IsInput = false;
						_node.App.Player.scriptEditor.TypeTargetReadType = null;
						_node.App.Player.scriptEditor.NodeButton = null;
					}
					else if (!_node.App.Player.scriptEditor.IsInput) {
						if (_node.App.Player.scriptEditor.NodeButton is null) {
							_node.App.Player.scriptEditor.IsInput = true;
							_node.App.Player.scriptEditor.TypeTargetReadType = ButtonType;
							_node.App.Player.scriptEditor.NodeButton = this;
						}
						else {
							if (Flow) {
								if(_node.App.Player.scriptEditor.NodeButton.ConnectedTo?.ConnectedTo is not null) {
									_node.App.Player.scriptEditor.NodeButton.ConnectedTo.ConnectedTo = null;
								}
							}
							_node.App.Player.scriptEditor.NodeButton.ConnectedTo = _node.App.Player.scriptEditor.NodeButton.ConnectedTo == this ? null : (NodeButton)this;
							_node.App.Player.scriptEditor.IsInput = false;
							_node.App.Player.scriptEditor.TypeTargetReadType = null;
							_node.App.Player.scriptEditor.NodeButton = null;
						}
					}
					else {
						_node.App.Player.scriptEditor.IsInput = true;
						_node.App.Player.scriptEditor.TypeTargetReadType = ButtonType;
						_node.App.Player.scriptEditor.NodeButton = this;
					}
				}
				UI.PopTint();
			}
			public override void InvokeNode() {
				_node?.InvokeNode();
			}
		}

		protected readonly List<NodeInput> nodeInputs = new();

		protected readonly List<NodeOutputBase> nodeOutputs = new();

		public NodeOutput<T> GetNodeOutput<T>(int current) {
			return nodeOutputs.Count <= current ? null : nodeOutputs[current] is NodeOutput<T> outdata ? outdata : null;
		}

		public bool HasFlowInput => nodeInputs.Count != 0 && nodeInputs[0].IsConnectedTo;
		public bool HasFlowOutputs => nodeOutputs.Count != 0 && nodeOutputs[0].IsConnectedTo;

		public bool HasFlow => HasFlowInput | HasFlowOutputs;

		public override void Dispose() {
			base.Dispose();
			foreach (var item in nodeInputs) {
				item.Dispose();
			}
			nodeInputs.Clear();
			foreach (var item in nodeOutputs) {
				item.Dispose();
			}
			nodeOutputs.Clear();
		}

		public abstract void RenderUI();

		public abstract void LoadArgs(params string[] args);

		protected abstract void Invoke();
		public void InvokeNode() {
			foreach (var item in nodeInputs) {
				if (!item.TargetNodeHasFlow) {
					item?.ConnectedTo?.InvokeNode();
				}
			}
			Invoke();
			if (nodeOutputs.Count == 0) {
				return;
			}
			nodeOutputs[0].InvokeFlow();
		}

		protected override void Step(float dt) {
			UI.PushId(ID);
			UI.WindowBegin(Name, ref Pos, UIWin.Normal);
			try {
				RenderUI();
			}
			catch (Exception e) {
				Log.Err("Failed To RenderUI Error:" + e);
			}
			for (var i = 0; i < Math.Max(nodeInputs.Count, nodeOutputs.Count); i++) {
				UI.PushId(i);
				UI.PushId(43);
				if (nodeInputs.Count > i) {
					nodeInputs[i].Step(i);
				}
				UI.PopId();
				if (nodeOutputs.Count > i) {
					nodeOutputs[i].Step(i);
				}
				UI.PopId();
			}

			UI.WindowEnd();
			UI.PopId();
		}

	}
}
