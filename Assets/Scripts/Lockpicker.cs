using System;
using FK.Common;
using FK.Lokpik.Data;
using FK.Lokpik.Locks;
using UnityEngine;
using Vertx.Attributes;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FK.Lokpik
{
    using RO = ReadOnlyFieldAttribute;

    public class Lockpicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PickControlsAsset controls;
        [SerializeField] private TumblerLock tumblerLock;

        [Header("Debug")]
        [SerializeField, RO, Range(0, 1)] private float appliedTorque;
        [SerializeField, RO, Range(0, 1)] private float appliedPickLiftForce;
        [SerializeField, RO, Range(0, 5)] private int targetedPin;
        [SerializeField, RO, Min(0)] private float chamberRetargetTimer;

        public TumblerLock Lock => tumblerLock;
        public int TargetedPin => targetedPin;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private IPickInput input;
        private RumbleService rumbleService;

        private void Awake()
        {
            for (int i = 0; i < tumblerLock.Config.PinCount; i++)
                tumblerLock.Chamber(i).SetLock(tumblerLock, i);

            tumblerLock.StopPicking();
            tumblerLock.SubscribeToChamberStateChanges(Chamber_StateChanged);
        }

        internal void Init(IPickInput input) => this.input = input;
        internal void Init(RumbleService rumbleService) => this.rumbleService = rumbleService;

        private void Update()
        {
            if (input is not Object)
                return;

            MovePick();
            ApplyTorque();

            appliedPickLiftForce = Mathf.Lerp(0f, controls.MaxPickReachHeight, input.PickHeight);

            tumblerLock.TurnPlugTowards(appliedTorque, controls.PreventUnsettingPins);
            tumblerLock.LiftPinTowards(targetedPin, appliedPickLiftForce);

            // rumble if any pin is binding
            for (int pinIndex = 0; pinIndex < tumblerLock.Config.PinCount; pinIndex++)
            {
                ChamberState state = tumblerLock.GetChamberState(pinIndex);
                if (state.IsBinding())
                {
                    rumbleService.SetFrequencies(controls.PinBindRumble * appliedTorque);
                    break;
                }
            }
        }

        private void MovePick()
        {
            chamberRetargetTimer += Time.deltaTime;
            if (chamberRetargetTimer < controls.ChamberRetargetRate)
                return;

            int delta = input.PickMoveDelta;
            if (delta == 0) return;

            if (appliedPickLiftForce > 0)
                return; // prevent moving pick while it's still manipulating a pin

            chamberRetargetTimer = 0;

            tumblerLock.StopLifting(targetedPin);
            targetedPin = tumblerLock.Config.ClampPinIndex(targetedPin + delta);
        }

        private void ApplyTorque()
        {
            float tension = input.Tension;
            if (controls.UseTensionDrift && tension > 0)
            {
                float randomDrift = Random.Range(-controls.TensionDrift, controls.TensionDrift);
                tension += randomDrift;
            }

            AppliedTorque = tension;
        }

        private async void Chamber_StateChanged(Chamber chamber, ChamberState oldState, ChamberState newState)
        {
            // if (chamber.ChamberIndex != targetedPin)
            //     return;

            switch (newState)
            {
                case ChamberState.Free:
                    await rumbleService.SetFrequencies(controls.PinFreedRumble, controls.RumbleDuration);
                    break;
                case ChamberState.Set:
                case ChamberState.AboveShearLine:
                    await rumbleService.SetFrequencies(controls.PinSetRumble, controls.RumbleDuration);
                    break;
                case ChamberState.Underset:
                case ChamberState.Overset:
                    // handled every frame for continuous rumble
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }
    }
}
