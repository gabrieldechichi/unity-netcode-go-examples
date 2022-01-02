using Runtime.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal abstract class WorldConfigView<T> : MonoBehaviour where T : World
    {
        [SerializeField] protected T world;
        [SerializeField] protected InputField lagInput;
        [SerializeField] protected InputField updateRateInput;

        protected virtual void OnEnable()
        {
            lagInput.onEndEdit.AddListener(OnLagInputEndEdit);
            updateRateInput.onEndEdit.AddListener(OnUpdateRateInputEndEdit);
            UpdateUI();
        }

        protected virtual void OnDisable()
        {
            lagInput.onEndEdit.RemoveListener(OnLagInputEndEdit);
            updateRateInput.onEndEdit.RemoveListener(OnUpdateRateInputEndEdit);
        }

        private void OnUpdateRateInputEndEdit(string newValue)
        {
            if (int.TryParse(newValue, out var updateRate))
            {
                world.UpdatesPerSecond = updateRate;
                UpdateUI();
            }
        }

        private void OnLagInputEndEdit(string newValue)
        {
            if (int.TryParse(newValue, out var updateRate))
            {
                world.Network.LagMs = updateRate;
                UpdateUI();
            }
        }

        protected virtual void UpdateUI()
        {
            lagInput.text = $"{world.Network.LagMs} ms";
            updateRateInput.text = $"{world.UpdatesPerSecond}";
        }
    }
}
