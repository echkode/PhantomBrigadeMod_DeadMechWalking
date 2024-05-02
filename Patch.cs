// Copyright (c) 2024 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using PhantomBrigade;
using PhantomBrigade.Combat.Systems;
using PhantomBrigade.Combat.View;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace EchKode.PBMods.SelfDetonation.DeadMechWalking
{
	[HarmonyPatch]
	public static partial class Patch
	{
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(CombatUnitDestructionEffectSystem), "Execute", new System.Type[]
		{
			typeof(List<PersistentEntity>),
		})]
		public static void CreateDestructionFragments(PersistentEntity unitPersistent, DataBlockUnitDestructionFragments fragments)
		{
			_ = Transpiler(null, null);

			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				var cm = new CodeMatcher(instructions, generator);
				var fragmentsFieldInfo = AccessTools.DeclaredField(typeof(DataContainerUnitBlueprint), nameof(DataContainerUnitBlueprint.destructionFragments));
				var functionsFieldInfo = AccessTools.DeclaredField(typeof(DataContainerUnitBlueprint), nameof(DataContainerUnitBlueprint.destructionFunctions));
				var visualManagerFieldInfo = AccessTools.DeclaredField(typeof(CombatView), nameof(CombatView.visualManager));
				var loadFragmentsMatch = new CodeMatch(OpCodes.Ldfld, fragmentsFieldInfo);
				var loadFunctionsMatch = new CodeMatch(OpCodes.Ldfld, functionsFieldInfo);
				var loadUnitMatch = new CodeMatch(OpCodes.Ldloc_1);
				var brfalseMatch = new CodeMatch(OpCodes.Brfalse);
				var bleMatch = new CodeMatch(OpCodes.Ble);
				var bleunMatch = new CodeMatch(OpCodes.Ble_Un);
				var brtrueMatch = new CodeMatch(OpCodes.Brtrue);
				var loadVisualManagerMatch = new CodeMatch(OpCodes.Ldfld, visualManagerFieldInfo);
				var nop = new CodeInstruction(OpCodes.Nop);
				var loadUnit = new CodeInstruction(OpCodes.Ldarg_0);
				var loadFragments = new CodeInstruction(OpCodes.Ldarg_1);

				cm.MatchStartForward(loadFragmentsMatch)
					.Advance(1);

				var storeMatch = new CodeMatch(cm.Instruction);
				cm.Advance(1);
				var loadMatch = new CodeMatch(cm.Instruction);

				cm.Start()
					.MatchStartForward(loadFragmentsMatch)
					.RemoveInstructionsInRange(0, cm.Pos);

				cm.Start()
					.MatchStartForward(loadFunctionsMatch)
					.Advance(-1);
				var startPos = cm.Pos;
				// It's important to keep the ret at the end of the original function otherwise
				// Harmony will throw an invalid IL exception when it tries to apply the reverse patch.
				cm.End()
					.Advance(-1)
					.RemoveInstructionsInRange(startPos, cm.Pos);

				cm.End().Labels.Clear();
				cm.CreateLabel(out var endLabel);

				cm.Start()
					.MatchStartForward(storeMatch)
					.Repeat(m => m.RemoveInstruction());
				cm.Start()
					.MatchStartForward(loadMatch)
					.Repeat(m => m.SetInstruction(loadFragments));
				cm.Start()
					.MatchStartForward(loadUnitMatch)
					.Repeat(m => m.SetInstruction(loadUnit));
				cm.Start()
					.MatchStartForward(brfalseMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(bleMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(bleunMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(brtrueMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(brfalseMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(brfalseMatch)
					.SetOperandAndAdvance(endLabel)
					.MatchStartForward(loadVisualManagerMatch)
					.Advance(-2)
					.SetOperandAndAdvance(endLabel);

				return cm.InstructionEnumeration();
			}
		}

		[HarmonyPatch(typeof(DataHelperAction), nameof(DataHelperAction.IsValid))]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Dha_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var cm = new CodeMatcher(instructions, generator);
			var getDataKeyActionMethodInfo = AccessTools.DeclaredPropertyGetter(typeof(ActionEntity), nameof(ActionEntity.dataKeyAction));
			var getLinkedPilotMethodInfo = AccessTools.DeclaredMethod(typeof(IDUtility), nameof(IDUtility.GetLinkedPilot));
			var getDataKeyActionMatch = new CodeMatch(OpCodes.Callvirt, getDataKeyActionMethodInfo);
			var getLinkedPilotMatch = new CodeMatch(OpCodes.Call, getLinkedPilotMethodInfo);
			var checkMemory = CodeInstruction.Call(typeof(Patch), nameof(CheckMemory));

			cm.End()
				.MatchStartBackwards(getDataKeyActionMatch)
				.Advance(-1);
			var jump = new CodeInstruction(OpCodes.Brtrue, cm.Labels[0]);
			cm.MatchStartBackwards(getLinkedPilotMatch)
				.Advance(-1);
			var loadUnit = cm.Instruction.Clone();
			cm.Advance(1)
				.InsertAndAdvance(checkMemory)
				.InsertAndAdvance(jump)
				.InsertAndAdvance(loadUnit);

			return cm.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(MechAnimationSystem), nameof(MechAnimationSystem.LateUpdateUnit))]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Mas_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var cm = new CodeMatcher(instructions, generator);
			var getLinkedPilotMethodInfo = AccessTools.DeclaredMethod(typeof(IDUtility), nameof(IDUtility.GetLinkedPilot));
			var getLinkedPilotMatch = new CodeMatch(OpCodes.Call, getLinkedPilotMethodInfo);
			var storeLocMatch = new CodeMatch(OpCodes.Stloc_S);
			var checkMemory = CodeInstruction.Call(typeof(Patch), nameof(CheckMemory));
			var loadZero = new CodeInstruction(OpCodes.Ldc_I4_0);

			cm.MatchEndForward(getLinkedPilotMatch)
				.Advance(-1);
			var loadUnit = cm.Instruction.Clone();
			cm.Advance(3)
				.MatchStartForward(storeLocMatch);
			var storeLabel = cm.Labels[0];
			var jumpStore = new CodeInstruction(OpCodes.Br, storeLabel);
			cm.Advance(-1)
				.MatchEndBackwards(storeLocMatch)
				.Advance(1)
				.CreateLabel(out var jumpLabel);
			var skip = new CodeInstruction(OpCodes.Brfalse_S, jumpLabel);
			cm.InsertAndAdvance(loadUnit)
				.InsertAndAdvance(checkMemory)
				.InsertAndAdvance(skip)
				.InsertAndAdvance(loadZero)
				.InsertAndAdvance(jumpStore);

			return cm.InstructionEnumeration();
		}

		public static bool CheckMemory(PersistentEntity unitPersistent) => unitPersistent.IsMemoryPresent("dead_mech_walking");
	}
}
