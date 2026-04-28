using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CruiserRadioSelector
{
    [BepInPlugin("Potatoh.CruiserRadioSelector", "Cruiser Radio Selector", "1.3.2")]
    [BepInDependency("Mellowdy.CruiserTunes", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        private int currentPage = 0;
        internal static VehicleController? CachedCar;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Cruiser Radio Selector loaded!");
        }

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying) return;

            if (CachedCar == null || CachedCar.gameObject == null)
                CachedCar = FindObjectOfType<VehicleController>();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.vKey.wasPressedThisFrame)
            {
                NextPage();
                return;
            }

            bool selectorHeld = keyboard.cKey.isPressed;
            if (!selectorHeld) return;

            if (keyboard.digit1Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 0);
            if (keyboard.digit2Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 1);
            if (keyboard.digit3Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 2);
            if (keyboard.digit4Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 3);
            if (keyboard.digit5Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 4);
            if (keyboard.digit6Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 5);
            if (keyboard.digit7Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 6);
            if (keyboard.digit8Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 7);
            if (keyboard.digit9Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 8);
            if (keyboard.digit0Key.wasPressedThisFrame) TrySetTrack(currentPage * 10 + 9);
        }

        private void NextPage()
        {
            VehicleController? car = CachedCar;
            if (car == null || car.radioClips == null || car.radioClips.Length == 0) return;

            int tracksPerPage = 10;
            int totalPages = Mathf.CeilToInt(car.radioClips.Length / (float)tracksPerPage);

            currentPage++;
            if (currentPage >= totalPages)
                currentPage = 0;

            Log.LogInfo($"Cruiser Radio Selector page: {currentPage + 1}/{totalPages}");
        }

        private static void TrySetTrack(int index)
        {
            VehicleController? car = CachedCar;

            if (car == null)
            {
                Log.LogWarning("VehicleController not found.");
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

            int signalQuality = 3;

            Log.LogInfo($"Setting cruiser radio track: {index + 1}/{car.radioClips.Length}");

            car.SetRadioStationServerRpc(index, signalQuality);
        }

        private void OnGUI()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool selectorHeld = keyboard.cKey.isPressed;
            if (!selectorHeld) return;

            VehicleController? car = CachedCar;
            if (car == null || car.radioClips == null || car.radioClips.Length == 0) return;

            int tracksPerPage = 10;
            int totalPages = Mathf.CeilToInt(car.radioClips.Length / (float)tracksPerPage);
            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));

            GUI.Box(new Rect(20, 80, 460, 350), "");

            GUI.Label(new Rect(35, 95, 420, 25), $"Cruiser Radio Selector | Page {currentPage + 1}/{totalPages}");
            GUI.Label(new Rect(35, 120, 420, 25), "Hold C + 1-9/0 - select track | V - next page");

            for (int slot = 0; slot < tracksPerPage; slot++)
            {
                int index = currentPage * tracksPerPage + slot;
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