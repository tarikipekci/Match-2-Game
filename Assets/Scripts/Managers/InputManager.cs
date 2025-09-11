using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public static bool CanPlay { get; private set; }

        private void OnEnable()
        {
            GridManager.OnBoardReady += EnableInput;
        }

        private void OnDisable()
        {
            GridManager.OnBoardReady -= EnableInput;
        }

        private void EnableInput(GridManager grid)
        {
            CanPlay = true;
        }

        public static void DisableInput()
        {
            CanPlay = false;
        }
    }
}
