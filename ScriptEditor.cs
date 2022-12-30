using Microsoft.Maui.Controls;

using StereoKit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VisualScript;

namespace VisualScript;

public sealed class ScriptEditor
{
	public struct UIElement
	{
		public string name;
		public string spawnNodeType;
	}

	private readonly Player _player;

	public ScriptEditor(Player player) {
		_player = player;
		UIPos = new Pose(new Vec3(0, 0.25f, -0.5f), Quat.LookAt(Vec3.Forward, Vec3.Zero));
		_targetReadType = typeof(string);
		SpawnNodeType = BuildSpawnNodeType(typeof(Const<string>));
		ReBuildUI();
	}

	private static readonly Dictionary<Color, TextStyle> _anyColor = new();

	public static TextStyle GetTextStyle(Color color) {
		if (_anyColor.ContainsKey(color)) {
			return _anyColor[color];
		}
		_anyColor.Add(color, Text.MakeStyle(Font.Default, TextStyle.Default.CharHeight, color));
		return _anyColor[color];
	}
	private TextStyle _targetReadTypeStyle;
	private Type _targetReadType;
	private readonly List<UIElement> _elements = new();

	public Type TypeTargetReadType
	{
		get => _targetReadType; set {
			_targetReadType = value;
			ReBuildUI();
		}
	}


	public bool IsInput { get; set; }

	public string SpawnNodeType;

	public int CurrentPage { get; set; } = 0;

	private NodeBase.NodeButton _nodeOutput;

	private void ClearNodeOutput() {
		_nodeOutput = null;
	}

	public NodeBase.NodeButton NodeButton
	{
		get => _nodeOutput;
		set {
			if (_nodeOutput is not null) {
				_nodeOutput.OnDispos -= ClearNodeOutput;
			}
			if (value is not null) {
				value.OnDispos += ClearNodeOutput;
			}
			_nodeOutput = value;
		}
	}

	private void ReBuildUI() {
		_targetReadTypeStyle = GetTextStyle(TypeTargetReadType.GetTypeColor());
		_elements.Clear();
		CurrentPage = 0;
		_elements.Add(new UIElement {
			name = "TriggerButton",
			spawnNodeType = BuildSpawnNodeType(typeof(TriggerButton))
		});
		_elements.Add(new UIElement {
			name = "Display",
			spawnNodeType = BuildSpawnNodeType(typeof(DisplayNode<string>))
		});

		if (!IsInput) {
			var methods = TypeTargetReadType?.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) ?? Array.Empty<MethodInfo>();
			foreach (var item in methods) {
				if (item.Attributes.HasFlag(MethodAttributes.SpecialName)) {
					continue;
				}
				if (item.Attributes.HasFlag(MethodAttributes.HasSecurity)) {
					continue;
				}
				if (item.IsGenericMethod) {

				}
				else {
					if (item.ReturnParameter.ParameterType == typeof(void)) {
						if (item.GetParameters().Length == 0) {
							try {
								_elements.Add(new UIElement {
									name = item.Name + "()",
									spawnNodeType = BuildSpawnNodeType((typeof(CallMethodVoid<>).MakeGenericType(TypeTargetReadType)), item.Name)
								});
							}
							catch { }
						}
						else {

						}
					}
					else {
						if (item.GetParameters().Length == 0) {
							try {
								_elements.Add(new UIElement {
									name = item.Name + "()",
									spawnNodeType = BuildSpawnNodeType((typeof(CallMethodOut<,>).MakeGenericType(TypeTargetReadType, item.ReturnParameter.ParameterType)), item.Name)
								});
							}
							catch { }
						}
						else {

						}
					}
				}
			}

			var props = TypeTargetReadType?.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) ?? Array.Empty<PropertyInfo>();
			foreach (var prop in props) {
				if (prop.CanRead) {
					try {
						_elements.Add(new UIElement {
							name = "Get " + prop.Name,
							spawnNodeType = BuildSpawnNodeType((typeof(GetProperty<,>).MakeGenericType(TypeTargetReadType, prop.PropertyType)), prop.Name)
						});
					}
					catch { }
				}
				if (prop.CanWrite) {
					try {
						_elements.Add(new UIElement {
							name = "Set " + prop.Name,
							spawnNodeType = BuildSpawnNodeType((typeof(SetProperty<,>).MakeGenericType(TypeTargetReadType, prop.PropertyType)), prop.Name)
						});
					}
					catch { }
				}
			}
		}
		var asemb = new Assembly[2] {
			typeof(UIElement).Assembly,
			typeof(Color).Assembly,
		};
		var systemTypes = new Type[] { typeof(string), typeof(Type), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
		foreach (var item in systemTypes) {
			_elements.Add(new UIElement {
				name = "Const " + item.GetFormattedName(),
				spawnNodeType = BuildSpawnNodeType(typeof(Const<>).MakeGenericType(item))
			});
		}
		var staticTypes = from asm in asemb
						  from t in asm.GetTypes()
						  where t.IsPublic
						  where !t.IsAbstract
						  where !t.IsGenericType
						  where t.GetMethods(BindingFlags.Static | BindingFlags.Public).Length >= 0
						  select t;
		foreach (var item in staticTypes) {
			_elements.Add(new UIElement {
				name = "Const " + item.GetFormattedName(),
				spawnNodeType = BuildSpawnNodeType(typeof(Const<>).MakeGenericType(item))
			});
		}
	}

	public Pose UIPos;

	private bool _spawnLastFrame = false;

	public void Step() {
		var spawnThisFrame = false;
		var currentSide = Handed.Right;
		var (typeData, ArgData) = GetSpawnNodeTypeData(SpawnNodeType);
		if (Input.Controller(Handed.Right).trigger >= 0.5f) {
			spawnThisFrame = true;
			currentSide = Handed.Right;
		}
		if (Input.Controller(Handed.Left).trigger >= 0.5f) {
			spawnThisFrame = true;
			currentSide = Handed.Left;
		}
		if (Input.Key(Key.MouseCenter).IsActive()) {
			spawnThisFrame = true;
		}
		if (Input.Key(Key.Tab).IsActive()) {
			spawnThisFrame = true;
		}
		if (spawnThisFrame & !_spawnLastFrame) {
			var spawnPos = Pose.Identity;
			switch (currentSide) {
				case Handed.Left:
					spawnPos = Input.Hand(Handed.Left).palm;
					break;
				case Handed.Right:
					spawnPos = Input.Hand(Handed.Right).palm;
					break;
				default:
					break;
			}
			var node = _player.App.AddWorldObject<NodeBase>(typeData);
			node.LoadArgs(ArgData);
			if (node is not null) {
				node.Pos = spawnPos;
			}
		}
		_spawnLastFrame = spawnThisFrame;
		UI.WindowBegin("Script Editor", ref UIPos, new Vec2(0.3f, 0.5f));
		var startIndex = CurrentPage * 9;
		var xPos = 0;
		var yPos = 0;
		void AddButton() {
			if (startIndex < _elements.Count) {
				var element = _elements[startIndex];
				UI.PushId(startIndex);
				var hasBeenEnabled = false;
				if (SpawnNodeType == element.spawnNodeType) {
					UI.PushEnabled(false);
					hasBeenEnabled = true;
				}
				if (UI.ButtonAt(element.name, new Vec3(0.13f - (xPos * 0.09f), -0.02f - (yPos * 0.09f), 0), new Vec2(0.075f))) {
					SpawnNodeType = element.spawnNodeType;
				}
				if (hasBeenEnabled) {
					UI.PopEnabled();
				}
				UI.PopId();
			}
			startIndex++;
		}
		AddButton();
		xPos++;
		AddButton();
		xPos++;
		AddButton();
		yPos++;
		xPos = 0;
		AddButton();
		xPos++;
		AddButton();
		xPos++;
		AddButton();
		yPos++;
		xPos = 0;
		AddButton();
		xPos++;
		AddButton();
		xPos++;
		AddButton();
		if (CurrentPage == 0) {
			UI.PushEnabled(false);
		}
		UI.PushId("BackPage");
		if (UI.ButtonAt("<", new Vec3(0.13f, -0.3f, 0), new Vec2(0.06f))) {
			CurrentPage--;
		}
		UI.PopId();
		if (CurrentPage == 0) {
			UI.PopEnabled();
		}
		var maxPage = _elements.Count / 9;
		var enabledActive = false;
		if (CurrentPage >= maxPage) {
			UI.PushEnabled(false);
			enabledActive = true;
		}
		UI.PushId("ForwaredPage");
		if (UI.ButtonAt(">", new Vec3(-0.06f, -0.3f, 0), new Vec2(0.06f))) {
			CurrentPage++;
		}
		UI.PopId();
		if (enabledActive) {
			UI.PopEnabled();
		}
		UI.NextLine();
		UI.Space(0.4f);
		if (typeData is not null) {
			var argString = "";
			if (ArgData.Length > 0) {
				argString = "(";
				foreach (var item in ArgData) {
					argString += item + " ,";
				}
				argString = argString.Remove(argString.Length - 2);
				argString += ")";
			}
			UI.Text(typeData.GetFormattedName() + argString, TextAlign.Center);
		}
		UI.PushTextStyle(_targetReadTypeStyle);
		UI.Text((IsInput ? "Input: " : "Output: ") + TypeTargetReadType.GetFormattedName(), TextAlign.Center);
		UI.PopTextStyle();
		UI.WindowEnd();
	}

	public string BuildSpawnNodeType(Type targetComp, params string[] args) {
		return args.Length <= 0 ? (targetComp?.FullName) : targetComp?.FullName + "`~`" + string.Join("`~`", args);
	}

	public (Type, string[]) GetSpawnNodeTypeData(string type) {
		if (string.IsNullOrEmpty(type)) {
			return (null, null);
		}
		var data = type.Split("`~`");
		if (data.Length <= 0) {
			return (null, null);
		}
		var typeInfo = Type.GetType(data[0]);
		var returenArray = new string[data.Length - 1];
		Array.Copy(data, 1, returenArray, 0, returenArray.Length);
		return (typeInfo, returenArray);
	}
}