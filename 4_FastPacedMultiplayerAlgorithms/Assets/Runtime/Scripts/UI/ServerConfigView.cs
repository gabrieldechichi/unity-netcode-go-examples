using System;
using Runtime.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal class ServerConfigView : MonoBehaviour
    {
        [SerializeField] private Server server;
        [SerializeField] private InputField lagInput;
        [SerializeField] private InputField updateRateInput;

        private void OnEnable()
        {
            lagInput.onEndEdit.AddListener(OnLagInputEndEdit);
            updateRateInput.onEndEdit.AddListener(OnUpdateRateInputEndEdit);
            UpdateUI();
        }

        private void OnDisable()
        {
            lagInput.onEndEdit.RemoveListener(OnLagInputEndEdit);
            updateRateInput.onEndEdit.RemoveListener(OnUpdateRateInputEndEdit);
        }

        private void OnUpdateRateInputEndEdit(string newValue)
        {
            if (int.TryParse(newValue, out var updateRate))
            {
                server.UpdatesPerSecond = updateRate;
                UpdateUI();
            }
        }

        private void OnLagInputEndEdit(string newValue)
        {
            if (int.TryParse(newValue, out var updateRate))
            {
                server.Network.LagMs = updateRate;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            lagInput.text = $"{server.Network.LagMs} ms";
            updateRateInput.text = $"{server.UpdatesPerSecond}";
        }
    }
}
