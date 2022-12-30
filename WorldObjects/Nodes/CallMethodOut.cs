using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class CallMethodOut<T, T2> : NodeBase
	{
		private Func<T, T2> _propertyInfo;
		private string _targetMethod;

		public string TargetMethod
		{
			get => _targetMethod;
			set {
				_targetMethod = value;
				var target = from p in typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.GetParameters().Length == 0
							 where p.ReturnParameter.ParameterType == typeof(T2)
							 select p;
				try {
					_propertyInfo = (Func<T, T2>)Delegate.CreateDelegate(typeof(Func<T, T2>), target.FirstOrDefault());
				}
				catch {
					var temp = (Func<T2>)Delegate.CreateDelegate(typeof(Func<T2>),target.FirstOrDefault());
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
			nodeOutputs.Add(new NodeOutput<T2>(this) {
				Name = "Out",
				ButtonType = typeof(T2),
			});
		}



		public override void RenderUI() {
			UI.Text(typeof(T2).GetFormattedName() + " " + TargetMethod + "()", TextAlign.Center);
		}

		protected override void Invoke() {
			var outPut = GetNodeOutput<T2>(1);
			T2 e = default;
			if (_propertyInfo is not null) {
				e = _propertyInfo.Invoke(nodeInputs[1].GetValue<T>());
			}
			if (outPut is not null) {
				outPut.Value = e;
			}
		}
	}
}
