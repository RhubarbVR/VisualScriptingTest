using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class SetProperty<T, T2> : NodeBase
	{
		private CallVoidARG<T, T2> _propertyInfo;
		private string _targetPram;

		public string TargetPram
		{
			get => _targetPram;
			set {
				_targetPram = value;
				var target = from p in typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.PropertyType == typeof(T2)
							 where p.CanWrite
							 where p.Name == _targetPram
							 select p;
				try {
					try {
						_propertyInfo = (CallVoidARG<T, T2>)target.FirstOrDefault()?.GetSetMethod()?.CreateDelegate(typeof(CallVoidARG<T, T2>));
					}
					catch {
						var temp = (Action<T, T2>)target.FirstOrDefault()?.GetSetMethod()?.CreateDelegate(typeof(Action<T, T2>));
						_propertyInfo = (ref T a, T2 b) => temp(a, b);
					}
				}
				catch {
					var temp = ((Action<T2>)target.FirstOrDefault()?.GetSetMethod()?.CreateDelegate(typeof(Action<T2>)));
					_propertyInfo = (ref T a, T2 b) => temp(b);
				}
			}
		}

		public override void LoadArgs(params string[] args) {
			if (args.Length >= 1) {
				TargetPram = args[0];
			}
		}
		protected override void Init() {
			base.Init();
			nodeInputs.Add(new NodeInput(this) {
				Name = "Target",
				ButtonType = typeof(T),
			});
			nodeInputs.Add(new NodeInput(this) {
				Name = "Value",
				ButtonType = typeof(T2),
			});
		}



		public override void RenderUI() {
			UI.Text("Set " + TargetPram, TextAlign.Center);
		}

		protected override void Invoke() {
			_propertyInfo?.Invoke(ref nodeInputs[1].GetValue<T>(), nodeInputs[2].GetValue<T2>());
		}
	}
}
