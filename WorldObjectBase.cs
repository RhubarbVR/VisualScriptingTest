using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StereoKit;

namespace VisualScript
{
	public abstract class WorldObject : IWorldObject
	{
		protected virtual void Step(float dt) {

		}

		protected virtual void AlwaysStep(float dt) {

		}


		void IWorldObject.Step() {
			AlwaysStep(Time.Stepf);
			if (!Enabled) {
				return;
			}
			Step(Time.Stepf);
		}

		protected virtual void Init() {

		}

		void IWorldObject.Init(App app) {
			App = app;
			ID = App.Add(this);
			Init();
		}

		public virtual void Dispose() {
			App.Remove(this);
			App = null;
		}

		public virtual string Name => GetType().GetFormattedName();
		public int ID { get; private set; }
		public App App { get; private set; }
		public bool Enabled { get; set; } = true;
	}
}
