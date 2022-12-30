using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class TriggerButton : NodeBase
	{
		public override bool IsInputEvent => true;

		protected override void Invoke() { }

		public override void LoadArgs(params string[] args) {

		}

		public override void RenderUI() {
			if (UI.Button("Trigger")) {
				try {
					InvokeNode();
				}
				catch (Exception e) {
					Log.Err("Button Error" + e);
				}
			}
		}
	}
}
