using System;
using Runtime.Client;
using Runtime.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal class ClientConfigView : WorldConfigView<Client>
    {
        [SerializeField] private Toggle clientPredictionToggle;
        [SerializeField] private Toggle serverReconciliationToggle;

        private void Awake()
        {
            updateRateInput.readOnly = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            clientPredictionToggle.onValueChanged.AddListener(OnClientPredictionToggle);
            serverReconciliationToggle.onValueChanged.AddListener(OnServerReconcilicationToggle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            clientPredictionToggle.onValueChanged.RemoveListener(OnClientPredictionToggle);
            serverReconciliationToggle.onValueChanged.RemoveListener(OnServerReconcilicationToggle);
        }

        private void OnServerReconcilicationToggle(bool newValue)
        {
            world.EnableServerReconciliation = newValue;
            UpdateUI();
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
            serverReconciliationToggle.isOn = world.EnableServerReconciliation;
        }
    }
}
