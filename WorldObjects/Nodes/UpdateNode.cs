using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class UpdateNode : NodeBase
	{
		public override bool IsInputEvent => true;

		protected override void Invoke() { }

		public override void LoadArgs(params string[] args) {

		}

		protected override void AlwaysStep(float dt) {
			base.AlwaysStep(dt);
			try {
				InvokeNode();
			}
			catch (Exception e) {
				Log.Err("Update Error" + e);
			}
		}

		public override void RenderUI() {
			UI.Text("Update");
		}
	}
}
