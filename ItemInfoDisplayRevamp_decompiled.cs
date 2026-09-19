using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Peak;
using Peak.Afflictions;
using TMPro;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: AssemblyCompany("com.github.chuxiaaaa.ItemInfoDisplayForkedCN")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.2.0")]
[assembly: AssemblyInformationalVersion("1.0.2")]
[assembly: AssemblyProduct("com.github.chuxiaaaa.ItemInfoDisplayForkedCN")]
[assembly: AssemblyTitle("ItemInfoDisplayForkedCN")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.2.0")]
[module: UnverifiableCode]
namespace ItemInfoDisplay
{
	internal static class Drone_Compat
	{
		private const string DroneGuid = "com.SebastianYang.DronePlugin";

		internal static bool Loaded => Chainloader.PluginInfos.ContainsKey("com.SebastianYang.DronePlugin");

		public static bool IsDroneActive
		{
			get
			{
				if (!Chainloader.PluginInfos.TryGetValue("com.SebastianYang.DronePlugin", out var value) || (Object)(object)value.Instance == (Object)null)
				{
					return false;
				}
				object instance = value.Instance;
				Type type = instance.GetType();
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
				string[] array = new string[2] { "isDroneActive", "IsDroneActive" };
				foreach (string name in array)
				{
					PropertyInfo property = type.GetProperty(name, bindingAttr);
					if (property != null && property.PropertyType == typeof(bool))
					{
						try
						{
							MethodInfo? getMethod = property.GetGetMethod(nonPublic: true);
							return (bool)property.GetValue(((object)getMethod != null && getMethod.IsStatic) ? null : instance);
						}
						catch
						{
						}
					}
					FieldInfo field = type.GetField(name, bindingAttr);
					if (field != null && field.FieldType == typeof(bool))
					{
						try
						{
							return (bool)field.GetValue(field.IsStatic ? null : instance);
						}
						catch
						{
						}
					}
				}
				return false;
			}
		}
	}
	[BepInDependency(/*Could not decode attribute arguments.*/)]
	[BepInPlugin("com.github.chuxiaaaa.ItemInfoDisplayForkedCN", "ItemInfoDisplayForkedCN", "1.0.3")]
	public class Plugin : BaseUnityPlugin
	{
		private static class ItemInfoDisplayUpdatePatch
		{
			[HarmonyPatch(typeof(CharacterItems), "Update")]
			[HarmonyPostfix]
			private static void ItemInfoDisplayUpdate(CharacterItems __instance)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Expected O, but got Unknown
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Expected O, but got Unknown
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Expected O, but got Unknown
				try
				{
					if ((Object)guiManager == (Object)null)
					{
						AddDisplayObject();
					}
					else if (Drone_Compat.Loaded && Drone_Compat.IsDroneActive)
					{
						if ((Object)itemInfoDisplayTextMesh != (Object)null && ((Component)itemInfoDisplayTextMesh).gameObject.activeSelf)
						{
							((Component)itemInfoDisplayTextMesh).gameObject.SetActive(false);
						}
					}
					else if ((Object)Character.observedCharacter.data.currentItem != (Object)null)
					{
						if (hasChanged)
						{
							hasChanged = false;
							ProcessItemGameObject();
						}
						else if (Mathf.Abs(Character.observedCharacter.data.sinceItemAttach - lastKnownSinceItemAttach) >= configForceUpdateTime.Value)
						{
							hasChanged = true;
							lastKnownSinceItemAttach = Character.observedCharacter.data.sinceItemAttach;
						}
						if (!((Component)itemInfoDisplayTextMesh).gameObject.activeSelf)
						{
							((Component)itemInfoDisplayTextMesh).gameObject.SetActive(true);
						}
					}
					else if (((Component)itemInfoDisplayTextMesh).gameObject.activeSelf)
					{
						((Component)itemInfoDisplayTextMesh).gameObject.SetActive(false);
					}
				}
				catch (Exception ex)
				{
					Log.LogError((object)(ex.Message + ex.StackTrace));
				}
			}
		}

		private static class ItemInfoDisplayEquipPatch
		{
			[HarmonyPatch(typeof(CharacterItems), "Equip")]
			[HarmonyPostfix]
			private static void ItemInfoDisplayEquip(CharacterItems __instance)
			{
				try
				{
					if ((Object)(object)Character.observedCharacter == (Object)(object)__instance.character)
					{
						hasChanged = true;
					}
				}
				catch (Exception ex)
				{
					Log.LogError((object)(ex.Message + ex.StackTrace));
				}
			}
		}

		private static class ItemInfoDisplayFinishCookingPatch
		{
			[HarmonyPatch(typeof(ItemCooking), "FinishCooking")]
			[HarmonyPostfix]
			private static void ItemInfoDisplayFinishCooking(ItemCooking __instance)
			{
				try
				{
					if ((Object)(object)Character.observedCharacter == (Object)(object)((ItemComponent)__instance).item.holderCharacter)
					{
						hasChanged = true;
					}
				}
				catch (Exception ex)
				{
					Log.LogError((object)(ex.Message + ex.StackTrace));
				}
			}
		}

		private static class ItemInfoDisplayReduceUsesRPCPatch
		{
			[HarmonyPatch(typeof(Action_ReduceUses), "ReduceUsesRPC")]
			[HarmonyPostfix]
			private static void ItemInfoDisplayReduceUsesRPC(Action_ReduceUses __instance)
			{
				try
				{
					if ((Object)(object)Character.observedCharacter == (Object)(object)((ItemActionBase)__instance).character)
					{
						hasChanged = true;
					}
				}
				catch (Exception ex)
				{
					Log.LogError((object)(ex.Message + ex.StackTrace));
				}
			}
		}

		private class ComponentEffectInfo
		{
			public Component Component { get; set; }

			public float Value { get; set; }

			public string EffectKey { get; set; }
		}

		private static GUIManager guiManager;

		private static TextMeshProUGUI itemInfoDisplayTextMesh;

		private static Dictionary<string, string> effectColors = new Dictionary<string, string>();

		private static float lastKnownSinceItemAttach;

		private static bool hasChanged;

		private static ConfigEntry<float> configFontSize;

		private static ConfigEntry<float> configOutlineWidth;

		private static ConfigEntry<float> configLineSpacing;

		private static ConfigEntry<float> configSizeDeltaX;

		private static ConfigEntry<float> configForceUpdateTime;

		public const string Id = "com.github.chuxiaaaa.ItemInfoDisplayForkedCN";

		internal static ManualLogSource Log { get; private set; } = null;

		public static bool EasyBackpack { get; set; }

		public static string Name => "ItemInfoDisplayForkedCN";

		public static string Version => "1.0.2";

		private void Awake()
		{
			Log = ((BaseUnityPlugin)this).Logger;
			InitEffectColors(effectColors);
			lastKnownSinceItemAttach = 0f;
			hasChanged = true;
			configFontSize = ((BaseUnityPlugin)this).Config.Bind<float>("ItemInfoDisplay", "Font Size", 20f, "Customize the Font Size for description text.");
			configOutlineWidth = ((BaseUnityPlugin)this).Config.Bind<float>("ItemInfoDisplay", "Outline Width", 0.08f, "Customize the Outline Width for item description text.");
			configLineSpacing = ((BaseUnityPlugin)this).Config.Bind<float>("ItemInfoDisplay", "Line Spacing", -35f, "Customize the Line Spacing for item description text.");
			configSizeDeltaX = ((BaseUnityPlugin)this).Config.Bind<float>("ItemInfoDisplay", "Size Delta X", 550f, "Customize the horizontal length of the container for the mod. Increasing moves text left, decreasing moves text right.");
			configForceUpdateTime = ((BaseUnityPlugin)this).Config.Bind<float>("ItemInfoDisplay", "Force Update Time", 1f, "Customize the time in seconds until the mod forces an update for the item.");
			Harmony.CreateAndPatchAll(typeof(ItemInfoDisplayUpdatePatch), (string)null);
			Harmony.CreateAndPatchAll(typeof(ItemInfoDisplayEquipPatch), (string)null);
			Harmony.CreateAndPatchAll(typeof(ItemInfoDisplayFinishCookingPatch), (string)null);
			Harmony.CreateAndPatchAll(typeof(ItemInfoDisplayReduceUsesRPCPatch), (string)null);
			Log.LogInfo((object)("Plugin " + Name + " is loaded!"));
		}

		private static string GetEffectChineseName(string effect)
		{
			if (1 == 0)
			{
			}
			string result = effect switch
			{
				"Hunger" => "HUNGER", 
				"Extra Stamina" => "EXTRA STAMINA", 
				"Spores" => "SPORES", 
				"Injury" => "INJURY", 
				"Poison" => "POISON", 
				"Cold" => "COLD", 
				"Heat" => "HEAT", 
				"Hot" => "HOT", 
				"Sleepy" => "SLEEPY", 
				"Drowsy" => "DROWSY", 
				"Curse" => "CURSE", 
				"Thorns" => "THORNS", 
				"Shield" => "SHIELD", 
				"Weight" => "WEIGHT", 
				"Web" => "WEB", 
				"Arrow" => "ARROW", 
				"Petrify" => "PETRIFY", 
				"FlyTrap" => "FLYTRAP", 
				_ => effect.ToUpper(), 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private static bool TryGetDeepComponent<T>(GameObject gameObject, out T component) where T : Component
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			component = default(T);
			if ((Object)gameObject == (Object)null)
			{
				return false;
			}
			if (gameObject.TryGetComponent<T>(ref component))
			{
				return true;
			}
			component = gameObject.GetComponentInChildren<T>(true);
			return (Object)(object)component != (Object)null;
		}

		private static string NormalizeItemIdentifier(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder(value.Length);
			foreach (char c in value)
			{
				if (char.IsLetterOrDigit(c))
				{
					stringBuilder.Append(char.ToUpperInvariant(c));
				}
			}
			return stringBuilder.ToString();
		}

		private static bool MatchesAnyItemIdentifier(Item currentItem, GameObject gameObject, params string[] expectedNames)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			List<string> list = new List<string>();
			if ((Object)currentItem != (Object)null)
			{
				if (currentItem.UIData != null)
				{
					list.Add(currentItem.UIData.itemName);
					try
					{
						list.Add(currentItem.GetName());
					}
					catch
					{
					}
				}
				list.Add(((Object)currentItem).name);
				if ((Object)((Component)currentItem).gameObject != (Object)null)
				{
					list.Add(((Object)((Component)currentItem).gameObject).name);
				}
			}
			if ((Object)gameObject != (Object)null)
			{
				list.Add(((Object)gameObject).name);
			}
			for (int i = 0; i < expectedNames.Length; i++)
			{
				string text = NormalizeItemIdentifier(expectedNames[i]);
				if (text.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (NormalizeItemIdentifier(list[j]) == text)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static Character GetTipCharacter(Item currentItem)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			if ((Object)currentItem != (Object)null)
			{
				if ((Object)currentItem.trueHolderCharacter != (Object)null)
				{
					return currentItem.trueHolderCharacter;
				}
				if ((Object)currentItem.holderCharacter != (Object)null)
				{
					return currentItem.holderCharacter;
				}
			}
			return Character.observedCharacter;
		}

		private static float GetCurrentStatusSafe(Character character, STATUSTYPE statusType)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)character == (Object)null || character.refs == null || (Object)character.refs.afflictions == (Object)null)
			{
				return 0f;
			}
			return Mathf.Max(0f, character.refs.afflictions.GetCurrentStatus(statusType));
		}

		private static string FormatPercentAmount(float value)
		{
			return FormatNumber(value * 100f);
		}

		private static float CalculateHealAllPotential(Character targetCharacter, float maxHealing, out string breakdown)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			breakdown = "";
			if ((Object)targetCharacter == (Object)null || maxHealing <= 0f)
			{
				return 0f;
			}
			STATUSTYPE[] array = new STATUSTYPE[6];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			STATUSTYPE[] array2 = (STATUSTYPE[])(object)array;
			float num = 0f;
			List<string> list = new List<string>();
			for (int i = 0; i < array2.Length; i++)
			{
				STATUSTYPE statusType = array2[i];
				float currentStatusSafe = GetCurrentStatusSafe(targetCharacter, statusType);
				if (!(currentStatusSafe <= 0f))
				{
					float num2 = Mathf.Min(currentStatusSafe, maxHealing - num);
					if (num2 > 0f)
					{
						string text = ((object)(STATUSTYPE)(ref statusType)/*cast due to .constrained prefix*/).ToString();
						list.Add(effectColors[text] + FormatPercentAmount(num2) + " " + GetEffectChineseName(text) + "</color>");
						num = Mathf.Min(maxHealing, num + num2);
					}
					if (num >= maxHealing)
					{
						break;
					}
				}
			}
			if (list.Count > 0)
			{
				breakdown = string.Join("、", list);
			}
			return num;
		}

		private static float CalculateCurableStatusSum(Character targetCharacter, bool curseIsCurable = false, bool petrifyIsCurable = false)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Invalid comparison between Unknown and I4
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)targetCharacter == (Object)null)
			{
				return 0f;
			}
			STATUSTYPE[] array = new STATUSTYPE[11];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			STATUSTYPE[] array2 = (STATUSTYPE[])(object)array;
			float num = 0f;
			foreach (STATUSTYPE val in array2)
			{
				if (((int)val != 5 || curseIsCurable) && ((int)val != 13 || petrifyIsCurable))
				{
					num += GetCurrentStatusSafe(targetCharacter, val);
				}
			}
			return num;
		}

		private static float GetOneTimePetrifyFromActions(GameObject gameObject)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Invalid comparison between Unknown and I4
			if ((Object)gameObject == (Object)null)
			{
				return 0f;
			}
			float num = 0f;
			Action_ModifyStatus[] componentsInChildren = gameObject.GetComponentsInChildren<Action_ModifyStatus>(true);
			foreach (Action_ModifyStatus val in componentsInChildren)
			{
				if ((Object)val != (Object)null && (int)val.statusType == 13 && val.changeAmount > 0f)
				{
					num += val.changeAmount;
				}
			}
			return num;
		}

		private static int GetBackpackFilledSlotCountSafe(Backpack backpack)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			if ((Object)backpack == (Object)null)
			{
				return -1;
			}
			try
			{
				return backpack.FilledSlotCount();
			}
			catch
			{
				return -1;
			}
		}

		private static ComponentEffectInfo GetComponentEffectInfo(Component component)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Expected O, but got Unknown
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			Type type = ((object)component).GetType();
			ComponentEffectInfo componentEffectInfo = new ComponentEffectInfo
			{
				Component = component
			};
			if (type == typeof(Action_RestoreHunger))
			{
				Action_RestoreHunger val = (Action_RestoreHunger)component;
				componentEffectInfo.Value = Mathf.Abs(val.restorationAmount);
				componentEffectInfo.EffectKey = "Action_RestoreHunger_Hunger";
			}
			else if (type == typeof(Action_GiveExtraStamina))
			{
				Action_GiveExtraStamina val2 = (Action_GiveExtraStamina)component;
				componentEffectInfo.Value = Mathf.Abs(val2.amount);
				componentEffectInfo.EffectKey = "Action_GiveExtraStamina_ExtraStamina";
			}
			else if (type == typeof(Action_InflictPoison))
			{
				Action_InflictPoison val3 = (Action_InflictPoison)component;
				componentEffectInfo.Value = Mathf.Abs(val3.poisonPerSecond * val3.inflictionTime);
				componentEffectInfo.EffectKey = "Action_InflictPoison_Poison";
			}
			else if (type == typeof(Action_AddOrRemoveThorns))
			{
				Action_AddOrRemoveThorns val4 = (Action_AddOrRemoveThorns)component;
				componentEffectInfo.Value = val4.thornCount;
				componentEffectInfo.EffectKey = "Action_AddOrRemoveThorns_Thorns";
			}
			else if (type == typeof(Action_ModifyStatus))
			{
				Action_ModifyStatus val5 = (Action_ModifyStatus)component;
				componentEffectInfo.Value = Mathf.Abs(val5.changeAmount);
				componentEffectInfo.EffectKey = $"Action_ModifyStatus_{val5.statusType}";
			}
			else
			{
				componentEffectInfo.Value = 0f;
				componentEffectInfo.EffectKey = type.Name;
			}
			return componentEffectInfo;
		}

		private static string FormatNumber(float value)
		{
			if (float.IsPositiveInfinity(value))
			{
				return "∞";
			}
			if (float.IsNegativeInfinity(value))
			{
				return "-∞";
			}
			return value.ToString("F1").Replace(".0", "");
		}

		private static string FormatSeconds(float value)
		{
			if (float.IsPositiveInfinity(value))
			{
				return "∞";
			}
			return FormatNumber(value) + " s";
		}

		private static string FormatPetrifyAmount(float value)
		{
			float value2 = ((Mathf.Abs(value) <= 1f) ? (value * 100f) : value);
			return FormatNumber(value2);
		}

		private static string GetNewVersionItemTips(GameObject gameObject, Item currentItem)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0550: Unknown result type (might be due to invalid IL or missing references)
			//IL_055b: Expected O, but got Unknown
			//IL_095d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0968: Expected O, but got Unknown
			//IL_0da1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dac: Expected O, but got Unknown
			StringBuilder stringBuilder = new StringBuilder();
			if ((Object)gameObject == (Object)null || (Object)currentItem == (Object)null)
			{
				return "";
			}
			Flare val = default(Flare);
			if (gameObject.TryGetComponent<Flare>(ref val))
			{
				stringBuilder.Append(effectColors["Hunger"]).Append("WHEN LIT</color>, EMITS A TALL COLUMN OF SMOKE WITH THE HOLDER'S COLOUR\n\n");
				stringBuilder.Append("<#CCCCCC>LIGHTING AT THE SUMMIT CALLS A HELICOPTER</color>\n\n");
			}
			Candle val2 = default(Candle);
			if (gameObject.TryGetComponent<Candle>(ref val2))
			{
				float value = (((ItemComponent)val2).HasData((DataEntryKey)10) ? ((ItemComponent)val2).GetData<FloatItemData>((DataEntryKey)10).Value : val2.startingFuel);
				stringBuilder.Append(effectColors["Hunger"]).Append("LIGHT/EXTINGUISH</color> CANDLE; WHEN LIT, DISPELS MIST\n\n");
				stringBuilder.Append("<#CCCCCC>BURN TIME LEFT: ").Append(FormatSeconds(value)).Append("</color>\n\n");
			}
			JetpackItem val3 = default(JetpackItem);
			if (gameObject.TryGetComponent<JetpackItem>(ref val3))
			{
				float value2 = (((ItemComponent)val3).HasData((DataEntryKey)10) ? ((ItemComponent)val3).GetData<FloatItemData>((DataEntryKey)10).Value : val3.startingFuel);
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("HOLD JUMP</color> TO JET-FLY\n\n");
				stringBuilder.Append("<#CCCCCC>FUEL LEFT: ").Append(FormatNumber(value2)).Append(" / ")
					.Append(FormatNumber(val3.startingFuel))
					.Append("</color>\n\n");
				stringBuilder.Append("<#CCCCCC>FULL TANK FLIES ~").Append(FormatSeconds(val3.startingFuel / 20f)).Append("</color>\n\n");
			}
			Rocketpack val4 = default(Rocketpack);
			if (gameObject.TryGetComponent<Rocketpack>(ref val4))
			{
				stringBuilder.Append(effectColors["Injury"]).Append("WHEN LIT</color>, IGNITES, THEN THRUSTS AND FINALLY EXPLODES\n\n");
				stringBuilder.Append("<#CCCCCC>~3 s FUSE, ~3 s FLIGHT</color>\n\n");
			}
			ParachuteItem val5 = default(ParachuteItem);
			if (gameObject.TryGetComponent<ParachuteItem>(ref val5))
			{
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("OPENS AUTOMATICALLY</color> WITH A LONG FALL\n\n");
				stringBuilder.Append("<#CCCCCC>USES ONE CHARGE WHEN OPENED; SLOWS FALL</color>\n\n");
			}
			Glider val6 = default(Glider);
			if (gameObject.TryGetComponent<Glider>(ref val6))
			{
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("OPENS AND GLIDES</color> WHEN OFF THE GROUND\n\n");
				stringBuilder.Append("<#CCCCCC>GLIDING DRAINS STAMINA; OPENING DRAINS EXTRA ").Append(FormatNumber(val6.extraOpeningCost * 100f)).Append("% STAMINA\n");
			}
			WarpOnThrow val7 = default(WarpOnThrow);
			if (gameObject.TryGetComponent<WarpOnThrow>(ref val7))
			{
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("CHARGED THROW</color> WARPS YOU TO WHERE IT LANDS\n\n");
				stringBuilder.Append("<#CCCCCC>REQUIRES SPEED > ").Append(FormatNumber(val7.minVelocity)).Append(", TIME BETWEEN ")
					.Append(FormatSeconds(val7.minTime))
					.Append(" AND ")
					.Append(FormatSeconds(val7.maxTime))
					.Append("</color>\n\n");
			}
			Action_Antizooka component;
			bool flag = TryGetDeepComponent<Action_Antizooka>(gameObject, out component);
			Action_RaycastSpawnSomething component2;
			bool flag2 = TryGetDeepComponent<Action_RaycastSpawnSomething>(gameObject, out component2);
			if (flag || flag2 || MatchesAnyItemIdentifier(currentItem, gameObject, "ANTI-ZOOKA", "VOIDLAUNCHER", "ANTI-ZOOKA", "VOIDLAUNCHER"))
			{
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("FIRES AN ANTI-GRAVITY SHELL; CREATES AN ANTI-GRAV FIELD WHERE IT LANDS</color>\n\n");
				if (flag2)
				{
					stringBuilder.Append("<#CCCCCC>MAX RANGE ").Append(FormatNumber(component2.maxDistance * CharacterStats.unitsToMeters)).Append(" m</color>\n\n");
					if ((Object)(object)component2.prefabToSpawn != (Object)null && TryGetDeepComponent<AntiSphere>(component2.prefabToSpawn, out AntiSphere component3))
					{
						if (TryGetDeepComponent<RemoveAfterSeconds>(component2.prefabToSpawn, out RemoveAfterSeconds component4))
						{
							stringBuilder.Append("<#CCCCCC>FIELD LASTS ").Append(FormatSeconds(component4.seconds)).Append("</color>\n\n");
						}
						stringBuilder.Append("<#CCCCCC>LIFTS PLAYERS AND GROUND ITEMS; PULLS YOU IN ON ENTRY, BOUNCES YOU OUT ON EXIT</color>\n\n");
						if (component3.staminaGainRate > 0f)
						{
							stringBuilder.Append(effectColors["Extra Stamina"]).Append("RECOVERS ").Append(FormatPercentAmount(component3.staminaGainRate))
								.Append("% STAMINA</color>\n\n");
						}
					}
				}
				else
				{
					stringBuilder.Append("<#CCCCCC>CHARGES THEN FIRES; TRIGGERS ON TERRAIN HIT OR MAX RANGE</color>\n\n");
				}
			}
			Balloon val8 = default(Balloon);
			if (gameObject.TryGetComponent<Balloon>(ref val8))
			{
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("ATTACHES TO YOUR BODY FOR EXTRA BUOYANCY AND JUMP</color>\n\n");
				stringBuilder.Append("<#CCCCCC>").Append(val8.isBunch ? "A BUNCH BINDS 3 BALLOONS AT ONCE" : "A SINGLE BALLOON BINDS ONLY 1 BALLOON").Append("</color>\n\n");
			}
			Beehive val9 = default(Beehive);
			if (gameObject.TryGetComponent<Beehive>(ref val9))
			{
				stringBuilder.Append(effectColors["Poison"]).Append("SPAWNS A SWARM</color> THAT GUARDS THE HIVE\n\n");
				stringBuilder.Append("<#CCCCCC>IF THE HIVE IS STOLEN OR DESTROYED, THE SWARM ENRAGES AND SCATTERS</color>\n\n");
			}
			RopeTier val10 = default(RopeTier);
			if (gameObject.TryGetComponent<RopeTier>(ref val10))
			{
			}
			Snowball val11 = default(Snowball);
			ItemScaleSyncer val12 = default(ItemScaleSyncer);
			if (gameObject.TryGetComponent<Snowball>(ref val11))
			{
				ItemScaleSyncer component5 = gameObject.GetComponent<ItemScaleSyncer>();
				float value3 = (((Object)component5 != (Object)null) ? component5.currentScale : 1f);
				stringBuilder.Append(effectColors["Hunger"]).Append("ROLLS BIGGER</color> IN SNOW OR ON ICE\n\n");
				stringBuilder.Append("<#CCCCCC>CURRENT SIZE: ").Append(FormatNumber(value3)).Append("x, BEYOND ")
					.Append(FormatNumber(val11.maxScaleToPickUp))
					.Append("x IT CAN'T BE PICKED UP</color>\n\n");
				stringBuilder.Append("<#CCCCCC>MAX ~").Append(FormatNumber(val11.maxCarriedScale)).Append("x</color>\n\n");
			}
			else if (gameObject.TryGetComponent<ItemScaleSyncer>(ref val12))
			{
				stringBuilder.Append("<#CCCCCC>CURRENT SIZE: ").Append(FormatNumber(val12.currentScale)).Append("x</color>\n\n");
			}
			KnockOutPlayerOnImpact val13 = default(KnockOutPlayerOnImpact);
			if (gameObject.TryGetComponent<KnockOutPlayerOnImpact>(ref val13))
			{
				stringBuilder.Append(effectColors["Injury"]).Append("CHARGED THROW</color> KNOCKS DOWN THE TARGET AND DEALS DAMAGE\n\n");
				stringBuilder.Append("<#CCCCCC>THRESHOLD SPEED ").Append(FormatNumber(val13.knockoutVelocity)).Append(", DAMAGE ")
					.Append(FormatNumber(val13.damage))
					.Append("</color>\n\n");
			}
			EventOnItemCollision val14 = default(EventOnItemCollision);
			if (gameObject.TryGetComponent<EventOnItemCollision>(ref val14))
			{
			}
			Action_CloneSelectedItem val15 = default(Action_CloneSelectedItem);
			if (gameObject.TryGetComponent<Action_CloneSelectedItem>(ref val15))
			{
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("AIM AT AN ITEM TO COPY IT</color>\n\n");
				stringBuilder.Append("<#CCCCCC>RANGE ").Append(FormatNumber(val15.range)).Append(" m; CANNOT COPY AMULETS OR UNCOPYABLE ITEMS</color>\n\n");
				stringBuilder.Append(effectColors["Petrify"]).Append("COPYING A NORMAL ITEM GRANTS ").Append(val15.petrify)
					.Append(" PETRIFY; MYSTICAL ITEMS GRANT ")
					.Append(val15.petrifyMystical)
					.Append(" PETRIFY</color>\n");
			}
			Action_SuperJumpAmulet val16 = default(Action_SuperJumpAmulet);
			if (gameObject.TryGetComponent<Action_SuperJumpAmulet>(ref val16))
			{
				if (((Action_ApplyAffliction)val16).affliction != null)
				{
					stringBuilder.Append(ProcessAffliction(((Action_ApplyAffliction)val16).affliction));
				}
				if (((Action_ApplyAffliction)val16).extraAfflictions != null)
				{
					for (int i = 0; i < ((Action_ApplyAffliction)val16).extraAfflictions.Length; i++)
					{
						stringBuilder.Append(ProcessAffliction(((Action_ApplyAffliction)val16).extraAfflictions[i]));
					}
				}
				if (val16.petrifyPerUse > 0f)
				{
					stringBuilder.Append(effectColors["Petrify"]).Append("GAIN AN EXTRA ").Append(FormatPetrifyAmount(val16.petrifyPerUse))
						.Append(" PETRIFY</color>\n\n");
				}
			}
			DoubleJumpAmulet val17 = default(DoubleJumpAmulet);
			if (gameObject.TryGetComponent<DoubleJumpAmulet>(ref val17))
			{
				stringBuilder.Append(effectColors["Extra Stamina"]).Append("GAIN EXTRA JUMPS</color>\n\n");
				stringBuilder.Append(effectColors["Petrify"]).Append("EACH DOUBLE JUMP GRANTS ").Append(val17.petrifyPerJump)
					.Append(" PETRIFY</color>\n\n");
			}
			Action_HealingGem component6;
			bool flag3 = TryGetDeepComponent<Action_HealingGem>(gameObject, out component6);
			HealingAmulet component7;
			if (flag3 || MatchesAnyItemIdentifier(currentItem, gameObject, "SCOUT'S TENACITY", "SCOUT'S TENACITY", "SCOUT’S TENACITY", "SCOUT'S TENACITY", "AMULET_HEALING"))
			{
				float num = ((flag3 && component6.healingAffliction != null) ? component6.healingAffliction.maxHealing : 0f);
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("HEALS INJURY, SPORES, POISON, COLD, HEAT, DROWSY</color> AT ONCE\n\n");
				if (num > 0f)
				{
					stringBuilder.Append("<#CCCCCC>MAX TOTAL HEAL ").Append(FormatPercentAmount(num)).Append("%</color>\n\n");
				}
				Character tipCharacter = GetTipCharacter(currentItem);
				if (flag3 && (Object)tipCharacter != (Object)null)
				{
					string breakdown;
					float num2 = CalculateHealAllPotential(tipCharacter, num, out breakdown);
					stringBuilder.Append("<#CCCCCC>CURRENT USE HEALS ").Append(FormatPercentAmount(num2)).Append("%</color>\n\n");
					if (!string.IsNullOrEmpty(breakdown))
					{
						stringBuilder.Append("<#CCCCCC>CURRENTLY CURES:</color>").Append(breakdown).Append("\n\n");
					}
					float value4 = Mathf.Clamp(num2 * component6.healingToPetrifyRatio, component6.minPetrify, component6.maxPetrify);
					stringBuilder.Append(effectColors["Petrify"]).Append("DOWNSIDE: USING ON SELF GRANTS ").Append(FormatPetrifyAmount(value4))
						.Append(" PETRIFY</color>\n\n");
				}
				else if (flag3)
				{
					stringBuilder.Append(effectColors["Petrify"]).Append("DOWNSIDE: GAIN PETRIFY BY ACTUAL HEAL, RANGE ").Append(FormatPetrifyAmount(component6.minPetrify))
						.Append(" - ")
						.Append(FormatPetrifyAmount(component6.maxPetrify))
						.Append("</color>\n\n");
				}
				if (flag3 && component6.invincibilityAffliction != null)
				{
					stringBuilder.Append(ProcessAffliction(component6.invincibilityAffliction));
				}
			}
			else if (TryGetDeepComponent<HealingAmulet>(gameObject, out component7))
			{
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("WHILE CARRIED, CONTINUOUSLY HEALS NEGATIVE STATUS</color>\n\n");
				stringBuilder.Append("<#CCCCCC>HEALS 2.5% OF ONE STATUS EVERY 0.5 s: INJURY, POISON, SPORES, COLD, DROWSY, HEAT</color>\n\n");
				stringBuilder.Append(effectColors["Petrify"]).Append("DOWNSIDE: EACH HEAL GRANTS 1 PETRIFY</color>\n\n");
			}
			InfiniteStamAmulet component8;
			bool flag4 = TryGetDeepComponent<InfiniteStamAmulet>(gameObject, out component8);
			Action_ApplyInfiniteStamina component9;
			bool flag5 = TryGetDeepComponent<Action_ApplyInfiniteStamina>(gameObject, out component9);
			if (flag4 || MatchesAnyItemIdentifier(currentItem, gameObject, "SCOUT'S AMBITION", "SCOUT'S AMBITION", "SCOUT'S AMBITION", "SCOUT’S AMBITION", "AMULET_INFINITESTAM"))
			{
				if (flag5 && component9.buffTime > 0f)
				{
					stringBuilder.Append(effectColors["Extra Stamina"]).Append("GAIN ").Append(FormatSeconds(component9.buffTime))
						.Append(" OF INFINITE STAMINA</color>\n\n");
				}
				else
				{
					stringBuilder.Append(effectColors["Extra Stamina"]).Append("PLAYERS IN RANGE GAIN INFINITE STAMINA</color>\n\n");
				}
				if (flag4)
				{
					stringBuilder.Append("<#CCCCCC>RADIUS ").Append(FormatNumber(component8.radius)).Append(" m</color>\n\n");
				}
				float oneTimePetrifyFromActions = GetOneTimePetrifyFromActions(gameObject);
				if (oneTimePetrifyFromActions > 0f)
				{
					stringBuilder.Append(effectColors["Petrify"]).Append("DOWNSIDE: USING/ACTIVATING GRANTS AN INSTANT ").Append(FormatPetrifyAmount(oneTimePetrifyFromActions))
						.Append(" PETRIFY</color>\n\n");
				}
			}
			RitualDaggerFeedBehavior component10;
			bool flag6 = TryGetDeepComponent<RitualDaggerFeedBehavior>(gameObject, out component10);
			if (flag6 || TryGetDeepComponent<Action_SacrificeFriend>(gameObject, out Action_SacrificeFriend _) || MatchesAnyItemIdentifier(currentItem, gameObject, "RITUAL DAGGER", "RITUAL DAGGER", "RITUAL DAGGER", "RITUALDAGGER"))
			{
				stringBuilder.Append(effectColors["Injury"]).Append("SACRIFICES THE TARGET: THEY INSTANTLY PETRIFY AND DIE</color>\n\n");
				if (flag6)
				{
					stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("ALL OTHER LIVE PLAYERS CLEAR CURABLE STATUS</color>\n\n");
					stringBuilder.Append(effectColors["Extra Stamina"]).Append("GAIN ").Append(FormatPercentAmount(component10.bonusStamina))
						.Append("% EXTRA STAMINA</color>\n\n");
					if (component10.infiniteStaminaTime > 0f)
					{
						stringBuilder.Append(effectColors["Extra Stamina"]).Append("GAIN ").Append(FormatSeconds(component10.infiniteStaminaTime))
							.Append(" OF INFINITE STAMINA</color>\n\n");
					}
					Character tipCharacter2 = GetTipCharacter(currentItem);
					if ((Object)tipCharacter2 != (Object)null)
					{
						stringBuilder.Append("<#CCCCCC>IF YOU'RE NOT THE SACRIFICE, CURES ").Append(FormatPercentAmount(CalculateCurableStatusSum(tipCharacter2))).Append("% CURABLE STATUS</color>\n\n");
					}
					stringBuilder.Append("<#CCCCCC>DOES NOT CLEAR UNCURABLE STATUS LIKE CURSE, PETRIFY, WEIGHT, THORNS, ARROW</color>\n\n");
				}
			}
			if (!TryGetDeepComponent<Backpack>(gameObject, out Backpack _) && MatchesAnyItemIdentifier(currentItem, gameObject, "FANNY PACK", "FANNY PACK", "FANNY PACK", "FANNYPACK"))
			{
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("PROVIDES EXTRA ITEM SLOTS</color>\n\n");
				stringBuilder.Append("<#CCCCCC>STORES ITEMS LIKE A BACKPACK</color>\n\n");
			}
			if (TryGetDeepComponent<ScoutsHonor>(gameObject, out ScoutsHonor _) || MatchesAnyItemIdentifier(currentItem, gameObject, "SCOUT'S HONOR", "SCOUT'S HONOR", "SCOUT’S HONOR", "SCOUT'S HONOR", "SCOUTSHONOR"))
			{
				stringBuilder.Append(effectColors["ItemInfoDisplayPositive"]).Append("REWARD ITEM AFTER COMPLETING A SCOUT STATUE</color>\n\n");
			}
			return stringBuilder.ToString();
		}

		private static void ProcessItemGameObject()
		{
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Expected O, but got Unknown
			//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Expected O, but got Unknown
			//IL_0391: Unknown result type (might be due to invalid IL or missing references)
			//IL_0398: Expected O, but got Unknown
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e0: Expected O, but got Unknown
			//IL_041e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Expected O, but got Unknown
			//IL_0538: Unknown result type (might be due to invalid IL or missing references)
			//IL_053f: Expected O, but got Unknown
			//IL_0584: Unknown result type (might be due to invalid IL or missing references)
			//IL_058b: Expected O, but got Unknown
			//IL_0635: Unknown result type (might be due to invalid IL or missing references)
			//IL_063c: Expected O, but got Unknown
			//IL_0707: Unknown result type (might be due to invalid IL or missing references)
			//IL_070e: Expected O, but got Unknown
			//IL_0746: Unknown result type (might be due to invalid IL or missing references)
			//IL_074d: Expected O, but got Unknown
			//IL_0833: Unknown result type (might be due to invalid IL or missing references)
			//IL_083a: Expected O, but got Unknown
			//IL_0b35: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b3c: Expected O, but got Unknown
			//IL_0cb1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cb8: Expected O, but got Unknown
			//IL_0dc9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dd0: Expected O, but got Unknown
			//IL_0d40: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d47: Expected O, but got Unknown
			//IL_0fdd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fe4: Expected O, but got Unknown
			//IL_1050: Unknown result type (might be due to invalid IL or missing references)
			//IL_1057: Expected O, but got Unknown
			//IL_0bda: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bdf: Unknown result type (might be due to invalid IL or missing references)
			//IL_11b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_11bb: Expected O, but got Unknown
			//IL_0f81: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f8c: Expected O, but got Unknown
			//IL_1384: Unknown result type (might be due to invalid IL or missing references)
			//IL_138b: Expected O, but got Unknown
			//IL_1457: Unknown result type (might be due to invalid IL or missing references)
			//IL_145e: Expected O, but got Unknown
			//IL_15c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_15c9: Expected O, but got Unknown
			//IL_173e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1745: Expected O, but got Unknown
			//IL_17c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_17ce: Expected O, but got Unknown
			//IL_187b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1882: Expected O, but got Unknown
			//IL_18c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_18d0: Expected O, but got Unknown
			//IL_1911: Unknown result type (might be due to invalid IL or missing references)
			//IL_191c: Expected O, but got Unknown
			//IL_1b5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b64: Expected O, but got Unknown
			//IL_1cd0: Unknown result type (might be due to invalid IL or missing references)
			//IL_1cd7: Expected O, but got Unknown
			//IL_1e34: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e3b: Expected O, but got Unknown
			//IL_2007: Unknown result type (might be due to invalid IL or missing references)
			//IL_200e: Expected O, but got Unknown
			//IL_20bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_20c6: Expected O, but got Unknown
			//IL_22a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_22b0: Expected O, but got Unknown
			//IL_27ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_27f4: Expected O, but got Unknown
			//IL_22d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_22dd: Expected O, but got Unknown
			//IL_28a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_28ad: Expected O, but got Unknown
			//IL_28af: Unknown result type (might be due to invalid IL or missing references)
			//IL_28b5: Invalid comparison between Unknown and I4
			//IL_2b7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b85: Expected O, but got Unknown
			//IL_2b8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b97: Expected O, but got Unknown
			//IL_231b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2326: Expected O, but got Unknown
			//IL_2fe7: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ff0: Expected O, but got Unknown
			//IL_3187: Unknown result type (might be due to invalid IL or missing references)
			//IL_3190: Expected O, but got Unknown
			//IL_3378: Unknown result type (might be due to invalid IL or missing references)
			//IL_3381: Expected O, but got Unknown
			//IL_33c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_33cf: Expected O, but got Unknown
			Item currentItem = Character.observedCharacter.data.currentItem;
			GameObject gameObject = ((Component)currentItem).gameObject;
			Component[] components = gameObject.GetComponents(typeof(Component));
			Dictionary<Type, List<ComponentEffectInfo>> dictionary = new Dictionary<Type, List<ComponentEffectInfo>>();
			bool flag = false;
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			((TMP_Text)itemInfoDisplayTextMesh).text = "";
			text2 = ((((Object)currentItem).name == "Backpack(Clone)") ? (text2 + effectColors["Weight"] + "0 WEIGHT</color>") : ((Ascents.itemWeightModifier <= 0) ? (text2 + effectColors["Weight"] + ((float)currentItem.carryWeight * 2.5f).ToString("F1").Replace(".0", "") + " WEIGHT</color>") : (text2 + effectColors["Weight"] + ((float)(currentItem.carryWeight + Ascents.itemWeightModifier) * 2.5f).ToString("F1").Replace(".0", "") + " WEIGHT</color>")));
			if (((Object)gameObject).name.Equals("Bugle(Clone)"))
			{
				TextMeshProUGUI val = itemInfoDisplayTextMesh;
				((TMP_Text)val).text = ((TMP_Text)val).text + "USE THE " + effectColors["Hunger"] + "BUGLE</color> TO PLAY SOME LOVELY NOTES\n\n";
			}
			else if (((Object)gameObject).name.Equals("Pirate Compass(Clone)"))
			{
				TextMeshProUGUI val2 = itemInfoDisplayTextMesh;
				((TMP_Text)val2).text = ((TMP_Text)val2).text + effectColors["Injury"] + "POINTS</color> TO THE NEAREST LUGGAGE\n\n";
			}
			else if (((Object)gameObject).name.Equals("Compass(Clone)"))
			{
				TextMeshProUGUI val3 = itemInfoDisplayTextMesh;
				((TMP_Text)val3).text = ((TMP_Text)val3).text + effectColors["Injury"] + "POINTS</color> TO THE PEAK IN THE NORTH\n\n";
			}
			else if (((Object)gameObject).name.Equals("Shell Big(Clone)"))
			{
				TextMeshProUGUI val4 = itemInfoDisplayTextMesh;
				((TMP_Text)val4).text = ((TMP_Text)val4).text + effectColors["Hunger"] + "A GREAT TOOL</color> FOR OPENING COCONUTS\n\n";
			}
			Character val5 = Character.localCharacter;
			if (val5.data.fullyPassedOut)
			{
				val5 = MainCameraMovement.specCharacter;
			}
			bool flag2 = false;
			TextMeshProUGUI val17;
			for (int i = 0; i < components.Length; i++)
			{
				Type type = ((object)components[i]).GetType();
				Component val6 = components[i];
				Behaviour val7 = (Behaviour)((val6 is Behaviour) ? val6 : null);
				if ((Object)(object)val7 != (Object)null && !val7.enabled)
				{
					continue;
				}
				if (type == typeof(ItemUseFeedback))
				{
					ItemUseFeedback val8 = (ItemUseFeedback)val6;
					if (val8.useAnimation.Equals("Eat") || ((Component)val8).tag == "BookOfBones" || val8.useAnimation.Equals("Drink") || val8.useAnimation.Equals("Heal"))
					{
						flag = true;
					}
				}
				else if (type == typeof(Action_Consume))
				{
					flag = true;
				}
				else if (type == typeof(Action_RestoreHunger))
				{
					Action_RestoreHunger val9 = (Action_RestoreHunger)val6;
					text += ProcessEffect(val9.restorationAmount * -1f, "Hunger");
				}
				else if (type == typeof(Action_GiveExtraStamina))
				{
					Action_GiveExtraStamina val10 = (Action_GiveExtraStamina)val6;
					text += ProcessEffect(val10.amount, "Extra Stamina");
				}
				else if (type == typeof(Action_InflictPoison))
				{
					Action_InflictPoison val11 = (Action_InflictPoison)val6;
					text = ((!(val11.delay > 0f)) ? (text + "STARTS NOW, ") : (text + val11.delay.ToString("F1").Replace(".0", "") + " s DELAY, "));
					text = text + "OVER " + val11.inflictionTime.ToString("F1").Replace(".0", "") + " s, TOTAL ";
					text = text + effectColors["Poison"] + (val11.poisonPerSecond * val11.inflictionTime * 100f).ToString("F1").Replace(".0", "") + " " + GetEffectChineseName("Poison") + "</color>\n\n";
				}
				else if (type == typeof(Action_AddOrRemoveThorns))
				{
					Action_AddOrRemoveThorns val12 = (Action_AddOrRemoveThorns)val6;
					text += ProcessEffect((float)val12.thornCount * 0.05f, "Thorns");
				}
				else if (type == typeof(Action_ModifyStatus))
				{
					Action_ModifyStatus val13 = (Action_ModifyStatus)val6;
					if ((val13.ifSkeleton && val5.data.isSkeleton) || flag2)
					{
						text += ProcessEffect(val13.changeAmount, ((object)(STATUSTYPE)(ref val13.statusType)/*cast due to .constrained prefix*/).ToString());
					}
					else if (!val13.ifSkeleton)
					{
						text += ProcessEffect(val13.changeAmount, ((object)(STATUSTYPE)(ref val13.statusType)/*cast due to .constrained prefix*/).ToString());
					}
				}
				else if (type == typeof(Action_ApplyMassAffliction))
				{
					Action_ApplyMassAffliction val14 = (Action_ApplyMassAffliction)val6;
					text5 += $"<#CCCCCC>PLAYERS WITHIN {val14.radius}m GAIN:</color>\n\n";
					text5 += ProcessAffliction(((Action_ApplyAffliction)val14).affliction);
					if (((Action_ApplyAffliction)val14).extraAfflictions.Length == 0)
					{
						continue;
					}
					for (int j = 0; j < ((Action_ApplyAffliction)val14).extraAfflictions.Length; j++)
					{
						if (text5.EndsWith('\n'))
						{
							text5 = text5.Remove(text5.Length - 1);
						}
						text5 = text5 + "\n" + ProcessAffliction(((Action_ApplyAffliction)val14).extraAfflictions[j]);
					}
				}
				else if (type == typeof(Action_ApplyAffliction))
				{
					Action_ApplyAffliction val15 = (Action_ApplyAffliction)val6;
					text5 += ProcessAffliction(val15.affliction);
				}
				else if (type == typeof(Mandrake))
				{
					Mandrake val16 = (Mandrake)val6;
					AOE component = ((Component)val16).GetComponent<AOE>();
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + "WHEN YOU WAKE " + effectColors["ItemInfoDisplayNegative"] + "MANDRAKE</color>, IT SINGS A LULLABY\n\nNEARBY " + (component.range * CharacterStats.unitsToMeters).ToString("F1").Replace(".0", "") + " m: " + effectColors[((object)(STATUSTYPE)(ref component.statusType)/*cast due to .constrained prefix*/).ToString()] + GetEffectChineseName(((object)(STATUSTYPE)(ref component.statusType)/*cast due to .constrained prefix*/).ToString()) + "</color> EFFECT\n\n";
				}
				else if (type == typeof(Action_Numb))
				{
					Action_Numb val18 = (Action_Numb)val6;
					text = text + "GAIN " + effectColors["ItemInfoDisplayNegative"] + "NUMB</color> FOR " + val18.numbAmount.ToString("F1").Replace(".0", "") + " s</color>\n\n";
				}
				else if (type == typeof(Action_BecomeSkeleton))
				{
					if (!val5.data.isSkeleton)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append(effectColors["ItemInfoDisplayPositive"] + "USE TO BECOME</color> A SKELETON " + effectColors["ItemInfoDisplayPositive"] + "WITH THESE EFFECTS</color>\n\n");
						stringBuilder.Append(effectColors["Shield"] + "IMMUNE</color> TO EVERYTHING EXCEPT " + effectColors["Injury"] + "INJURY</color>, ARROW, FLYTRAP, WEB, WEIGHT, " + effectColors["Curse"] + "CURSE AND PETRIFY</color>; BUT YOU TAKE " + effectColors["Injury"] + "8x INJURY</color>\n\n");
						stringBuilder.Append("WHILE YOU ARE " + effectColors["Injury"] + "DOWNED</color>, YOU INSTANTLY " + effectColors["Curse"] + "DIE</color>\n\n");
						stringBuilder.Append(effectColors["Thorns"] + "CACTUS BALLS</color> HAVE NO EFFECT ON YOU\n\n");
						stringBuilder.Append(effectColors["Cold"] + "COLD MIST</color> DEALS " + effectColors["Injury"] + "0.125x INJURY</color> INSTEAD OF " + effectColors["Cold"] + "COLD</color>\n\n");
						stringBuilder.Append(effectColors["Injury"] + "ZOMBIE BITES DEAL YOU " + effectColors["Injury"] + "DOUBLE INJURY (20)</color>\n\n");
						TextMeshProUGUI val19 = itemInfoDisplayTextMesh;
						((TMP_Text)val19).text = ((TMP_Text)val19).text + stringBuilder.ToString();
						flag2 = true;
					}
					else
					{
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["ItemInfoDisplayPositive"] + "USE TO TURN BACK</color> INTO A SCOUT" + effectColors["ItemInfoDisplayPositive"] + "</color>\n\n";
					}
				}
				else if (type == typeof(Action_ClearAllStatus))
				{
					Action_ClearAllStatus val20 = (Action_ClearAllStatus)val6;
					TextMeshProUGUI val21 = itemInfoDisplayTextMesh;
					((TMP_Text)val21).text = ((TMP_Text)val21).text + effectColors["ItemInfoDisplayPositive"] + "CLEAR ALL STATUS</color>";
					if (val20.excludeCurse)
					{
						TextMeshProUGUI val22 = itemInfoDisplayTextMesh;
						((TMP_Text)val22).text = ((TMP_Text)val22).text + " EXCEPT " + effectColors["Curse"] + "CURSE AND PETRIFY</color>";
					}
					if (val20.otherExclusions.Count > 0)
					{
						foreach (STATUSTYPE otherExclusion in val20.otherExclusions)
						{
							STATUSTYPE current = otherExclusion;
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + "、" + effectColors[((object)current).ToString()] + ((object)current).ToString().ToUpper() + "</color>";
						}
					}
					((TMP_Text)itemInfoDisplayTextMesh).text = ((TMP_Text)itemInfoDisplayTextMesh).text.Replace("、<#E13542>CRAB</color>", "") + "\n\n";
				}
				else if (type == typeof(Action_ConsumeAndSpawn))
				{
					Action_ConsumeAndSpawn val23 = (Action_ConsumeAndSpawn)val6;
					if (((object)val23.itemToSpawn).ToString().Contains("Peel"))
					{
						TextMeshProUGUI val24 = itemInfoDisplayTextMesh;
						((TMP_Text)val24).text = ((TMP_Text)val24).text + "<#CCCCCC>LEAVES A PEEL AFTER EATING</color>\n\n";
					}
				}
				else if (type == typeof(Action_ReduceUses))
				{
					if (currentItem.data.data.ContainsKey((DataEntryKey)2))
					{
						OptionableIntItemData val25 = (OptionableIntItemData)currentItem.data.data[(DataEntryKey)2];
						if (val25.HasData)
						{
							text3 = ((val25.Value > 1) ? (text3 + "   ONLY " + val25.Value + " USES") : ((val25.Value != 1) ? (text3 + "   NO USES LEFT") : (text3 + "   ONLY 1 USE LEFT")));
						}
					}
				}
				else if (type == typeof(Lantern))
				{
					Lantern val26 = (Lantern)val6;
					if (((Object)gameObject).name.Equals("Torch(Clone)"))
					{
						TextMeshProUGUI val27 = itemInfoDisplayTextMesh;
						((TMP_Text)val27).text = ((TMP_Text)val27).text + "CAN BE LIT\n\n";
					}
					else
					{
						text5 += "<#CCCCCC>WHEN LIT, NEARBY PLAYERS GAIN:</color>\n\n";
					}
					if (((Object)gameObject).name.Equals("Lantern_Faerie(Clone)"))
					{
						StatusField component2 = ((Component)gameObject.transform.Find("FaerieLantern/Light/Heat")).GetComponent<StatusField>();
						text5 = text5 + "<#CCCCCC>STILL USABLE: " + val26.fuel.ToString("F1").Replace(".0", "") + " s</color>\n\n";
						text5 += ProcessEffectPerSecond(((StatusFieldBase)component2).statusAmountPerSecond, ((object)(STATUSTYPE)(ref ((StatusFieldBase)component2).statusType)/*cast due to .constrained prefix*/).ToString());
						foreach (StatusFieldStatus additionalStatus in ((StatusFieldBase)component2).additionalStatuses)
						{
							text5 += ProcessEffectPerSecond(additionalStatus.statusAmountPerSecond, ((object)(STATUSTYPE)(ref additionalStatus.statusType)/*cast due to .constrained prefix*/).ToString());
						}
					}
					else if (((Object)gameObject).name.Equals("Lantern(Clone)"))
					{
						Transform val28 = gameObject.transform.Find("GasLantern/Light/Heat");
						StatusField val29 = (((Object)(object)val28 != (Object)null) ? ((Component)val28).GetComponent<StatusField>() : null);
						text5 = text5 + "<#CCCCCC>STILL USABLE: " + val26.fuel.ToString("F1").Replace(".0", "") + " s</color>\n\n";
						if ((Object)val29 != (Object)null)
						{
							text5 += ProcessEffectPerSecond(((StatusFieldBase)val29).statusAmountPerSecond, ((object)(STATUSTYPE)(ref ((StatusFieldBase)val29).statusType)/*cast due to .constrained prefix*/).ToString());
						}
					}
				}
				else if (type == typeof(Action_RaycastDart))
				{
					Action_RaycastDart val30 = (Action_RaycastDart)val6;
					flag = true;
					text5 += "<#CCCCCC>FIRES A DART APPLYING THESE EFFECTS TO THE HIT PLAYER:</color>\n\n";
					for (int k = 0; k < val30.afflictionsOnHit.Length; k++)
					{
						text5 += ProcessAffliction(val30.afflictionsOnHit[k]);
					}
				}
				else if (type == typeof(MagicBugle))
				{
					MagicBugle val31 = (MagicBugle)val6;
					TextMeshProUGUI val32 = itemInfoDisplayTextMesh;
					((TMP_Text)val32).text = ((TMP_Text)val32).text + "<#CCCCCC>STILL USABLE: " + val31.fuel.ToString("F1").Replace(".0", "") + " s</color>\n\n";
					TextMeshProUGUI val33 = itemInfoDisplayTextMesh;
					((TMP_Text)val33).text = ((TMP_Text)val33).text + "STARTUP COSTS " + val31.initialTootCost.ToString("F1") + " s\n\n";
					TextMeshProUGUI val34 = itemInfoDisplayTextMesh;
					((TMP_Text)val34).text = ((TMP_Text)val34).text + effectColors["ItemInfoDisplayPositive"] + "WHEN YOU PLAY THE BUGLE</color>\n\n";
				}
				else if (type == typeof(ClimbingSpikeComponent))
				{
					TextMeshProUGUI val35 = itemInfoDisplayTextMesh;
					((TMP_Text)val35).text = ((TMP_Text)val35).text + "PLACES A GRABBABLE PITON; ON IT YOU CAN " + effectColors["Extra Stamina"] + "RESTORE STAMINA</color>\n\n";
				}
				else if (type == typeof(Action_Flare))
				{
					TextMeshProUGUI val36 = itemInfoDisplayTextMesh;
					((TMP_Text)val36).text = ((TMP_Text)val36).text + "CAN BE LIT\n\n";
				}
				else if (type == typeof(Backpack))
				{
					Backpack val37 = (Backpack)val6;
					if (MatchesAnyItemIdentifier(currentItem, gameObject, "FANNY PACK", "FANNY PACK", "FANNY PACK", "FANNYPACK"))
					{
						TextMeshProUGUI val38 = itemInfoDisplayTextMesh;
						((TMP_Text)val38).text = ((TMP_Text)val38).text + effectColors["ItemInfoDisplayPositive"] + "PROVIDES EXTRA ITEM SLOTS</color>\n\n";
						TextMeshProUGUI val39 = itemInfoDisplayTextMesh;
						((TMP_Text)val39).text = ((TMP_Text)val39).text + "<#CCCCCC>CAPACITY " + val37.slotCount + " SLOTS</color>\n\n";
						int backpackFilledSlotCountSafe = GetBackpackFilledSlotCountSafe(val37);
						if (backpackFilledSlotCountSafe >= 0)
						{
							TextMeshProUGUI val40 = itemInfoDisplayTextMesh;
							((TMP_Text)val40).text = ((TMP_Text)val40).text + "<#CCCCCC>CURRENTLY HOLDING " + backpackFilledSlotCountSafe + " / " + val37.slotCount + "</color>\n\n";
						}
					}
					else if (EasyBackpack)
					{
						TextMeshProUGUI val41 = itemInfoDisplayTextMesh;
						((TMP_Text)val41).text = ((TMP_Text)val41).text + "PRESS B TO OPEN THE BACKPACK AND STORE ITEMS\n\n";
					}
					else
					{
						TextMeshProUGUI val42 = itemInfoDisplayTextMesh;
						((TMP_Text)val42).text = ((TMP_Text)val42).text + "PLACE THE BACKPACK DOWN TO STORE ITEMS\n\n";
					}
				}
				else if (type == typeof(BananaPeel))
				{
					TextMeshProUGUI val43 = itemInfoDisplayTextMesh;
					((TMP_Text)val43).text = ((TMP_Text)val43).text + effectColors["Hunger"] + "SLIP</color> WHEN STEPPED ON\n\n";
				}
				else if (type == typeof(Constructable))
				{
					Constructable val44 = (Constructable)val6;
					if (((Object)val44.constructedPrefab).name.Equals("PortableStovetop_Placed"))
					{
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + "PLACES A PORTABLE STOVE OFFERING " + effectColors["Injury"] + "COOKING</color> FOR " + val44.constructedPrefab.GetComponent<Campfire>().burnsFor + " s\n\n";
					}
					else
					{
						TextMeshProUGUI val45 = itemInfoDisplayTextMesh;
						((TMP_Text)val45).text = ((TMP_Text)val45).text + "CAN BE PLACED\n\n";
					}
				}
				else if (type == typeof(RopeSpool))
				{
					RopeSpool val46 = (RopeSpool)val6;
					if (val46.isAntiRope)
					{
						TextMeshProUGUI val47 = itemInfoDisplayTextMesh;
						((TMP_Text)val47).text = ((TMP_Text)val47).text + "PLACES AN UPWARD ANTI-GRAVITY ROPE\n\n";
					}
					else
					{
						TextMeshProUGUI val48 = itemInfoDisplayTextMesh;
						((TMP_Text)val48).text = ((TMP_Text)val48).text + "PLACES A ROPE\n\n";
					}
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + "LENGTH FROM " + (val46.minSegments / 4f).ToString("F2").Replace(".0", "") + " TO " + ((float)Rope.MaxSegments / 4f).ToString("F1").Replace(".0", "") + " m\n\n";
					if (configForceUpdateTime.Value <= 1f)
					{
						text3 = text3 + "   REMAINING " + (val46.RopeFuel / 4f).ToString("F2").Replace(".00", "") + " m";
					}
				}
				else if (type == typeof(RopeShooter))
				{
					RopeShooter val49 = (RopeShooter)val6;
					bool flag3 = (Object)(object)val49.ropeAnchorWithRopePref != (Object)null && ((Object)val49.ropeAnchorWithRopePref).name.IndexOf("Anti", StringComparison.OrdinalIgnoreCase) >= 0;
					if (MatchesAnyItemIdentifier(currentItem, gameObject, "ANTI-ROPE CANNON", "ANTI-ROPE CANNON", "ANTI-ROPE CANNON", "ANTI-ROPE"))
					{
						flag3 = true;
					}
					TextMeshProUGUI val50 = itemInfoDisplayTextMesh;
					((TMP_Text)val50).text = ((TMP_Text)val50).text + effectColors["ItemInfoDisplayPositive"] + "FIRES A ROPE ANCHOR</color>\n\n";
					if (flag3)
					{
						TextMeshProUGUI val51 = itemInfoDisplayTextMesh;
						((TMP_Text)val51).text = ((TMP_Text)val51).text + effectColors["Extra Stamina"] + "AN UPWARD-FLOATING ANTI-GRAVITY ROPE</color>\n\n";
					}
					else
					{
						TextMeshProUGUI val52 = itemInfoDisplayTextMesh;
						((TMP_Text)val52).text = ((TMP_Text)val52).text + "A ROPE THAT HANGS DOWN</color>\n\n";
					}
					TextMeshProUGUI val53 = itemInfoDisplayTextMesh;
					((TMP_Text)val53).text = ((TMP_Text)val53).text + "<#CCCCCC>MAX RANGE " + FormatNumber(val49.maxLength) + " m; ROPE LENGTH ~" + FormatNumber(val49.length / 4f) + " m</color>\n\n";
				}
				else if (type == typeof(Antigrav))
				{
					Antigrav val54 = (Antigrav)val6;
					if (val54.intensity != 0f)
					{
						text5 = text5 + effectColors["Injury"] + "WARNING:</color> <#CCCCCC>FLIES AWAY IF DROPPED</color>\n\n";
					}
				}
				else if (type == typeof(Action_Balloon))
				{
					text5 += "CAN BE TIED TO PLAYERS\n\n";
				}
				else if (type == typeof(VineShooter))
				{
					VineShooter val55 = (VineShooter)val6;
					TextMeshProUGUI val56 = itemInfoDisplayTextMesh;
					((TMP_Text)val56).text = ((TMP_Text)val56).text + "SHOOT A CHAIN FROM YOUR POSITION TO WHERE YOU SHOOT\n\nUP TO\n\n" + (val55.maxLength / 1.6666666f).ToString("F1").Replace(".0", "") + " m AWAY\n\n";
				}
				else if (type == typeof(CloudFungus))
				{
					text5 = text5 + effectColors["Hunger"] + "THROW</color> TO DEPLOY A PLATFORM (SPAWNS IN AIR)\n\n";
				}
				else if (type == typeof(ShelfShroom))
				{
					ShelfShroom val57 = (ShelfShroom)val6;
					if (((Object)val57.instantiateOnBreak).name.Equals("HealingPuffShroomSpawn"))
					{
						GameObject instantiateOnBreak = val57.instantiateOnBreak;
						GameObject gameObject2 = ((Component)instantiateOnBreak.transform.Find("VFX_SporeHealingExplo")).gameObject;
						if ((Object)gameObject2 != (Object)null)
						{
							TextMeshProUGUI val58 = itemInfoDisplayTextMesh;
							((TMP_Text)val58).text = ((TMP_Text)val58).text + effectColors["Hunger"] + "DROP</color> TO RELEASE AN AOE OF HEALING SMOKE\n\n";
							RemoveAfterSeconds component3 = gameObject2.GetComponent<RemoveAfterSeconds>();
							float globalDuration = (((Object)component3 != (Object)null) ? component3.seconds : 0f);
							TextMeshProUGUI val59 = itemInfoDisplayTextMesh;
							((TMP_Text)val59).text = ((TMP_Text)val59).text + ProcessGameObjectAndChildrenAOE(gameObject2, globalDuration, addTips: false);
						}
					}
					else if (((Object)val57.instantiateOnBreak).name.Equals("ShelfShroomSpawn"))
					{
						text5 = text5 + effectColors["Hunger"] + "THROW</color> TO DEPLOY A PLATFORM\n\n";
					}
					else if (((Object)val57.instantiateOnBreak).name.Equals("BounceShroomSpawn"))
					{
						text5 = text5 + effectColors["Hunger"] + "THROW</color> TO DEPLOY A BOUNCY PLATFORM\n\n";
					}
				}
				else if (type == typeof(ScoutEffigy))
				{
					TextMeshProUGUI val60 = itemInfoDisplayTextMesh;
					((TMP_Text)val60).text = ((TMP_Text)val60).text + effectColors["Extra Stamina"] + "REVIVES</color> DEAD PLAYERS\n\n";
				}
				else if (type == typeof(Action_Die))
				{
					TextMeshProUGUI val61 = itemInfoDisplayTextMesh;
					((TMP_Text)val61).text = ((TMP_Text)val61).text + "USING IT MAKES YOU " + effectColors["Curse"] + "DIE</color>\n\n";
				}
				else if (type == typeof(Action_SpawnGuidebookPage))
				{
					flag = true;
					TextMeshProUGUI val62 = itemInfoDisplayTextMesh;
					((TMP_Text)val62).text = ((TMP_Text)val62).text + "CAN BE OPENED\n\n";
				}
				else if (type == typeof(Action_Guidebook))
				{
					TextMeshProUGUI val63 = itemInfoDisplayTextMesh;
					((TMP_Text)val63).text = ((TMP_Text)val63).text + "CAN BE READ\n\n";
				}
				else if (type == typeof(Action_CallScoutmaster))
				{
					TextMeshProUGUI val64 = itemInfoDisplayTextMesh;
					((TMP_Text)val64).text = ((TMP_Text)val64).text + effectColors["Injury"] + "BREAKS RULE 0</color> WHEN USED\n\n";
				}
				else if (type == typeof(Action_MoraleBoost))
				{
					Action_MoraleBoost val65 = (Action_MoraleBoost)val6;
					if (val65.boostRadius < 0f)
					{
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + effectColors["Extra Stamina"] + (val65.baselineStaminaBoost * 100f).ToString("F1").Replace(".0", "") + " EXTRA STAMINA</color>\n\n";
					}
					else if (val65.boostRadius > 0f)
					{
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>NEARBY PLAYERS</color>" + effectColors["ItemInfoDisplayPositive"] + " GAIN</color> " + effectColors["Extra Stamina"] + (val65.baselineStaminaBoost * 100f).ToString("F1").Replace(".0", "") + " EXTRA STAMINA</color>\n\n";
					}
				}
				else if (type == typeof(Breakable))
				{
					Breakable val66 = (Breakable)val6;
					if (val66.breakOnCollision)
					{
						if (val66.minBreakVelocity > 0f)
						{
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + "THROW SPEED > " + effectColors["Hunger"] + val66.minBreakVelocity.ToString("F1").Replace(".0", "") + "</color> TO SMASH IT OPEN\n\n";
						}
						else
						{
							TextMeshProUGUI val67 = itemInfoDisplayTextMesh;
							((TMP_Text)val67).text = ((TMP_Text)val67).text + effectColors["Hunger"] + "THROW</color> TO SMASH ON IMPACT\n\n";
						}
					}
				}
				else if (type == typeof(Bonkable))
				{
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Hunger"] + "AIM AT A TEAMMATE'S HEAD</color> " + effectColors["Injury"] + "TO KNOCK THEM OUT\n\n";
				}
				else if (type == typeof(MagicBean))
				{
					MagicBean val68 = (MagicBean)val6;
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Hunger"] + "DROP</color> TO PLANT VINES,\n\nGROWING PERPENDICULAR TO TERRAIN, UP TO " + (val68.plantPrefab.maxLength / 2f).ToString("F1").Replace(".0", "") + " m, OR UNTIL THEY HIT AN OBSTACLE\n\n";
				}
				else if (type == typeof(BingBong))
				{
					TextMeshProUGUI val69 = itemInfoDisplayTextMesh;
					((TMP_Text)val69).text = ((TMP_Text)val69).text + "AN AIRLINE MASCOT: " + effectColors["Extra Stamina"] + "BingBong</color>\n\n";
				}
				else if (type == typeof(Action_Passport))
				{
					TextMeshProUGUI val70 = itemInfoDisplayTextMesh;
					((TMP_Text)val70).text = ((TMP_Text)val70).text + "USE THE " + effectColors["Hunger"] + "PASSPORT</color> TO CUSTOMISE YOUR LOOK\n\n";
				}
				else if (type == typeof(Actions_Binoculars))
				{
					TextMeshProUGUI val71 = itemInfoDisplayTextMesh;
					((TMP_Text)val71).text = ((TMP_Text)val71).text + "USE THE " + effectColors["Hunger"] + "BINOCULARS</color> TO VIEW FAR OBJECTS\n\n";
				}
				else if (type == typeof(Action_WarpToRandomPlayer))
				{
					TextMeshProUGUI val72 = itemInfoDisplayTextMesh;
					((TMP_Text)val72).text = ((TMP_Text)val72).text + "TELEPORTS TO A RANDOM PLAYER\n\n";
				}
				else if (type == typeof(Action_WarpToBiome))
				{
					Action_WarpToBiome val73 = (Action_WarpToBiome)val6;
					TextMeshProUGUI val74 = itemInfoDisplayTextMesh;
					((TMP_Text)val74).text = ((TMP_Text)val74).text + "TELEPORTS TO " + ((object)(Segment)(ref val73.segmentToWarpTo)/*cast due to .constrained prefix*/).ToString().ToUpper() + "\n\n";
				}
				else if (type == typeof(Parasol))
				{
					TextMeshProUGUI val75 = itemInfoDisplayTextMesh;
					((TMP_Text)val75).text = ((TMP_Text)val75).text + "USE THE " + effectColors["Hunger"] + "PARASOL</color> TO OPEN/CLOSE; SLOWS FALL AND BLOCKS MESA SUN\n\n";
				}
				else if (type == typeof(RescueHook))
				{
					RescueHook val76 = (RescueHook)val6;
					TextMeshProUGUI val77 = itemInfoDisplayTextMesh;
					((TMP_Text)val77).text = ((TMP_Text)val77).text + "<#CCCCCC>FIRES A GRAPPLING HOOK TO:</color>\n\n";
					TextMeshProUGUI val78 = itemInfoDisplayTextMesh;
					((TMP_Text)val78).text = ((TMP_Text)val78).text + effectColors["ItemInfoDisplayPositive"] + "RESCUE OTHER PLAYERS</color>\n\n";
					TextMeshProUGUI val79 = itemInfoDisplayTextMesh;
					((TMP_Text)val79).text = ((TMP_Text)val79).text + effectColors["ItemInfoDisplayPositive"] + "PULL YOURSELF TO A WALL</color>\n\n";
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Extra Stamina"] + "UPWARD RANGE " + (val76.range * CharacterStats.unitsToMeters).ToString("F1").Replace(".0", "") + " m</color>\n\n";
					val17 = itemInfoDisplayTextMesh;
					((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Extra Stamina"] + "DOWNWARD RANGE " + (val76.rangeDownward * CharacterStats.unitsToMeters).ToString("F1").Replace(".0", "") + " m</color>\n\n";
				}
				else
				{
					if (type == typeof(Frisbee))
					{
						continue;
					}
					if (type == typeof(Action_ConstructableScoutCannonScroll))
					{
						TextMeshProUGUI val80 = itemInfoDisplayTextMesh;
						((TMP_Text)val80).text = ((TMP_Text)val80).text + "\n<#CCCCCC>WHEN PLACED, LIGHT FUSE TO:</color>\n\nLAUNCH SCOUTS IN BARREL\n\n";
					}
					else if (type == typeof(Action_RandomMushroomEffect))
					{
						Action_RandomMushroomEffect val81 = (Action_RandomMushroomEffect)val6;
						int num = -1;
						if (val81.useDebugEffect)
						{
							num = val81.debugEffect;
						}
						else if ((Object)MushroomManager.instance != (Object)null)
						{
							int num2 = val81.mushroomTypeIndex % MushroomManager.instance.mushroomEffects.Length;
							num = MushroomManager.instance.mushroomEffects[num2];
						}
						int num3 = 0;
						if (!val81.useDebugEffect && (Object)MushroomManager.instance != (Object)null)
						{
							int num4 = val81.mushroomTypeIndex % MushroomManager.instance.mushroomStamAmt.Length;
							num3 = MushroomManager.instance.mushroomStamAmt[num4];
						}
						TextMeshProUGUI val82 = itemInfoDisplayTextMesh;
						((TMP_Text)val82).text = ((TMP_Text)val82).text + effectColors["ItemInfoDisplayPositive"] + "EAT TO RECEIVE A RANDOM EFFECT:</color>\n\n";
						if (num3 > 0)
						{
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Extra Stamina"] + "+" + ((float)num3 * 0.05f * 100f).ToString("F1").Replace(".0", "") + " EXTRA STAMINA</color>\n\n";
						}
						if (num >= 0)
						{
							TextMeshProUGUI val83 = itemInfoDisplayTextMesh;
							((TMP_Text)val83).text = ((TMP_Text)val83).text + "\n<#CCCCCC>CURRENT EFFECT: ";
							switch (num)
							{
							case 0:
							{
								TextMeshProUGUI val93 = itemInfoDisplayTextMesh;
								((TMP_Text)val93).text = ((TMP_Text)val93).text + effectColors["Extra Stamina"] + "INFINITE STAMINA FOR 4 s</color>";
								break;
							}
							case 1:
							{
								TextMeshProUGUI val92 = itemInfoDisplayTextMesh;
								((TMP_Text)val92).text = ((TMP_Text)val92).text + effectColors["Extra Stamina"] + "SPEED: +50% RUN, +150% CLIMB, 5 s</color>";
								break;
							}
							case 2:
							{
								TextMeshProUGUI val91 = itemInfoDisplayTextMesh;
								((TMP_Text)val91).text = ((TMP_Text)val91).text + effectColors["ItemInfoDisplayPositive"] + "LOW GRAVITY FOR 15 s</color>";
								break;
							}
							case 3:
							{
								TextMeshProUGUI val90 = itemInfoDisplayTextMesh;
								((TMP_Text)val90).text = ((TMP_Text)val90).text + effectColors["Shield"] + "INVINCIBILITY FOR 10 s</color>";
								break;
							}
							case 4:
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Hunger"] + "-15 HUNGER</color>, " + effectColors["Injury"] + "-15 INJURY</color>, " + effectColors["Poison"] + "-15 POISON, -15 SPORES, CURES POISON</color>";
								break;
							case 5:
							{
								TextMeshProUGUI val89 = itemInfoDisplayTextMesh;
								((TMP_Text)val89).text = ((TMP_Text)val89).text + effectColors["Injury"] + "EXPLODES AND KNOCKS BACK NEARBY TEAMMATES</color>";
								break;
							}
							case 6:
							{
								TextMeshProUGUI val88 = itemInfoDisplayTextMesh;
								((TMP_Text)val88).text = ((TMP_Text)val88).text + effectColors["ItemInfoDisplayNegative"] + "BLINDNESS FOR 60 s</color>";
								break;
							}
							case 7:
							{
								TextMeshProUGUI val87 = itemInfoDisplayTextMesh;
								((TMP_Text)val87).text = ((TMP_Text)val87).text + effectColors["Injury"] + "UNCONSCIOUS FOR 8 s</color>";
								break;
							}
							case 8:
							{
								TextMeshProUGUI val86 = itemInfoDisplayTextMesh;
								((TMP_Text)val86).text = ((TMP_Text)val86).text + effectColors["Poison"] + "+25 SPORES</color>";
								break;
							}
							case 9:
							{
								TextMeshProUGUI val85 = itemInfoDisplayTextMesh;
								((TMP_Text)val85).text = ((TMP_Text)val85).text + effectColors["ItemInfoDisplayNegative"] + "NUMBNESS FOR 60 s</color>";
								break;
							}
							default:
							{
								TextMeshProUGUI val84 = itemInfoDisplayTextMesh;
								((TMP_Text)val84).text = ((TMP_Text)val84).text + effectColors["ItemInfoDisplayNegative"] + "UNKNOWN EFFECT</color>";
								break;
							}
							}
							TextMeshProUGUI val94 = itemInfoDisplayTextMesh;
							((TMP_Text)val94).text = ((TMP_Text)val94).text + "</color>\n\n";
						}
						else
						{
							TextMeshProUGUI val95 = itemInfoDisplayTextMesh;
							((TMP_Text)val95).text = ((TMP_Text)val95).text + "<#CCCCCC>POSSIBLE EFFECTS:</color>\n\n";
							TextMeshProUGUI val96 = itemInfoDisplayTextMesh;
							((TMP_Text)val96).text = ((TMP_Text)val96).text + effectColors["ItemInfoDisplayPositive"] + "GOOD: INFINITE STAMINA, SPEED, LOW GRAVITY, INVINCIBILITY, HEAL</color>\n\n";
							TextMeshProUGUI val97 = itemInfoDisplayTextMesh;
							((TMP_Text)val97).text = ((TMP_Text)val97).text + effectColors["ItemInfoDisplayNegative"] + "BAD: EXPLODE, BLIND, UNCONSCIOUS, SPORES, NUMB</color>\n\n";
						}
						TextMeshProUGUI val98 = itemInfoDisplayTextMesh;
						((TMP_Text)val98).text = ((TMP_Text)val98).text + effectColors["Hunger"] + " TAKES EFFECT AFTER 3 s</color>\n\n";
					}
					else if (type == typeof(Dynamite))
					{
						Dynamite val99 = (Dynamite)val6;
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Injury"] + "EXPLOSION</color> DEALS UP TO " + effectColors["Injury"] + (val99.explosionPrefab.GetComponent<AOE>().statusAmount * 100f).ToString("F1").Replace(".0", "") + " INJURY</color>\n\n<#CCCCCC>EXTRA DAMAGE WHILE HELD</color>\n\n";
					}
					else if (type == typeof(Scorpion))
					{
						Scorpion val100 = (Scorpion)val6;
						if ((int)((Mob)val100).mobState != 3)
						{
							TextMeshProUGUI val101 = itemInfoDisplayTextMesh;
							((TMP_Text)val101).text = ((TMP_Text)val101).text + "WHEN HELD OR NEAR, DEALS " + effectColors["Poison"] + "POISON</color>:\n\n";
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Heat"] + "COOKING</color> MAKES IT " + effectColors["Curse"] + "DIE</color>\n\n";
							if (configForceUpdateTime.Value <= 1f)
							{
								float num5 = Mathf.Max(0.5f, 1f - currentItem.holderCharacter.refs.afflictions.statusSum + 0.05f) * 100f;
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>NEXT STING DEALS:</color> " + effectColors["Poison"] + num5.ToString("F1").Replace(".0", "") + " </color>POISON</color> OVER " + val100.totalPoisonTime.ToString("F1").Replace(".0", "") + " s\n\n";
								TextMeshProUGUI val102 = itemInfoDisplayTextMesh;
								((TMP_Text)val102).text = ((TMP_Text)val102).text + "<#CCCCCC>(DEALS MORE WHEN HEALTHY)</color>\n\n";
							}
							else
							{
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>NEXT STING DEALS:</color> AT LEAST " + effectColors["Poison"] + "50 </color>POISON</color> OVER " + val100.totalPoisonTime.ToString("F1").Replace(".0", "") + " s\n\n";
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + "UP TO " + effectColors["Poison"] + "100 POISON</color> OVER " + val100.totalPoisonTime.ToString("F1").Replace(".0", "") + " s\n\n";
								TextMeshProUGUI val103 = itemInfoDisplayTextMesh;
								((TMP_Text)val103).text = ((TMP_Text)val103).text + "<#CCCCCC>(DEALS MORE WHEN HEALTHY)</color>\n\n";
							}
						}
					}
					else if (type == typeof(Action_Spawn))
					{
						Action_Spawn val104 = (Action_Spawn)val6;
						if ((Object)val104.objectToSpawn == (Object)null)
						{
							continue;
						}
						AntiSphere component7;
						if (((Object)val104.objectToSpawn).name.Equals("VFX_Sunscreen"))
						{
							AOE component4 = ((Component)val104.objectToSpawn.transform.Find("AOE")).GetComponent<AOE>();
							RemoveAfterSeconds component5 = ((Component)val104.objectToSpawn.transform.Find("AOE")).GetComponent<RemoveAfterSeconds>();
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>SPRAY CREATES A MIST FOR " + component5.seconds.ToString("F1").Replace(".0", "") + " s, APPLYING:</color>\n\n" + ProcessAffliction(component4.affliction);
						}
						else if (((Object)val104.objectToSpawn).name.Equals("VFX_BalloonPopWithKnockback"))
						{
							AOE component6 = ((Component)val104.objectToSpawn.transform).GetComponent<AOE>();
							if (component6.fallTime > 0f)
							{
								float factor = component6.GetFactor(1f);
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Injury"] + "STUN</color> " + (factor * component6.fallTime).ToString("F1").Replace(".0", "") + " s\n\n";
							}
							TextMeshProUGUI val105 = itemInfoDisplayTextMesh;
							((TMP_Text)val105).text = ((TMP_Text)val105).text + effectColors["Hunger"] + "USING</color> RELEASES AN AOE EFFECT\n\n";
							TextMeshProUGUI val106 = itemInfoDisplayTextMesh;
							((TMP_Text)val106).text = ((TMP_Text)val106).text + "<#CCCCCC>RANGE: " + (component6.range * CharacterStats.unitsToMeters).ToString("F1").Replace(".0", "") + " m</color>\n\n";
							TextMeshProUGUI val107 = itemInfoDisplayTextMesh;
							((TMP_Text)val107).text = ((TMP_Text)val107).text + effectColors["Injury"] + "KNOCKS BACK</color> NEARBY PLAYERS\n\n";
						}
						else if (TryGetDeepComponent<AntiSphere>(val104.objectToSpawn, out component7))
						{
							TryGetDeepComponent<RemoveAfterSeconds>(val104.objectToSpawn, out RemoveAfterSeconds component8);
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Extra Stamina"] + "GENERATES AN ANTI-GRAV FIELD</color>\n\n";
							if ((Object)(object)component8 != (Object)null)
							{
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>LASTS " + FormatSeconds(component8.seconds) + "</color>\n\n";
							}
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>LIFTS PLAYERS AND GROUND ITEMS; GRANTS INFINITE STAMINA</color>\n\n";
							if (component7.staminaGainRate > 0f)
							{
								val17 = itemInfoDisplayTextMesh;
								((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Extra Stamina"] + "RECOVERS " + FormatNumber(component7.staminaGainRate * 100f) + "% STAMINA</color>\n\n";
							}
							val17 = itemInfoDisplayTextMesh;
							((TMP_Text)val17).text = ((TMP_Text)val17).text + "<#CCCCCC>BOUNCED OUT ON EXIT; PULLED TO CENTRE ON ENTRY</color>\n\n";
						}
					}
					else if (type == typeof(Action_ApplyAntigrav))
					{
						TextMeshProUGUI val108 = itemInfoDisplayTextMesh;
						((TMP_Text)val108).text = ((TMP_Text)val108).text + effectColors["Extra Stamina"] + "GENERATES AN ANTI-GRAV FIELD</color>\n\n";
						TextMeshProUGUI val109 = itemInfoDisplayTextMesh;
						((TMP_Text)val109).text = ((TMP_Text)val109).text + "<#CCCCCC>LIFTS PLAYERS AND GROUND ITEMS; GRANTS INFINITE STAMINA</color>\n\n";
					}
					else if (type == typeof(CactusBall))
					{
						CactusBall val110 = (CactusBall)val6;
						val17 = itemInfoDisplayTextMesh;
						((TMP_Text)val17).text = ((TMP_Text)val17).text + effectColors["Thorns"] + "STICKS</color> TO YOUR BODY\n\n" + effectColors["Hunger"] + "THROW TO RELEASE</color>\n\nNEEDS AT LEAST " + (((StickyItemComponent)val110).throwChargeRequirement * 100f).ToString("F1").Replace(".0", "") + "%\n\n";
					}
					else if (type == typeof(BingBongShieldWhileHolding))
					{
						TextMeshProUGUI val111 = itemInfoDisplayTextMesh;
						((TMP_Text)val111).text = ((TMP_Text)val111).text + "<#CCCCCC>WHILE HELD YOU GAIN:</color>\n\n" + effectColors["Shield"] + "INVINCIBILITY</color>\n\n";
					}
					else if (type == typeof(CheckpointConstructable))
					{
						TextMeshProUGUI val112 = itemInfoDisplayTextMesh;
						((TMP_Text)val112).text = ((TMP_Text)val112).text + "PLACE SOMEWHERE TO GRANT ONE " + effectColors["Extra Stamina"] + "REVIVE</color>\n\n";
					}
					else
					{
						if (!(type == typeof(ItemCooking)))
						{
							continue;
						}
						text4 = "";
						ItemCooking val113 = (ItemCooking)val6;
						if (val113.wreckWhenCooked && val113.timesCookedLocal >= 1)
						{
							text4 = text4 + "\n\n" + effectColors["Curse"] + "UNUSABLE DUE TO COOKING</color>";
						}
						else if (val113.wreckWhenCooked)
						{
							text4 = text4 + "\n\n" + effectColors["Curse"] + "BECOMES UNUSABLE WHEN COOKED</color>";
						}
						else if (val113.timesCookedLocal >= 12)
						{
							text4 = text4 + "   " + effectColors["Curse"] + val113.timesCookedLocal + "x COOKED\n\nCANNOT BE COOKED</color>";
						}
						else if (val113.timesCookedLocal == 0 && val113.canBeCooked)
						{
							text4 = text4 + "\n\nCAN BE " + effectColors["Extra Stamina"] + "COOKED</color>";
							if (val113.additionalCookingBehaviors.Length != 0)
							{
								int num6 = 0;
								int num7 = 0;
								AdditionalCookingBehavior[] additionalCookingBehaviors = val113.additionalCookingBehaviors;
								AdditionalCookingBehavior[] array = additionalCookingBehaviors;
								foreach (AdditionalCookingBehavior val114 in array)
								{
									CookingBehavior_EnableScripts val115 = (CookingBehavior_EnableScripts)((val114 is CookingBehavior_EnableScripts) ? val114 : null);
									if (val115 != null)
									{
										num6 = val115.scriptsToEnable.Length;
										continue;
									}
									CookingBehavior_DisableScripts val116 = (CookingBehavior_DisableScripts)((val114 is CookingBehavior_DisableScripts) ? val114 : null);
									if (val116 != null)
									{
										num7 = val116.scriptsToDisable.Length;
									}
								}
								if (num6 > 0 || num7 > 0)
								{
									text4 += ", AFTER COOKING</color> WILL:\n\n";
								}
								if (num6 > 0)
								{
									text4 += string.Format("{0}ADD {1} </color>EFFECT(S), ", effectColors["Extra Stamina"], num6);
								}
								if (num7 > 0)
								{
									text4 += string.Format("{0}REMOVE {1} </color>EFFECT(S)", effectColors["ItemInfoDisplayPositive"], num7);
								}
								text4 = text4.TrimEnd('，');
							}
						}
						else if (val113.timesCookedLocal == 1)
						{
							text4 = text4 + "   " + effectColors["Extra Stamina"] + val113.timesCookedLocal + "x COOKED</color>\n\n" + effectColors["Hunger"] + "CAN BE COOKED</color>";
						}
						else if (val113.timesCookedLocal == 2)
						{
							text4 = text4 + "   " + effectColors["Hunger"] + val113.timesCookedLocal + "x COOKED</color>\n\n" + effectColors["Injury"] + "CAN BE COOKED</color>";
						}
						else if (val113.timesCookedLocal == 3)
						{
							text4 = text4 + "   " + effectColors["Injury"] + val113.timesCookedLocal + "x COOKED</color>\n\n" + effectColors["Poison"] + "CAN BE COOKED</color>";
						}
						else if (val113.timesCookedLocal >= 4)
						{
							text4 = text4 + "   " + effectColors["Poison"] + val113.timesCookedLocal + "x COOKED\n\nCAN BE COOKED</color>";
						}
						if (val113.hasExplosion)
						{
							text4 = text4 + "\n\nAFTER COOKING, IT" + effectColors["Injury"] + " EXPLODES</color>";
						}
					}
				}
			}
			text5 += GetNewVersionItemTips(gameObject, currentItem);
			if (text.Length > 0 && flag)
			{
				((TMP_Text)itemInfoDisplayTextMesh).text = text + "" + ((TMP_Text)itemInfoDisplayTextMesh).text;
			}
			if (text5.Length > 0)
			{
				TextMeshProUGUI val117 = itemInfoDisplayTextMesh;
				((TMP_Text)val117).text = ((TMP_Text)val117).text + "\n" + text5;
			}
			val17 = itemInfoDisplayTextMesh;
			((TMP_Text)val17).text = ((TMP_Text)val17).text + "\n" + text2 + text3 + text4;
			((TMP_Text)itemInfoDisplayTextMesh).text = ((TMP_Text)itemInfoDisplayTextMesh).text.Replace("\n\n", "\n\n");
		}

		private static string ProcessSingleGameObjectAOE(GameObject targetObject, float globalDuration = 0f, bool addTips = true)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			string text = "";
			float x = 0.025f;
			if ((Object)targetObject == (Object)null)
			{
				return text;
			}
			AOE component = targetObject.GetComponent<AOE>();
			if ((Object)component == (Object)null || Mathf.Abs(component.statusAmount) == 0f)
			{
				return text;
			}
			if (!addTips)
			{
				addTips = true;
				text = text + "<#CCCCCC>RANGE: " + (component.range * CharacterStats.unitsToMeters).ToString("F1").Replace(".0", "") + "m</color>, FOR " + globalDuration + " s\n\n";
				text = text + "<#CCCCCC>EFFECT WEAKENS WITH DISTANCE (MIN " + (component.minFactor * 100f).ToString("F0") + "%)</color>\n\n";
			}
			TimeEvent component2 = targetObject.GetComponent<TimeEvent>();
			if ((Object)component2 != (Object)null && globalDuration > 0f)
			{
				x = ((!(component.statusAmount < 0f)) ? MathF.Abs(x) : (MathF.Abs(x) * -1f));
				float amountPerSecond = Mathf.Floor(component.statusAmount * (1f / component2.rate) / x) * x;
				text += ProcessEffectPerSecond(amountPerSecond, ((object)(STATUSTYPE)(ref component.statusType)/*cast due to .constrained prefix*/).ToString());
				if (component.addtlStatus != null && component.addtlStatus.Length != 0)
				{
					STATUSTYPE[] addtlStatus = component.addtlStatus;
					for (int i = 0; i < addtlStatus.Length; i++)
					{
						text += ProcessEffectPerSecond(amountPerSecond, ((object)addtlStatus[i]).ToString());
					}
				}
			}
			else
			{
				text = text + "INSTANTLY" + ProcessEffect(component.statusAmount, ((object)(STATUSTYPE)(ref component.statusType)/*cast due to .constrained prefix*/).ToString());
				if (component.addtlStatus != null && component.addtlStatus.Length != 0)
				{
					STATUSTYPE[] addtlStatus2 = component.addtlStatus;
					for (int j = 0; j < addtlStatus2.Length; j++)
					{
						STATUSTYPE val = addtlStatus2[j];
						text += ProcessEffect(component.statusAmount, ((object)val).ToString());
					}
				}
			}
			if (component.hasAffliction && component.affliction != null)
			{
				text += ProcessAffliction(component.affliction);
			}
			if (!text.EndsWith("\n\n"))
			{
				text += "\n\n";
			}
			return text;
		}

		private static string ProcessGameObjectAndChildrenAOE(GameObject targetObject, float globalDuration = 0f, bool addTips = true)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			string text = "";
			if ((Object)targetObject == (Object)null)
			{
				return text;
			}
			string text2 = ProcessSingleGameObjectAOE(targetObject, globalDuration, addTips);
			if (!string.IsNullOrEmpty(text2))
			{
				text += text2;
			}
			for (int i = 0; i < targetObject.transform.childCount; i++)
			{
				Transform child = targetObject.transform.GetChild(i);
				string text3 = ProcessGameObjectAndChildrenAOE(((Component)child).gameObject, globalDuration);
				if (!string.IsNullOrEmpty(text3))
				{
					text += text3;
				}
			}
			return text;
		}

		private static string ProcessEffect(float amount, string effect, bool newLine = true)
		{
			string text = "";
			if (amount < 0f && effect == "Poison")
			{
				text += ProcessEffect(amount, "Spores");
			}
			if (amount == 0f)
			{
				return text;
			}
			if (amount > 0f)
			{
				text = ((!effect.Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayNegative"]) : (text + effectColors["ItemInfoDisplayPositive"]));
				text += "GAIN</color> ";
			}
			else if (amount < 0f)
			{
				text = ((!effect.Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayPositive"]) : (text + effectColors["ItemInfoDisplayNegative"]));
				text += "REMOVE</color> ";
			}
			return text + effectColors[effect] + (Mathf.Abs(amount) * 100f).ToString("F1").Replace(".0", "") + " " + GetEffectChineseName(effect) + "</color>" + (newLine ? "\n\n" : "");
		}

		private static string ProcessEffectPerSecond(float amountPerSecond, string effect, bool newLine = true)
		{
			string text = "";
			if (amountPerSecond < 0f && effect == "Poison")
			{
				text += ProcessEffectPerSecond(amountPerSecond, "Spores");
			}
			if (amountPerSecond == 0f)
			{
				return text;
			}
			if (amountPerSecond > 0f)
			{
				text = ((!effect.Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayNegative"]) : (text + effectColors["ItemInfoDisplayPositive"]));
				text += "GAINS</color> ";
			}
			else if (amountPerSecond < 0f)
			{
				text = ((!effect.Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayPositive"]) : (text + effectColors["ItemInfoDisplayNegative"]));
				text += "REMOVES</color> ";
			}
			return text + effectColors[effect] + (Mathf.Abs(amountPerSecond) * 100f).ToString("F1").Replace(".0", "") + " " + GetEffectChineseName(effect) + "</color>" + (newLine ? "\n\n" : ((object)newLine));
		}

		private static string ProcessEffectOverTime(float amountPerSecond, float rate, float time, string effect, bool newLine = true)
		{
			string text = "";
			float num = Mathf.Abs(amountPerSecond) * time;
			if (amountPerSecond > 0f)
			{
				text = text + effectColors["ItemInfoDisplayNegative"] + "GAIN</color> ";
			}
			else
			{
				if (!(amountPerSecond < 0f))
				{
					return text;
				}
				text = text + effectColors["ItemInfoDisplayPositive"] + "REMOVE</color> ";
			}
			text = text + effectColors[effect] + num.ToString("F1").Replace(".0", "") + " " + GetEffectChineseName(effect) + "</color> ";
			return text + "FOR " + time.ToString("F1").Replace(".0", "") + " s" + (newLine ? "\n" : "");
		}

		private static string ProcessAffliction(Affliction affliction)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Invalid comparison between Unknown and I4
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Expected O, but got Unknown
			//IL_034d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0353: Invalid comparison between Unknown and I4
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Expected O, but got Unknown
			//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04db: Invalid comparison between Unknown and I4
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Expected O, but got Unknown
			//IL_0649: Unknown result type (might be due to invalid IL or missing references)
			//IL_0650: Invalid comparison between Unknown and I4
			//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ef: Expected O, but got Unknown
			//IL_0750: Unknown result type (might be due to invalid IL or missing references)
			//IL_0756: Invalid comparison between Unknown and I4
			//IL_065d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0664: Expected O, but got Unknown
			//IL_0845: Unknown result type (might be due to invalid IL or missing references)
			//IL_084b: Invalid comparison between Unknown and I4
			//IL_0763: Unknown result type (might be due to invalid IL or missing references)
			//IL_076a: Expected O, but got Unknown
			//IL_08b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bf: Invalid comparison between Unknown and I4
			//IL_0934: Unknown result type (might be due to invalid IL or missing references)
			//IL_093b: Invalid comparison between Unknown and I4
			//IL_08c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d0: Expected O, but got Unknown
			//IL_09ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b5: Invalid comparison between Unknown and I4
			//IL_0945: Unknown result type (might be due to invalid IL or missing references)
			//IL_094c: Expected O, but got Unknown
			//IL_0a84: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a8b: Invalid comparison between Unknown and I4
			//IL_09c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c9: Expected O, but got Unknown
			//IL_0b27: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b2e: Invalid comparison between Unknown and I4
			//IL_0a98: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9f: Expected O, but got Unknown
			//IL_0ba8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0baf: Invalid comparison between Unknown and I4
			//IL_0b38: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b3f: Expected O, but got Unknown
			//IL_0c08: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c0f: Invalid comparison between Unknown and I4
			//IL_0bb9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bc0: Expected O, but got Unknown
			//IL_0c68: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c6f: Invalid comparison between Unknown and I4
			//IL_0c19: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c20: Expected O, but got Unknown
			//IL_0d2a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d31: Invalid comparison between Unknown and I4
			//IL_0c7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c83: Expected O, but got Unknown
			//IL_0da4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dab: Invalid comparison between Unknown and I4
			//IL_0d3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d42: Expected O, but got Unknown
			//IL_0e19: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e20: Invalid comparison between Unknown and I4
			//IL_0db5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dbc: Expected O, but got Unknown
			//IL_0eb5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ebc: Invalid comparison between Unknown and I4
			//IL_0e2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e34: Expected O, but got Unknown
			//IL_0f35: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f3c: Invalid comparison between Unknown and I4
			//IL_0ec6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ecd: Expected O, but got Unknown
			//IL_0f49: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f50: Expected O, but got Unknown
			string text = "";
			if ((int)affliction.GetAfflictionType() == 2)
			{
				Affliction_FasterBoi val = (Affliction_FasterBoi)affliction;
				string text2 = (((Affliction)val).totalTime + val.climbDelay).ToString("F1").Replace(".0", "");
				string text3 = Mathf.Round((val.moveSpeedMod - 1f) * 100f).ToString("+0;-#").Replace(".0", "");
				string text4 = Mathf.Round((val.climbSpeedMod - 1f) * 100f).ToString("+0;-#").Replace(".0", "");
				string text5 = (val.drowsyOnEnd * 100f).ToString("F1").Replace(".0", "");
				List<string> list = new List<string>();
				if (val.moveSpeedMod != 1f)
				{
					list.Add(effectColors[(val.moveSpeedMod > 1f) ? "Extra Stamina" : "ItemInfoDisplayNegative"] + "MOVE SPEED " + text3 + "%</color>");
				}
				if (val.climbSpeedMod != 1f)
				{
					list.Add(effectColors[(val.climbSpeedMod > 1f) ? "Extra Stamina" : "ItemInfoDisplayNegative"] + "CLIMB SPEED " + text4 + "%</color>");
				}
				string text6 = string.Join("\n\n", list);
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN " + text2 + " s EFFECT:</color>\n\n" + text6 + "\n\n";
				if (val.drowsyOnEnd > 0f)
				{
					text = text + effectColors["ItemInfoDisplayNegative"] + "WHEN IT ENDS, GAIN " + effectColors["Drowsy"] + text5 + " DROWSY</color></color>\n\n";
				}
			}
			else if ((int)affliction.GetAfflictionType() == 8)
			{
				Affliction_ClearAllStatus val2 = (Affliction_ClearAllStatus)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "CLEAR ALL STATUS</color>";
				if (val2.excludeCurse)
				{
					text = text + " EXCEPT " + effectColors["Curse"] + "CURSE</color>";
				}
				text += "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 10)
			{
				Affliction_AddBonusStamina val3 = (Affliction_AddBonusStamina)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + effectColors["Extra Stamina"] + (val3.staminaAmount * 100f).ToString("F1").Replace(".0", "") + " EXTRA STAMINA</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 1)
			{
				Affliction_InfiniteStamina val4 = (Affliction_InfiniteStamina)affliction;
				float num = ((Affliction)val4).totalTime + val4.climbDelay;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + num.ToString("F1").Replace(".0", "") + " s OF " + effectColors["Extra Stamina"] + "INFINITE STAMINA</color>";
				if (val4.climbDelay > 0f)
				{
					text = text + "\n\n" + effectColors["ItemInfoDisplayNegative"] + "CLIMB SHORTENED:</color> WHEN CLIMBING, IF TIME LEFT > " + ((Affliction)val4).totalTime.ToString("F1").Replace(".0", "") + " s, IT CUTS TO " + ((Affliction)val4).totalTime.ToString("F1").Replace(".0", "") + " s";
				}
				text += "\n\n";
				if (val4.drowsyAffliction != null && val4.drowsyAffliction.totalTime > 0f)
				{
					text = text + ", THEN " + ProcessAffliction(val4.drowsyAffliction);
				}
			}
			else if ((int)affliction.GetAfflictionType() == 7)
			{
				Affliction_AdjustStatus val5 = (Affliction_AdjustStatus)affliction;
				if (val5.statusAmount > 0f)
				{
					text = ((!((object)(STATUSTYPE)(ref val5.statusType)/*cast due to .constrained prefix*/).ToString().Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayNegative"]) : (text + effectColors["ItemInfoDisplayPositive"]));
					text += "GAIN</color> ";
				}
				else
				{
					text = ((!((object)(STATUSTYPE)(ref val5.statusType)/*cast due to .constrained prefix*/).ToString().Equals("Extra Stamina")) ? (text + effectColors["ItemInfoDisplayPositive"]) : (text + effectColors["ItemInfoDisplayNegative"]));
					text += "REMOVE</color> ";
				}
				text = text + effectColors[((object)(STATUSTYPE)(ref val5.statusType)/*cast due to .constrained prefix*/).ToString()] + (Mathf.Abs(val5.statusAmount) * 100f).ToString("F1").Replace(".0", "") + " " + GetEffectChineseName(((object)(STATUSTYPE)(ref val5.statusType)/*cast due to .constrained prefix*/).ToString()) + "</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 11)
			{
				Affliction_AdjustDrowsyOverTime val6 = (Affliction_AdjustDrowsyOverTime)affliction;
				text = ((!(val6.statusPerSecond > 0f)) ? (text + effectColors["ItemInfoDisplayPositive"] + "REMOVE</color> ") : (text + effectColors["ItemInfoDisplayNegative"] + "GAIN</color> "));
				text = text + effectColors["Drowsy"] + (Mathf.Round(Mathf.Abs(val6.statusPerSecond) * ((Affliction)val6).totalTime * 100f * 0.4f) / 0.4f).ToString("F1").Replace(".0", "") + " DROWSY</color> OVER " + ((Affliction)val6).totalTime.ToString("F1").Replace(".0", "") + " s\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 5)
			{
				Affliction_AdjustColdOverTime val7 = (Affliction_AdjustColdOverTime)affliction;
				text = ((!(val7.statusPerSecond > 0f)) ? (text + effectColors["ItemInfoDisplayPositive"] + "REMOVE</color> ") : (text + effectColors["ItemInfoDisplayNegative"] + "GAIN</color> "));
				text = text + effectColors["Cold"] + (Mathf.Abs(val7.statusPerSecond) * ((Affliction)val7).totalTime * 100f).ToString("F1").Replace(".0", "") + " COLD</color> OVER " + ((Affliction)val7).totalTime.ToString("F1").Replace(".0", "") + " s\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 6)
			{
				text = text + effectColors["ItemInfoDisplayPositive"] + "CLEAR ALL STATUS</color>\n\n";
				text = text + effectColors["ItemInfoDisplayNegative"] + "RANDOM NEGATIVE STATUS COMBO</color>\n\n";
				text = text + effectColors["ItemInfoDisplayPositive"] + "RANDOM EXTRA STAMINA</color>\n\n";
				text += "<#CCCCCC>EFFECT IS FULLY RANDOM</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 13)
			{
				Affliction_Sunscreen val8 = (Affliction_Sunscreen)affliction;
				text = text + "IN THE SUN AT MESA, PREVENTS " + effectColors["Heat"] + "HEAT</color> FOR " + ((Affliction)val8).totalTime.ToString("F1").Replace(".0", "") + " s\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 14)
			{
				Affliction_BingBongShield val9 = (Affliction_BingBongShield)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + effectColors["Shield"] + "INVINCIBILITY</color> FOR " + FormatSeconds(((Affliction)val9).totalTime) + "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 15)
			{
				Affliction_ZombieBite val10 = (Affliction_ZombieBite)affliction;
				float value = Mathf.Max(0f, ((Affliction)val10).totalTime - val10.delayBeforeEffect);
				text = text + val10.delayBeforeEffect.ToString("F1").Replace(".0", "") + " s DELAY, FOR " + FormatSeconds(value) + ", GAINING " + effectColors["Spores"] + (Mathf.Abs(val10.statusPerSecond) * 100f).ToString("F1").Replace(".0", "") + " SPORES</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 16)
			{
				Affliction_Invincibility val11 = (Affliction_Invincibility)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + effectColors["Shield"] + ((Affliction)val11).totalTime.ToString("F1").Replace(".0", "") + "</color> s OF " + effectColors["Shield"] + "INVINCIBILITY</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 17)
			{
				Affliction_LowGravity val12 = (Affliction_LowGravity)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> LOW GRAVITY";
				if (val12.lowGravAmount > 1)
				{
					text = text + " x" + val12.lowGravAmount;
				}
				text = text + " FOR " + FormatSeconds(((Affliction)val12).totalTime) + "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 18)
			{
				Affliction_Blind val13 = (Affliction_Blind)affliction;
				text = text + effectColors["ItemInfoDisplayNegative"] + "BLIND</color> FOR " + FormatSeconds(((Affliction)val13).totalTime) + "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 19)
			{
				Affliction_Numb val14 = (Affliction_Numb)affliction;
				text = text + effectColors["ItemInfoDisplayNegative"] + "NUMB</color> FOR " + FormatSeconds(((Affliction)val14).totalTime) + "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 20)
			{
				Affliction_ClimbingChalk val15 = (Affliction_ClimbingChalk)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> CLIMBING CHALK FOR " + FormatSeconds(((Affliction)val15).totalTime) + "\n\n";
				text = text + effectColors["Extra Stamina"] + "CLIMB STAMINA COST BECOMES " + (val15.climbStaminaMultiplier * 100f).ToString("F1").Replace(".0", "") + "%</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 21)
			{
				Affliction_NoHunger val16 = (Affliction_NoHunger)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "DOES NOT GAIN</color> " + effectColors["Hunger"] + "HUNGER</color> FOR " + FormatSeconds(((Affliction)val16).totalTime) + "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 22)
			{
				Affliction_HealAll val17 = (Affliction_HealAll)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "INSTANTLY HEALS</color> INJURY, SPORES, POISON, COLD, HEAT, DROWSY, THORNS, CURSE\n\n";
				text = text + "<#CCCCCC>MAX TOTAL HEAL: " + (val17.maxHealing * 100f).ToString("F1").Replace(".0", "") + "</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 23)
			{
				Affliction_DoubleJumpAmulet val18 = (Affliction_DoubleJumpAmulet)affliction;
				text = text + effectColors["ItemInfoDisplayPositive"] + "GAIN</color> " + effectColors["Extra Stamina"] + "EXTRA JUMPS</color>";
				if (val18.stacks > 1)
				{
					text = text + " x" + val18.stacks;
				}
				text += "\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 24)
			{
				Affliction_RadiateInfiniteStam val19 = (Affliction_RadiateInfiniteStam)affliction;
				text = text + effectColors["Extra Stamina"] + "GRANTS NEARBY PLAYERS INFINITE STAMINA</color>\n\n";
				text = text + "<#CCCCCC>RANGE: " + FormatNumber(val19.radius) + " m, FOR " + FormatSeconds(((Affliction)val19).totalTime) + "</color>\n\n";
			}
			else if ((int)affliction.GetAfflictionType() == 25)
			{
				Affliction_MassSuperJump val20 = (Affliction_MassSuperJump)affliction;
				text = text + "<#CCCCCC>RANGE: " + FormatNumber(val20.radius) + " m, FORCE " + FormatNumber(val20.forceAmt) + " FOR " + FormatSeconds(val20.forceTime) + "</color>\n\n";
				if (val20.lowGravTime > 0f)
				{
					text = text + "<#CCCCCC>THEN GAINS LOW GRAVITY FOR " + FormatSeconds(val20.lowGravTime) + "</color>\n\n";
				}
			}
			return text;
		}

		private static void AddDisplayObject()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			EasyBackpack = Chainloader.PluginInfos.ContainsKey("nickklmao.easybackpack");
			GameObject val = GameObject.Find("GAME/GUIManager");
			guiManager = val.GetComponent<GUIManager>();
			TMP_FontAsset font = ((TMP_Text)guiManager.heroDayText).font;
			GameObject gameObject = ((Component)val.transform.Find("Canvas_HUD/Prompts/ItemPromptLayout")).gameObject;
			GameObject val2 = new GameObject("ItemInfoDisplay");
			val2.transform.SetParent(gameObject.transform);
			itemInfoDisplayTextMesh = val2.AddComponent<TextMeshProUGUI>();
			RectTransform component = val2.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(configSizeDeltaX.Value, 0f);
			((TMP_Text)itemInfoDisplayTextMesh).font = font;
			((TMP_Text)itemInfoDisplayTextMesh).fontSize = configFontSize.Value;
			((TMP_Text)itemInfoDisplayTextMesh).alignment = (TextAlignmentOptions)1025;
			((TMP_Text)itemInfoDisplayTextMesh).lineSpacing = configLineSpacing.Value;
			((TMP_Text)itemInfoDisplayTextMesh).text = "";
			((TMP_Text)itemInfoDisplayTextMesh).outlineWidth = configOutlineWidth.Value;
		}

		private static void InitEffectColors(Dictionary<string, string> dict)
		{
			dict.Add("Spores", "<#A45B62>");
			dict.Add("Hunger", "<#FFBD16>");
			dict.Add("Extra Stamina", "<#BFEC1B>");
			dict.Add("Injury", "<#FF5300>");
			dict.Add("Crab", "<#E13542>");
			dict.Add("Poison", "<#A139FF>");
			dict.Add("Cold", "<#00BCFF>");
			dict.Add("Heat", "<#C80918>");
			dict.Add("Hot", "<#C80918>");
			dict.Add("Sleepy", "<#FF5CA4>");
			dict.Add("Drowsy", "<#FF5CA4>");
			dict.Add("Curse", "<#1B0043>");
			dict.Add("Weight", "<#A65A1C>");
			dict.Add("Thorns", "<#768E00>");
			dict.Add("Shield", "<#D48E00>");
			dict.Add("Web", "<#DDDDDD>");
			dict.Add("Arrow", "<#FF5300>");
			dict.Add("Petrify", "<#B6B6B6>");
			dict.Add("FlyTrap", "<#7FBF3F>");
			dict.Add("ItemInfoDisplayPositive", "<#DDFFDD>");
			dict.Add("ItemInfoDisplayNegative", "<#FFCCCC>");
		}
	}
}
namespace BepInEx
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[Conditional("CodeGeneration")]
	internal sealed class BepInAutoPluginAttribute : Attribute
	{
		public BepInAutoPluginAttribute(string id = null, string name = null, string version = null)
		{
		}
	}
}
namespace BepInEx.Preloader.Core.Patching
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[Conditional("CodeGeneration")]
	internal sealed class PatcherAutoPluginAttribute : Attribute
	{
		public PatcherAutoPluginAttribute(string id = null, string name = null, string version = null)
		{
		}
	}
}
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class IgnoresAccessChecksToAttribute : Attribute
	{
		public IgnoresAccessChecksToAttribute(string assemblyName)
		{
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.0.0.9375' (yours is '9.0.0.7889')
