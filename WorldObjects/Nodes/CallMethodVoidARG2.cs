using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class CallMethodVoidARG<T, A1, A2> : NodeBase
	{
		private CallVoidARG<T, A1, A2> _propertyInfo;
		private string _targetMethod;

		public string TargetMethod
		{
			get => _targetMethod;
			set {
				_targetMethod = value;
				var target = from p in typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.GetParameters().Length == 2
							 where p.GetParameters()[0].ParameterType == typeof(A1)
							 where p.GetParameters()[1].ParameterType == typeof(A2)
							 where p.ReturnParameter.ParameterType == typeof(void)
							 where p.Name == _targetMethod
							 select p;
				nodeInputs[2].Name = target.FirstOrDefault()?.GetParameters()[0].Name;
				nodeInputs[3].Name = target.FirstOrDefault()?.GetParameters()[1].Name;
				try {
					try {
						_propertyInfo = (CallVoidARG<T, A1, A2>)target.FirstOrDefault().CreateDelegate(typeof(CallVoidARG<T, A1, A2>));
					}
					catch {
						var temp = ((Action<T, A1, A2>)target.FirstOrDefault().CreateDelegate(typeof(Action<T, A1, A2>)));
						_propertyInfo = (ref T a, A1 b, A2 c) => temp(a, b, c);
					}
				}
				catch {
					var temp = (Action<A1, A2>)target.FirstOrDefault()?.CreateDelegate(typeof(Action<A1, A2>));
					_propertyInfo = (ref T a, A1 b, A2 c) => temp(b, c);
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
			nodeInputs.Add(new NodeInput(this) {
				Name = "Arg1",
				ButtonType = typeof(A1),
			});
			nodeInputs.Add(new NodeInput(this) {
				Name = "Arg2",
				ButtonType = typeof(A2),
			});
		}



		public override void RenderUI() {
			UI.Text(TargetMethod + "(" + typeof(A1).GetFormattedName() + " " + nodeInputs[2].Name + "," + typeof(A2).GetFormattedName() + " " + nodeInputs[3].Name + ")", TextAlign.Center);
		}

		protected override void Invoke() {
			_propertyInfo?.Invoke(ref nodeInputs[1].GetValue<T>(), nodeInputs[2].GetValue<A1>(), nodeInputs[3].GetValue<A2>());
		}
	}
}
