using System;
using Runtime.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal class ClientConfigView : WorldConfigView<Client>
    {
        [SerializeField] private Toggle clientPredictionToggle;

        private void Awake()
        {
            updateRateInput.readOnly = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            clientPredictionToggle.onValueChanged.AddListener(OnClientPredictionToggle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            clientPredictionToggle.onValueChanged.RemoveListener(OnClientPredictionToggle);
        }

        private void OnClientPredictionToggle(bool newValue)
        {
            world.EnableClientPrediction = newValue;
            UpdateUI();
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            clientPredictionToggle.isOn = world.EnableClientPrediction;
        }
    }
}
