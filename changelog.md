# Changelog RaScenesSO

## v1.1.0 - 21/04/2025
* Bumped RaModelsSO dependency to v1.1.2 (was v1.1.0)
* Made it so for Loading a Prefab is linked in instead of a Scene (A Loading Scene is created at runtime and remains there for the entirety of the game's lifecycle)

## v1.0.3 - 30/04/2023
* Bumped RaScenesSOCollection dependency to v1.4.0 (was v1.3.2) to prevent constant exceptions during loading scenes
* Added Start / Stop loading events to the RaSceneModelSO
* Bumped RaModelsSO dependency to v1.1.0 (was v1.0.0)

## v1.0.2 - 14/04/2023
* Corrected RaModelSOCollection type
* Added Events for Start / End of loading
* Removed LoadScene util method from the RaSceneSO

## v1.0.1 - 10/04/2023
* Made it so the Scenes are automatically filled into the BuildSettings by the RaSceneSOCollection
* Added LoadScene method to the RaSceneSO
* Fixed Build issues due to editor only code

## v1.0.0 - 06/04/2023
* Initial Release
