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

		private Call<T, T2> _propertyInfo;

		public string TargetPram
		{
			get => _targetPram;
			set {
				_targetPram = value;
				var target = from p in typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.PropertyType == typeof(T2)
							 where p.Name == _targetPram
							 where p.CanRead
							 select p;
				try {
					try{
						_propertyInfo = (Call<T, T2>)target.FirstOrDefault()?.GetGetMethod()?.CreateDelegate(typeof(Call<T, T2>));
					}
					catch {
						var temp = ((Func<T, T2>)target.FirstOrDefault()?.GetGetMethod()?.CreateDelegate(typeof(Func<T, T2>)));
						_propertyInfo = (ref T a) => temp(a);
					}
				}
				catch {
					var temp = ((Func<T2>)target.FirstOrDefault()?.GetGetMethod()?.CreateDelegate(typeof(Func<T2>)));
					_propertyInfo = (ref T a) => temp();
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
				GetValue = () => ref _output
			});
			nodeInputs.Add(new NodeInput(this) {
				Name = "Target",
				ButtonType = typeof(T),
			});
		}



		public override void RenderUI() {
			UI.Text("Get " + TargetPram, TextAlign.Center);
		}
		private T2 _output;
		protected override void Invoke() {
			_output = default;
			if (_propertyInfo is not null) {
				_output = _propertyInfo.Invoke(ref nodeInputs[1].GetValue<T>());
			}
		}
	}
}
