using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Reagents;
using SealedSustenance.Components;
using Trading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Assets.Scripts.Objects.Items;
using SealedSustenance.Items;
using Assets.Scripts.Objects.Appliances;
using System.Text;
using Assets.Scripts.Localization2;
using SealedSustenance.Localization;
using Effects;
using System.Threading.Tasks;
using Assets.Scripts.Objects.Clothing.Suits;
using Assets.Scripts.Networking;
using Sealedsustenance.Scripts;

namespace SealedSustenance.Structures
{
    public class NutrientGelDispenser : Device // , IReferencable, IEvaluable
    {
        [Header("Nutrient Gel Dispenser")]
        [SerializeField] private LedProgressScreenComponent _ledQuantityScreen;
        [SerializeField] private MaterialChanger _progressLights;
        [SerializeField] private float _timeToRefill = 8f;
        [SerializeField] private float _capacity = 10000f;
        [SerializeField] private float _processingRate = 400f;

        public Slot InputSlot => this.Slots[2];

        private const int FLAG_MP_STORAGE = 1024;
        private const int FLAG_MP_PROCESSING = 2048;
        private const int FLAG_MP_PROCESSING_RATE = 4096;
        private const int FLAG_MP_CAPACITY = 8192;
        private const int FLAG_MP_TTR = 16384;

        private float _stored = 0f;
        private byte _processing;
        private UniTask _processingTask;
        private Food _toProcess;

        public float Stored
        {
            get
            {
                return _stored;
            }
            set
            {
                if (value != Stored)
                {
                    _stored = value;
                    if (NetworkManager.IsServer)
                    {
                        base.NetworkUpdateFlags |= FLAG_MP_STORAGE;
                    }
                }
            }
        }

        public float ProcessingRate
        {
            get
            {
                return _processingRate;
            }
            set
            {
                if (value != ProcessingRate)
                {
                    _processingRate = value;
                    if (NetworkManager.IsServer)
                    {
                        base.NetworkUpdateFlags |= FLAG_MP_PROCESSING_RATE;
                    }
                }
            }
        }

        public float TimeToRefill
        {
            get
            {
                return _timeToRefill;
            }
            set
            {
                if (value != TimeToRefill)
                {
                    _timeToRefill = value;
                    if (NetworkManager.IsServer)
                    {
                        base.NetworkUpdateFlags |= FLAG_MP_TTR;
                    }
                }
            }
        }

        public byte Processing
        {
            get
            {
                return _processing;
            }
            set
            {
                if (value != Processing)
                {
                    _processing = value;
                    if (NetworkManager.IsServer)
                    {
                        base.NetworkUpdateFlags |= FLAG_MP_PROCESSING;
                    }
                }
            }
        }

        public float Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                if (value != _capacity)
                {
                    _capacity = value;
                    if (NetworkManager.IsServer)
                    {
                        base.NetworkUpdateFlags |= FLAG_MP_CAPACITY;
                    }
                }
            }
        }

        public bool IsInputSlotEmpty
        {
            get
            {
                return InputSlot.Get() == null;
            }
        }

        public bool IsInputItemFood
        {
            get
            {
                Food food = InputSlot.Get<Food>();
                if (food == null)
                {
                    return false;
                }

                return true;
            }
        }

        public bool IsFull
        {
            get
            {
                return _stored >= _capacity;
            }
        }

        protected override bool IsOperable
        {
            get
            {
                if (!IsInputSlotEmpty)
                {
                    if (IsFull)
                    {
                        if (Error != 2)
                        {
                            Debug.Log("[NutrientGelDispenser] ERROR 2");
                            OnServer.Interact(base.InteractError, 2);
                        }

                        return false;
                    }

                    if (!IsInputItemFood)
                    {
                        if (Error != 1)
                        {
                            Debug.Log("[NutrientGelDispenser] ERROR 1");
                            OnServer.Interact(base.InteractError, 1);
                        }

                        return false;
                    }
                }

                if (Error != 0)
                {
                    OnServer.Interact(base.InteractError, 0);
                }

                return base.IsOperable;
            }
        }

        public override void Awake()
        {
            base.Awake();

            InputSlot.IsInteractable = true;
            InputSlot.Collider.enabled = true;

            Capacity = SealedSustenance.configNutrientGelDispenserMaxNutrientStorage.Value;
            TimeToRefill = SealedSustenance.configNutrientGelDispenserRefillTime.Value;
            ProcessingRate = SealedSustenance.configNutrientGelDispenserProcessRate.Value;
        }

        public override PassiveTooltip GetPassiveTooltip(Collider hitCollider)
        {
            PassiveTooltip passiveTooltip = base.GetPassiveTooltip(hitCollider);
            if (base.CurrentBuildStateIndex == BuildStates.Count - 1)
            {
                passiveTooltip.Title = DisplayName;
                passiveTooltip.State = DispenserInfo();
            }

            return passiveTooltip;
        }

        public string DispenserInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(MyGameStrings.NutrientDispenserStorage);
            stringBuilder.Append(" ");
            stringBuilder.Append(_stored.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append("(" + (100 * _stored/_capacity).ToStringPrefix("%", "yellow") + ")");

            if (_processing > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat(GameStrings.ProcessingThing, InputSlot.Get<Thing>().DisplayName);
                stringBuilder.Append(" ... ");
                stringBuilder.Append((100 * _processing).ToStringPrefix("%", "yellow"));
            }

            return stringBuilder.ToString();
        }

        public override StringBuilder GetExtendedText()
        {
            StringBuilder extendedText = base.GetExtendedText();

            if (Error == 1)
            {
                extendedText.AppendLine(MyGameStrings.CantProcessNutrient.DisplayString);
            }

            if (Error == 2)
            {
                extendedText.AppendLine(MyGameStrings.NutrientDispenserIsFull.DisplayString);
            }
            
            return extendedText;
        }

        private void EnableInputSlot()
        {
            this.InputSlot.Interactable.Collider.enabled = true;
            this.InputSlot.IsInteractable = true;
        }

        private void DisableInputSlot()
        {
            this.InputSlot.Interactable.Collider.enabled = false;
            this.InputSlot.IsInteractable = false;
        }

        public override void BuildUpdate(RocketBinaryWriter writer, ushort networkUpdateType)
        {
            base.BuildUpdate(writer, networkUpdateType);

            if (IsNetworkUpdateRequired(FLAG_MP_STORAGE, networkUpdateType))
                writer.WriteFloatHalf(_stored);
            if (IsNetworkUpdateRequired(FLAG_MP_PROCESSING, networkUpdateType))
                writer.WriteByte(_processing);
            if (IsNetworkUpdateRequired(FLAG_MP_PROCESSING_RATE, networkUpdateType))
                writer.WriteFloatHalf(_processingRate);
            if (IsNetworkUpdateRequired(FLAG_MP_TTR, networkUpdateType))
                writer.WriteFloatHalf(_timeToRefill);
            if (IsNetworkUpdateRequired(FLAG_MP_CAPACITY, networkUpdateType))
                writer.WriteFloatHalf(_capacity);
        }

        public override void ProcessUpdate(RocketBinaryReader reader, ushort networkUpdateType)
        {
            base.ProcessUpdate(reader, networkUpdateType);

            if (IsNetworkUpdateRequired(FLAG_MP_STORAGE, networkUpdateType))
                _stored = reader.ReadFloatHalf();
            if (IsNetworkUpdateRequired(FLAG_MP_PROCESSING, networkUpdateType))
                _processing = reader.ReadByte();
            if (IsNetworkUpdateRequired(FLAG_MP_PROCESSING_RATE, networkUpdateType))
                _processingRate = reader.ReadFloatHalf();
            if (IsNetworkUpdateRequired(FLAG_MP_TTR, networkUpdateType))
                _timeToRefill = reader.ReadFloatHalf();
            if (IsNetworkUpdateRequired(FLAG_MP_CAPACITY, networkUpdateType))
                _capacity = reader.ReadFloatHalf();
        }

        public override void SerializeOnJoin(RocketBinaryWriter writer)
        {
            base.SerializeOnJoin(writer);

            writer.WriteFloatHalf(_stored);
            writer.WriteFloatHalf(_processing);
            writer.WriteFloatHalf(_processingRate);
            writer.WriteFloatHalf(_timeToRefill);
            writer.WriteFloatHalf(_capacity);
        }

        public override void DeserializeOnJoin(RocketBinaryReader reader)
        {
            base.DeserializeOnJoin(reader);

            _stored = reader.ReadFloatHalf();
            _processing = reader.ReadByte();
            _processingRate = reader.ReadFloatHalf();
            _timeToRefill = reader.ReadFloatHalf();
            _capacity = reader.ReadFloatHalf();
        }

        public override ThingSaveData SerializeSave()
        {
            var saveData = new NutrientGelDispenserSaveData();
            var baseData = saveData as ThingSaveData;
            InitialiseSaveData(ref baseData);
            return saveData;
        }

        public override void DeserializeSave(ThingSaveData baseData)
        {
            base.DeserializeSave(baseData);
            if (baseData is not NutrientGelDispenserSaveData saveData)
                return;

            _stored = saveData.Storage;
            _capacity = saveData.Capacity;
            _processing = (byte)saveData.Processing;
            _processingRate = saveData.ProcessingRate;
            _timeToRefill = saveData.TimeToRefill;
        }

        protected override void InitialiseSaveData(ref ThingSaveData baseData)
        {
            base.InitialiseSaveData(ref baseData);
            if (baseData is not NutrientGelDispenserSaveData saveData)
                return;

            saveData.Storage = _stored;
            saveData.Capacity = _capacity;
            saveData.Processing = (int)_processing;
            saveData.ProcessingRate = _processingRate;
            saveData.TimeToRefill = _timeToRefill;
        }

        public override void OnFinishedLoad()
        {
            base.OnFinishedLoad();
        }

		private async UniTask ProcessTask()
		{
            CancellationToken cancelToken = base.gameObject.GetCancellationTokenOnDestroy();
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            if (!OnOff || !Powered)
            {
                return;
            }

            _toProcess = InputSlot.Get<Food>();
            if (_toProcess == null)
            {
                return;
            }

            Debug.Log("[NutrientGelDispenser] Starting to process...");

            var _nutrientsToConsume = Mathf.Min(_capacity - _stored, _toProcess.Quantity * _toProcess.NutritionValue);
            var _quantityToConsume = _nutrientsToConsume / _toProcess.NutritionValue;
            var _nutrientsToAdd =  _nutrientsToConsume * 0.5f;

            Debug.Log($"[NutrientGelDispenser] Will consume {_nutrientsToConsume} from {_toProcess.PrefabName}");

            DisableInputSlot();

            float complete = 0f;
            float length = _nutrientsToConsume / _processingRate;
            while (complete < length)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                if (!OnOff || !Powered)
                {
                    break;
                }

                complete += Time.deltaTime;
                Processing = (byte)(Mathf.Clamp01(complete / length) * 100f); 
                await UniTask.NextFrame(cancelToken);
            }

            if (complete >= length && IsOperable && _toProcess != null)
            {
                Processing = 0;
                _toProcess.SetQuantity(_toProcess.Quantity - _quantityToConsume);
                AddNutrientsToStorage(_nutrientsToAdd);
            }

            RefreshAnimState();
            EnableInputSlot();
        }

        public void AddNutrientsToStorage(float num)
        {
            Stored = Mathf.Min(_capacity, _stored + num);

            RefreshAnimState();
        }

        public override void OnInteractableUpdated(Interactable interactable)
		{
			base.OnInteractableUpdated(interactable);

            Debug.Log("[NutrientGelDispenser] Added item to input slot");
            if (GameManager.RunSimulation && this._processingTask.Status != UniTaskStatus.Pending && this.IsOperable)
            {
                Debug.Log("[NutrientGelDispenser] Starting...");
                _processingTask = ProcessTask();
            }
            else
            {
                Debug.Log("[NutrientGelDispenser] Failed to start!");
            }

            RefreshAnimState();
		}
        

        protected override void RefreshAnimState(bool skipAnimation = false)
        {
            base.RefreshAnimState(skipAnimation);
            if (_ledQuantityScreen != null)
            {
                _ledQuantityScreen.RefreshState(_stored / _capacity, this.Powered);
            }

            if (_progressLights != null)
            {
                int state = 1;
                state = _processing > 0 ? 2 : state;
                state = Error != 0 ? 3 : state;
                state = OnOff ? state : 0;
                _progressLights.ChangeState(state);
            }
        }

        public override void OnChildEnterInventory(DynamicThing newChild)
		{
			base.OnChildEnterInventory(newChild);
            RefreshAnimState();

            if (newChild.ParentSlot == InputSlot)
            {
                Debug.Log("[NutrientGelDispenser] Added item to input slot");
                if (GameManager.RunSimulation && this._processingTask.Status != UniTaskStatus.Pending && this.IsOperable)
                {
                    Debug.Log("[NutrientGelDispenser] Starting...");
                    _processingTask = ProcessTask();
                }
                else
                {
                    Debug.Log("[NutrientGelDispenser] Failed to start!");
                }
            }
            
		}

        public override void OnChildExitInventory(DynamicThing previousChild)
		{
			base.OnChildExitInventory(previousChild);
            RefreshAnimState();

            _ = IsOperable;
		}

        private void RefillTick()
        {
            if (_stored  == 0)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                var slot = Slots[i];
                if (slot.Contains<NutrientGel>(out var occupant))
                {
                    if (occupant == null || occupant.Quantity >= occupant.MaxQuantity)
                    {
                        continue;
                    }

                    float fill = Mathf.Clamp(Time.deltaTime / _timeToRefill, 0, Mathf.Min(_stored, occupant.MaxQuantity - occupant.Quantity));
                    Stored -= fill * occupant.NutritionValue;
                    occupant.Quantity += fill;
                }
            }
        }

        public override void UpdateEachFrame()
        {
            base.UpdateEachFrame();
            if (!OnOff || !Powered)
            {
                return;
            }

            RefillTick();

            // if (!(num2 <= 0f))
            // {
            //     _sealedWaterFill?.SetPitchMultiplier(Mathf.Lerp(0.75f, 2f, num / num2));
            // }
        }
    }
}
