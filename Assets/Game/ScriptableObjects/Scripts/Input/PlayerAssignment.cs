// Assets/_Game/Scripts/Input/PlayerAssignment.cs

using UnityEngine;

namespace WizardPunk
{
    public static class PlayerAssignment
    {
        public static WandSerialReader PlayerA { get; set; }
        public static WandSerialReader PlayerB { get; set; }

        public static void Initialize(WandSerialReader p1Reader = null, WandSerialReader p2Reader = null)
        {
            if (p1Reader != null && WandSerialReader.IsAlive(p1Reader))
            {
                PlayerA = p1Reader;
                Debug.Log($"[PlayerAssignment] PlayerA synced to {p1Reader.serialPort}");
            }
            else if (!WandSerialReader.IsAlive(PlayerA))
            {
                PlayerA = WandSerialReader.GetByPort("COM8");
                if (PlayerA != null) Debug.Log("[PlayerAssignment] PlayerA fallback -> COM8");
            }

            if (p2Reader != null && WandSerialReader.IsAlive(p2Reader))
            {
                PlayerB = p2Reader;
                Debug.Log($"[PlayerAssignment] PlayerB synced to {p2Reader.serialPort}");
            }
            else if (!WandSerialReader.IsAlive(PlayerB))
            {
                PlayerB = WandSerialReader.GetByPort("COM9");
                if (PlayerB != null) Debug.Log("[PlayerAssignment] PlayerB fallback -> COM9");
            }
        }
    }
}
