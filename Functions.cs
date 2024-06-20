// Copyright (c) 2024 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System;

using PhantomBrigade;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;

using UnityEngine;

namespace EchKode.PBMods.SelfDetonation.Functions
{
	[Serializable]
	public sealed class CombatUnitDebrisProjectiles : ICombatFunctionTargeted
	{
		public DataBlockUnitDestructionFragments fragments = new DataBlockUnitDestructionFragments();

		public void Run(PersistentEntity unitPersistent)
		{
			DeadMechWalking.Patch.CreateDestructionFragments(unitPersistent, fragments);
		}
	}

	[Serializable]
	public sealed class CombatActionPilotEject : ICombatActionExecutionFunction
	{
		public void Run(CombatEntity unitCombat, ActionEntity action)
		{
			if (!DataHelperAction.GetAvailableActions(unitCombat).Contains("eject"))
			{
				var unitPersistent = IDUtility.GetLinkedPersistentEntity(unitCombat);
				var pilotPersistent = IDUtility.GetLinkedPilot(unitPersistent);
				Debug.LogWarningFormat("Pilot {0} in unit {1}/{2} can't eject", pilotPersistent.ToLog(), unitPersistent.ToLog(), unitCombat.ToLog());
				return;
			}
			unitCombat.isCrashable = false;
			CombatActionEvent.OnEjection(unitCombat, action);
		}
	}
}
