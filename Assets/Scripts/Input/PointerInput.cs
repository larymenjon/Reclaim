using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Reclaim.Input
{
    /// <summary>
    /// Ponto único de leitura de mouse e teclado.
    /// O projeto usa apenas o Input System novo (Player Settings > Active Input Handling),
    /// então nenhuma chamada a UnityEngine.Input pode existir fora daqui.
    /// </summary>
    public static class PointerInput
    {
        public static Vector2 Position => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        public static Vector2 Delta => Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        /// <summary>Valor bruto da roda do mouse no eixo vertical, como o Input System entrega.</summary>
        public static float ScrollY => Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;

        public static bool PrimaryPressedThisFrame => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        public static bool PrimaryHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;

        public static bool PrimaryReleasedThisFrame => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        public static bool SecondaryPressedThisFrame => Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        public static bool MiddleHeld => Mouse.current != null && Mouse.current.middleButton.isPressed;

        public static bool AnyButtonOrKeyPressedThisFrame =>
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || PrimaryPressedThisFrame
            || SecondaryPressedThisFrame;

        public static bool IsOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public static bool KeyPressedThisFrame(Key key) => Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;

        public static bool KeyHeld(Key key) => Keyboard.current != null && Keyboard.current[key].isPressed;

        public static bool ControlHeld => KeyHeld(Key.LeftCtrl) || KeyHeld(Key.RightCtrl);
    }
}
