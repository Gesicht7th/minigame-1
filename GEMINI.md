\# Wizard Trial - Gemini Project Context



\## Project Overview



This is a Unity 6 game project called \*\*Wizard Trial\*\*.



The game is a multi-minigame experience controlled using a custom ESP32-based wand controller.



Players use physical wand hardware that sends:



\* Gyroscope velocity data

\* Action button input

\* Reset button input



The game consists of multiple scenes connected sequentially.



\---



\## HyperSmash Canonical Scene



IMPORTANT:



The official and active HyperSmash gameplay scene is:



test hypersmash 1



This is the scene currently used in the production game flow.



The following scenes are NOT the primary gameplay scene:



\* HyperSmash

\* test hypersmash

\* test hypersmash 2



These scenes should be treated as:



\* backup scenes

\* prototypes

\* experimental versions

\* legacy implementations



Do not assume they are part of the active game flow unless explicitly requested.



When investigating HyperSmash gameplay bugs, input issues, scoring issues, scene transitions, UI problems, or wand integration:



Always analyze:



test hypersmash 1



first.



Only inspect the other HyperSmash-related scenes if:



\* the bug is confirmed to originate there, or

\* the user explicitly requests analysis of those scenes.



\---



\## Official Game Flow



MainMenu

→ ReadyScreen

→ MemoryTest

→ test hypersmash 1

→ ReflexShowdown

→ EndScreen



Result scenes may exist between minigames depending on current implementation.



For HyperSmash-related debugging, consider:



test hypersmash 1



as the canonical gameplay scene.



\---



\## Important Gameplay Systems



\### 1. Wand Hardware Input



Primary input source:



Assets/Game/ScriptableObjects/Scripts/Input/WandSerialReader.cs



This script receives data from ESP32 hardware.



Input types:



\* GyroVelocity

\* Action button

\* Reset button



Important:



Do not modify WandSerialReader.cs unless explicitly requested.



\---



\### 2. Virtual Cursor System



Important scripts:



\* GlobalVirtualCursor.cs

\* CursorSceneBootstrap.cs

\* WandMenuCursor.cs (legacy system)



Current architecture:



GlobalVirtualCursor is the active cursor system.



Legacy scene-local cursor systems may still exist in some scenes.



Expected behavior:



MainMenu:



\* Cursor visible



ReadyScreen:



\* Cursor hidden



MemoryTest Tutorial:



\* Cursor hidden



MemoryTest Gameplay:



\* Cursor hidden



MemoryTest Result Popup:



\* Cursor visible



\---



\### 3. MemoryTest



Folder:



Assets/Game/ScriptableObjects/Scripts/MemoryTest/



Important scripts:



\* MemoryTestGameManager

\* MemoryTestUIManager

\* MemoryTestPatternManager

\* PlayerWandController

\* TutorialReadyController



Gameplay:



Players memorize rune patterns and reproduce them using wand gestures.



Uses:



\* Wand direction detection

\* Gyroscope input

\* Action button



\---



\### 4. HyperSmash



Folder:



Assets/Game/ScriptableObjects/Scripts/HyperSmash/



Important scripts:



\* HyperSmashGameManager

\* ShootingSystem

\* WandAimController

\* CrystalSpawner

\* CrystalFactory

\* HyperSmashScoreManager



Gameplay:



Players aim with the wand and shoot crystals.



Expected hardware support:



\* Wand aiming from gyro data

\* Action button for shooting

\* Reset button when applicable



Whenever HyperSmash input issues occur, trace:



WandSerialReader

→ WandAimController

→ ShootingSystem

→ HyperSmash gameplay



\---



\### 5. ReflexShowdown



Folder:



Assets/Game/ScriptableObjects/Scripts/ReflexShowdown/



Important scripts:



\* ReflexGameManager

\* PlayerReflexController

\* ReflexUIManager



Uses wand input and reaction mechanics.



\---



\## Scene Management



Important scripts:



\* SceneFlowManager.cs

\* SceneLoader.cs

\* SceneNames.cs

\* GameDataBridge.cs



Scene transitions are handled through SceneFlowManager.



Before diagnosing scene-loading issues:



1\. Verify scene name.

2\. Verify Build Settings / Build Profiles.

3\. Verify scene exists in project.

4\. Verify runtime value assigned in scene Inspector.



\---



\## Analysis Rules



When debugging:



1\. Analyze before editing.

2\. Identify root cause first.

3\. Explain execution flow.

4\. Show minimal fix.

5\. Only then propose edits.



Never immediately edit files without first explaining:



\* Root cause

\* Affected files

\* Why the fix works



\---



\## Unity Project Rules



When analyzing:



Ignore:



\* Library/

\* Temp/

\* Logs/

\* Obj/

\* Build/

\* UserSettings/



Do not scan the entire repository unless explicitly requested.



Prefer targeted analysis.



\---



\## Modification Rules



Do NOT modify these systems unless explicitly instructed:



\* WandSerialReader.cs

\* SerialManager.cs

\* DualSerialManager.cs



Treat them as core hardware communication systems.



If a bug appears in gameplay, first investigate:



\* Missing references

\* Scene configuration

\* Inspector assignments

\* Manager initialization

\* Scene transitions



before modifying hardware communication code.



\---



\## Expected Debugging Output



When investigating a bug, provide:



\### Execution Flow



\### Runtime Values



\### Root Cause



\### Minimal Fix



\### Files To Modify



Avoid speculation.

Only report findings supported by project files.




