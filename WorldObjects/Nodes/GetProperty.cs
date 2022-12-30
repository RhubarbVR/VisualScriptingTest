using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class GetProperty<T, T2> : NodeBase
	{
		private string _targetPram;

		private Func<T, T2> _propertyInfo;

		public string TargetPram
		{
			get => _targetPram;
			set {
				_targetPram = value;
				var target = from p in typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.PropertyType == typeof(T2)
							 where p.CanRead
							 select p;
				try {
					_propertyInfo = (Func<T, T2>)target.FirstOrDefault()?.GetGetMethod()?.CreateDelegate(typeof(Func<T, T2>));
				}
				catch {
					var temp = ((Func<T2>)target.FirstOrDefault()?.GetGetMethod()?.CreateDelegate(typeof(Func<T2>)));
					_propertyInfo = (T a) => temp();
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
			nodeOutputs.Add(new NodeOutput<T2>(this) {
				Name = "Value",
				ButtonType = typeof(T2),
			});
			nodeInputs.Add(new NodeInput(this) {
				Name = "Target",
				ButtonType = typeof(T),
			});
		}



		public override void RenderUI() {
			UI.Text("Get " + TargetPram, TextAlign.Center);
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
