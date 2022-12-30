using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class DisplayNode<T> : NodeBase
	{
		public override bool IsOutputEvent => true;

		protected override void Invoke() { }

		protected override void Init() {
			base.Init();
			nodeInputs.Add(new NodeInput(this) {
				Name = "IN",
				ButtonType = typeof(T),
			});
		}


		public override void LoadArgs(params string[] args) {

		}

		public override void RenderUI() {
			try {
				if (!HasFlow) {
					InvokeNode();
				}
				var text = nodeInputs[1].GetValue<T>();
				if (text is null) {
					UI.PushTint(new Color(0.9f, 0.9f, 0.9f));
					UI.Text("null", TextAlign.Center);
					UI.PopTint();
				}
				else {
					UI.Text(text.ToString(), TextAlign.Center);
				}
			}
			catch (Exception e) {
				UI.Text("Error: " + e);
			}

		}
	}
}
