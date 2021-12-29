using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Core.Player
{
    public partial class PlayerController
    {
        private void ClientProcessInput()
        {
            var rotInput = inputActions.PlayerControls.MouseDelta.ReadValue<Vector2>();
            transform.Rotate(0, rotInput.x * rotateSpeed * Time.fixedDeltaTime, 0);

            var moveinput = inputActions.PlayerControls.Move.ReadValue<Vector2>();
            var ds = (transform.forward * moveinput.y + transform.right * moveinput.x) * speed;
            charCtrl.SimpleMove(ds);
        }

        private void TEMP_ClientSetSpawnPosition()
        {
            //TODO (hack): Hardcoding spawn position
            charCtrl.enabled = false;
            transform.position = new Vector3(-45.76f, 1f, -10.65f);
            charCtrl.enabled = true;
        }
    }
}


