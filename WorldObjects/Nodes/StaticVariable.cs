using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class StaticVariable : NodeBase
	{
		public override bool IsInputEvent => true;
		public override bool IsOutputEvent => true;

		protected override void Invoke() { }

		protected override void Init() {
			base.Init();
			nodeOutputs.Add(new NodeOutput<object>(this) {
				Name = "value",
				ButtonType = typeof(object),
			});
		}

		public override void RenderUI() {
			UI.Text("Static", TextAlign.Center);
		}

		public override void LoadArgs(params string[] args) {
			if (args.Length == 1) {
				var type = Type.GetType(args[0]);
				nodeOutputs[1].ButtonType = type;
			}
		}
	}
}
