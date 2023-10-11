# RAFLAB

RAFLAB (Recreate Avatar From Loaded Asset Bundle)

RAFLAB lets you take a vrca loaded asset bundle from FACS01 load bundle (https://github.com/FACS01-01/FACS_Utilities) and make get it to project state without ripping it with an asset ripper.

Animation file copying is not supported at the moment as it's impoosible to recreate animations from loaded asset bundles

This tool is mainly used to rip anti rip avatars that break any asset ripper directly, or get turned into corrupt assets when ripped, I haven't been able to test it with other anti rip systems that either break your mesh, armature or anything else when ripped.

As of right now (10/11/2023) There's alot of bugs that need to be fixed, but it's in a mostly working state.

Known bugs:

1.   Materials will sometimes lose their reference to the shaders inside of the assetbundle when copied
     
2.   If an avatar has mutliple avatars, it will copy all avatars
     
3.   Audios do not like to be put back into place automatically for some reason
     
4.   Animation copying is not possible at the moment.
