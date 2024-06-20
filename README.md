# DeadMechWalking

A library mod for [Phantom Brigade](https://braceyourselfgames.com/phantom-brigade/) to add a new self-detonation action.

It is compatible with game release version **1.3.3**. It works with both the Steam and Epic installs of the game. All library mods are fragile and susceptible to breakage whenever a new version is released.

This is an example mod of how to add a new action to the game and also how to create a new status effect (introduced in version 1.3). The action causes the mech to self-detonate after a short delay. This gives time for the pilot to eject safely.

![The new action above the combat timeline](https://github.com/echkode/PhantomBrigadeMod_DeadMechWalking/assets/48565771/5a63af62-c789-47c0-ae09-ddd26d7677d0)

![The new action placed on the timeline](https://github.com/echkode/PhantomBrigadeMod_DeadMechWalking/assets/48565771/3ac424b4-7413-4439-992d-71d7c5b6a8c9)

Here's a video demonstrating the action in game.

<video controls src="https://github.com/echkode/PhantomBrigadeMod_DeadMechWalking/assets/48565771/62c5c86f-b61a-4fde-b94c-265522a4a011">
  <p>Demonstrating self-detonate in action. A mech runs toward another mech. A second before collision, the pilot ejects from the first mech. Just before collision, the first mech explodes in a powerful blast.</p>
</video>

## Technical Details

This mod uses a combination of config changes, function interfaces and dirty, low-down Harmony hacks, er, patches to make this work. The patches are needed only because there's no access to eject without crash and the mech needs to continue to run (or stand). Crash also wipes out all the actions on the timeline and the self-detonate action needs to remain on the timeline for it to trigger the explosion when it's time is up.
