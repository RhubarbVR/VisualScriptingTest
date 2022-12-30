using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


using StereoKit;


namespace VisualScript
{
	public static class Helper
	{

		public static Type PraseType(in string type, Assembly[] asm = null) {
			asm ??= AppDomain.CurrentDomain.GetAssemblies();
			return type.Contains('<') && type.Contains('>') ? PraseGeneric(type, asm) : SingleTypeParse(type, asm);
		}

		public static string[] ExtraNameSpaces = new string[] {
			"System.",
			"VisualScript.",
		};

		public static Type SingleTypeParse(string type, in Assembly[] asm) {
			if (type == "int") {
				type = nameof(Int32);
			}
			if (type == "uint") {
				type = nameof(UInt32);
			}
			if (type == "bool") {
				type = nameof(Boolean);
			}
			if (type == "char") {
				type = nameof(Char);
			}
			if (type == "string") {
				type = nameof(String);
			}
			if (type == "float") {
				type = nameof(Single);
			}
			if (type == "double") {
				type = nameof(Double);
			}
			if (type == "long") {
				type = nameof(Int64);
			}
			if (type == "ulong") {
				type = nameof(UInt64);
			}
			if (type == "byte") {
				type = nameof(Byte);
			}
			if (type == "sbyte") {
				type = nameof(SByte);
			}
			if (type == "short") {
				type = nameof(Int16);
			}
			if (type == "ushort") {
				type = nameof(UInt16);
			}
			var returnType = Type.GetType(type, false, true);
			if (returnType == null) {
				foreach (var item in asm) {
					if (returnType == null) {
						returnType = item.GetType(type, false, true);
						if (returnType != null) {
							return returnType;
						}
					}
				}
			}
			if (returnType == null) {
				foreach (var item in ExtraNameSpaces) {
					returnType = Type.GetType(item + type, false, true);
					if (returnType == null) {
						foreach (var itema in asm) {
							if (returnType == null) {
								returnType = itema.GetType(item + type, false, true);
								if (returnType != null) {
									return returnType;
								}
							}
						}
					}
					if (returnType != null) {
						return returnType;
					}
				}
			}
			return returnType;
		}
		public static Type PraseGeneric(in string type, in Assembly[] asm) {
			var firstGroup = type.IndexOf('<');
			var depth = 0;
			var lastIndex = firstGroup + 1;
			var types = new List<Type>();
			for (var i = lastIndex; i < type.Length; i++) {
				var c = type[i];
				if ((c == ',' || c == '>') && depth == 0) {
					var ennerdata = type.Substring(lastIndex, i - lastIndex);
					types.Add(PraseType(ennerdata, asm));
					lastIndex = i + 1;
				}
				if (c == '<') {
					depth++;
				}
				if (c == '>') {
					depth--;
				}
			}
			var FirstPartOfType = type.Substring(0, firstGroup);
			var starttype = SingleTypeParse(FirstPartOfType + $"`{types.Count}", asm);

			return starttype.MakeGenericType(types.ToArray());
		}



		public static string GetFormattedName(this Type type) {
			if (type == null) {
				return "Null";
			}
			if (type.IsGenericType) {
				var genericArguments = type.GetGenericArguments()
									.Select(x => x.Name)
									.Aggregate((x1, x2) => $"{x1}, {x2}");
				return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
					 + $" <{genericArguments}>";
			}
			return type.Name;
		}
		public static unsafe int GetHashCodeSafe(this string s) {
			if (s == null) {
				return int.MinValue;
			}
			fixed (char* str = s.ToCharArray()) {
				var chPtr = str;
				var num = 0x15051505;
				var num2 = num;
				var numPtr = (int*)chPtr;
				for (var i = s.Length; i > 0; i -= 4) {
					num = ((num << 5) + num + (num >> 0x1b)) ^ numPtr[0];
					if (i <= 2) {
						break;
					}
					num2 = ((num2 << 5) + num2 + (num2 >> 0x1b)) ^ numPtr[1];
					numPtr += 2;
				}
				return num + (num2 * 0x5d588b65);
			}
		}

		public static Color GetHashHue(this string str) {
			var hashCode = str.GetHashCodeSafe();
			var h = (float)(int)(ushort)hashCode % 360f;
			var s = (float)(int)(byte)(hashCode >> 16) / 255f / 2f;
			var v = 0.5f + ((float)(int)(byte)(hashCode >> 24) / 255f * 0.5f);
			return Color.HSV(h, s, v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAssignableTo(this Type type, Type type1) {
			return type1.IsAssignableFrom(type);
		}

		public static Type MemberInnerType(this MemberInfo data) {
			switch (data.MemberType) {
				case MemberTypes.Field:
					return ((FieldInfo)data).FieldType;
				case MemberTypes.Property:
					return ((PropertyInfo)data).PropertyType;
				default:
					break;
			}
			return null;
		}
		public static Color GetTypeColor(this Type type) {
			if (type is null) {
				return Color.Black;
			}
			if (type == typeof(bool)) {
				return new Color(0.25f, 0.25f, 0.25f);
			}
			if (type == typeof(string)) {
				return new Color(0.5f, 0f, 0f);
			}
			if (type == typeof(Color)) {
				return new Color(0.75f, 0.4f, 0f);
			}
			if (type == typeof(Action)) {
				return new Color(1, 1, 1);
			}
			if (type == typeof(object)) {
				return new Color(0.68f, 0.82f, 0.3137254901960784f);
			}
			if (type == typeof(byte)) {
				return new Color(0, 0.1f, 1) + new Color(0, 1f, 0.7f);
			}
			if (type == typeof(ushort)) {
				return new Color(0, 0.1f, 1) + new Color(0, 1f, 0.6f);
			}
			if (type == typeof(uint)) {
				return new Color(0, 0.1f, 1) + new Color(0, 1f, 0.5f);
			}
			if (type == typeof(ulong)) {
				return new Color(0, 0.1f, 1) + new Color(0, 1f, 0.4f);
			}
			if (type == typeof(sbyte)) {
				return new Color(1f, 0.7f, 0) + new Color(0.7f, 1f, 0f);
			}
			if (type == typeof(short)) {
				return new Color(1f, 0.7f, 0) + new Color(0.6f, 1f, 0f);
			}
			if (type == typeof(int)) {
				return new Color(1f, 0.7f, 0) + new Color(0.5f, 1f, 0f);
			}
			if (type == typeof(long)) {
				return new Color(1f, 0.7f, 0) + new Color(0.4f, 1f, 0f);
			}
			if (type == typeof(float)) {
				return new Color(0f, 1f, 1f) + new Color(0f, 1f, 0.7f);
			}
			if (type == typeof(double)) {
				return new Color(0f, 1f, 1f) + new Color(0f, 1f, 0.6f);
			}
			if (type == typeof(decimal)) {
				return new Color(0f, 1f, 1f) + new Color(0f, 1f, 0.5f);
			}
			var hashCode = type.GetFormattedName().GetHashCodeSafe();
			var h = (float)(int)(ushort)hashCode % 360f;
			var s = (float)(int)(byte)(hashCode >> 16) / 255f / 2f;
			var v = 0.5f + ((float)(int)(byte)(hashCode >> 24) / 255f * 0.5f);
			return Color.HSV(h, s, v);
		}

	}

}
