using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CruiserRadioSelector
{
    [BepInPlugin("Potatoh.CruiserRadioSelector", "Cruiser Radio Selector", "1.4.0")]
    [BepInDependency("Mellowdy.CruiserTunes", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        private const int TracksPerPage = 10;
        private int currentPage = 0;

        private static VehicleController? cachedCar;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Cruiser Radio Selector loaded!");
        }

        private static VehicleController? GetCar()
        {
            if (cachedCar == null || cachedCar.gameObject == null)
                cachedCar = FindObjectOfType<VehicleController>();

            if (cachedCar == null)
                return null;

            // vehicleID 0 = vanilla Company Cruiser.
            // Prevents touching custom vehicles like Scanvan or other modded cruisers.
            if (cachedCar.vehicleID != 0)
                return null;

            return cachedCar;
        }

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.vKey.wasPressedThisFrame)
            {
                NextPage();
                return;
            }

            if (!keyboard.cKey.isPressed) return;

            if (keyboard.digit1Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 0);
            if (keyboard.digit2Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 1);
            if (keyboard.digit3Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 2);
            if (keyboard.digit4Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 3);
            if (keyboard.digit5Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 4);
            if (keyboard.digit6Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 5);
            if (keyboard.digit7Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 6);
            if (keyboard.digit8Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 7);
            if (keyboard.digit9Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 8);
            if (keyboard.digit0Key.wasPressedThisFrame) TrySetTrack(currentPage * TracksPerPage + 9);
        }

        private void NextPage()
        {
            VehicleController? car = GetCar();
            if (car == null || car.radioClips == null || car.radioClips.Length == 0) return;

            int totalPages = Mathf.CeilToInt(car.radioClips.Length / (float)TracksPerPage);

            currentPage++;
            if (currentPage >= totalPages)
                currentPage = 0;

            Log.LogInfo($"Cruiser Radio Selector page: {currentPage + 1}/{totalPages}");
        }

        private static void TrySetTrack(int index)
        {
            VehicleController? car = GetCar();

            if (car == null)
            {
                Log.LogWarning("Vanilla VehicleController not found.");
                return;
            }

            if (car.radioClips == null || car.radioClips.Length == 0)
            {
                Log.LogWarning("No radio clips found on cruiser.");
                return;
            }

            if (index < 0 || index >= car.radioClips.Length)
            {
                Log.LogWarning($"Track index {index} is out of range. Tracks count: {car.radioClips.Length}");
                return;
            }

            const int signalQuality = 3;

            Log.LogInfo($"Setting cruiser radio track: {index + 1}/{car.radioClips.Length}");

            car.SetRadioStationServerRpc(index, signalQuality);
        }

        private void OnGUI()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (!keyboard.cKey.isPressed) return;

            VehicleController? car = GetCar();
            if (car == null || car.radioClips == null || car.radioClips.Length == 0) return;

            int totalPages = Mathf.CeilToInt(car.radioClips.Length / (float)TracksPerPage);
            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));

            GUI.Box(new Rect(20, 80, 460, 350), "");

            GUI.Label(new Rect(35, 95, 420, 25), $"Cruiser Radio Selector | Page {currentPage + 1}/{totalPages}");
            GUI.Label(new Rect(35, 120, 420, 25), "Hold C + 1-9/0 - select track | V - next page");

            for (int slot = 0; slot < TracksPerPage; slot++)
            {
                int index = currentPage * TracksPerPage + slot;
                if (index >= car.radioClips.Length) break;

                string key = slot == 9 ? "0" : (slot + 1).ToString();
                string name = car.radioClips[index] != null ? car.radioClips[index].name : "Unknown";

                GUI.Label(
                    new Rect(35, 155 + slot * 25, 420, 25),
                    $"C + {key}  |  {index + 1}. {name}"
                );
            }
        }
    }
}