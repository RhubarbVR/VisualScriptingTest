using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using StereoKit;

namespace VisualScript
{
	public sealed class Variable<T> : NodeBase
	{
		public override bool IsInputEvent => true;
		public override bool IsOutputEvent => true;

		protected override void Invoke() { }

		private T _value = default;

		public T Value
		{
			get => _value;
			set {
				_value = value;
				_rep = Value?.ToString();
			}
		}

		private string _rep;

		protected override void Init() {
			base.Init();
			nodeOutputs.Add(new NodeOutput<T>(this) {
				Name = "value",
				ButtonType = typeof(T),
				GetValue = () => ref _value
			});
			_rep = Value?.ToString();
		}

		private bool _isError = false;

		public override void RenderUI() {
			if (!(typeof(T).IsValueType | typeof(T) == typeof(string) | typeof(T) == typeof(Type))) {
				if (_rep is null) {
					UI.PushTint(new Color(0.9f, 0.9f, 0.9f));
					UI.Text("null", TextAlign.Center);
					UI.PopTint();
				}
				else {
					UI.Text(_rep, TextAlign.Center);
				}
				return;
			}
			var change = false;
			if (_isError) {
				UI.PushTint(new Color(1, 0, 0));
				change = true;
			}
			if (UI.Input(ID.ToString(), ref _rep)) {
				if (typeof(T) == typeof(Type)) {
					try {
						var result = (T)(object)Helper.PraseType(_rep);
						if (!EqualityComparer<T>.Default.Equals(_value, result)) {
							Value = result;
						}
						_isError = false;
					}
					catch {
						_isError = true;
					}
				}
				else {
					try {
						var converter = TypeDescriptor.GetConverter(typeof(T));
						var result = (T)converter.ConvertFrom(_rep);
						if (!EqualityComparer<T>.Default.Equals(_value, result)) {
							Value = result;
						}
						_isError = false;
					}
					catch {
						_isError = true;
					}
				}
			}
			if (change) {
				UI.PopTint();
			}
		}

		public override void LoadArgs(params string[] args) {
		}
	}
}
