# RAFLAB

Since RAFLAB is still under devolpment, I'm not releasing it as I want to fix some of the issues it has first

RAFLAB was made by me (qkms) and liamriley101

RAFLAB (Recreate Avatar From Loaded Asset Bundle)

RAFLAB lets you take a vrca loaded asset bundle from FACS01 load bundle (https://github.com/FACS01-01/FACS_Utilities) and make get it to project state without ripping it with an asset ripper.

Animation file copying is not supported at the moment as it's impoosible to recreate animations from loaded asset bundles

This tool is mainly used to rip anti rip avatars that break any asset ripper directly, or get turned into corrupt assets when ripped, I haven't been able to test it with other anti rip systems that either break your mesh, armature or anything else when ripped.

As of right now (10/11/2023) There's alot of bugs that need to be fixed, but it's in a mostly working state.

Known Issues (Not bugs, issues):
1. If a material uses multiple of the same texture, it'll copy all textures as if they are different, same with other materials using the same texture, so you can get multiple copies of the same texture

2. If an avatar has mutliple avatars, it will copy all avatars

3. Audios do not like to be put back into place automatically for some reason

4. Animation copying is not possible at the moment

5. If the folder you're copying materials too already has copied materials, the new materials will lose their references to both textures and shaders making them purple

Demonstration of RAFLAB in action:
https://www.youtube.com/watch?v=m9fn2ppZNZQ&feature=youtu.be
