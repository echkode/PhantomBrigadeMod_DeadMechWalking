using System;
using System.Collections.Generic;

using Content.Code.Utility;
using PhantomBrigade;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;

using UnityEngine;

namespace EchKode.PBMods.SelfDetonation.Functions
{
	[TypeHinted]
	[Serializable]
	public sealed class CombatUnitDebrisProjectiles : ICombatFunctionTargeted
	{
		public DataBlockUnitDestructionFragments fragments = new DataBlockUnitDestructionFragments();

		public void Run(PersistentEntity unitPersistent)
		{
			DeadMechWalking.Patch.CreateDestructionFragments(unitPersistent, fragments);
		}
	}

	[TypeHinted]
	[Serializable]
	public sealed class CombatActionSetMemory : ICombatActionExecutionFunction
	{
		public string key;
		public UnitMemoryContext context = UnitMemoryContext.Unit; 
		public ValueOperation operation;
		public CombatUnitSetMemory.ResolverMode resolverMode = CombatUnitSetMemory.ResolverMode.Random;
		public List<ICombatUnitValueResolver> resolvers = new List<ICombatUnitValueResolver>();

		public void Run(CombatEntity unitCombat, ActionEntity action)
		{
			var unitPersistent = IDUtility.GetLinkedPersistentEntity(unitCombat);
			if (unitPersistent == null)
			{
				return;
			}
			var setMemory = new CombatUnitSetMemory()
			{
				key = key,
				context = context,
				operation = operation,
				resolverMode = resolverMode,
				resolvers = resolvers,
			};
			setMemory.Run(unitPersistent);
		}
	}

	[TypeHinted]
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
