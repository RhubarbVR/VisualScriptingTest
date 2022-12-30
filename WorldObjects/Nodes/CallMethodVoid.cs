using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class CallMethodVoid<T> : NodeBase
	{
		private Action<T> _propertyInfo;
		private string _targetMethod;

		public string TargetMethod
		{
			get => _targetMethod;
			set {
				_targetMethod = value;
				var target = from p in typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.GetParameters().Length == 0
							 where p.ReturnParameter.ParameterType == typeof(void)
							 select p;
				try {
					_propertyInfo = (Action<T>)target.FirstOrDefault()?.CreateDelegate(typeof(Action<T>));
				}
				catch {
					var temp = ((Action)target.FirstOrDefault()?.CreateDelegate(typeof(Action)));
					_propertyInfo = (T a) => temp();
				}
			}
		}

		public override void LoadArgs(params string[] args) {
			if (args.Length >= 1) {
				TargetMethod = args[0];
			}
		}
		protected override void Init() {
			base.Init();
			nodeInputs.Add(new NodeInput(this) {
				Name = "Target",
				ButtonType = typeof(T),
			});
		}



		public override void RenderUI() {
			UI.Text(TargetMethod + "()", TextAlign.Center);
		}

		protected override void Invoke() {
			_propertyInfo?.Invoke(nodeInputs[1].GetValue<T>());
		}
	}
}
