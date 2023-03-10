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
		private Call<T, T2> _propertyInfo;
		private string _targetMethod;

		public string TargetMethod
		{
			get => _targetMethod;
			set {
				_targetMethod = value;
				var target = from p in typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
							 where p.GetParameters().Length == 0
							 where p.ReturnParameter.ParameterType == typeof(T2)
							 where p.Name == _targetMethod
							 select p;
				try {
					try { 
						_propertyInfo = (Call<T, T2>)target.FirstOrDefault().CreateDelegate(typeof(Call<T, T2>));
					}
					catch {
						var temp = ((Func<T, T2>)target.FirstOrDefault().CreateDelegate(typeof(Func<T, T2>)));
						_propertyInfo = (ref T a) => temp(a);
					}
				}
				catch {
					var temp = (CallR<T2>)Delegate.CreateDelegate(typeof(CallR<T2>), target.FirstOrDefault());
					_propertyInfo = (ref T a) => temp();
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
				GetValue = () => ref _outPutValue
			});
		}



		public override void RenderUI() {
			UI.Text(typeof(T2).GetFormattedName() + " " + TargetMethod + "()", TextAlign.Center);
		}

		private T2 _outPutValue = default;

		protected override void Invoke() {
			_outPutValue = default;
			if (_propertyInfo is not null) {
				_outPutValue = _propertyInfo(ref nodeInputs[1].GetValue<T>());
			}
		}
	}
}
