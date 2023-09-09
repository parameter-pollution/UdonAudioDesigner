
using UdonSharp;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System.Numerics;
using VRC.SDKBase.Midi;

namespace AudioDesigner
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class AudioSources : UdonSharpBehaviour
    {

        public GameObject spawn;
        public Button spawnAudioSourceButton;
        public Text audioSourceListStatus;
        public GameObject[] editableAudioSources;

        [UdonSynced, FieldChangeCallback(nameof(ActiveAudioSources))]
        private uint _activeAudioSources = 0;

        private float RAND_MIN = 0.02f;
        private float RAND_MAX = 0.1f;
        void Start()
        {
            UpdateAudioSourceListStatus();
            for (int i = 0; i < editableAudioSources.Length; i++)
            {
                EditableAudioSource source = editableAudioSources[i].GetComponent<EditableAudioSource>();
                source.SetID(i);
            }
        }

        public uint ActiveAudioSources
        {
            set
            {
                Debug.Log("AudioDesigner: ActiveAudioSources changed to: "+PackedBoolArrayToString(value,editableAudioSources.Length));
                _activeAudioSources = value;

                for (int i = 0; i < editableAudioSources.Length; i++)
                {
                    EditableAudioSource source = editableAudioSources[i].GetComponent<EditableAudioSource>();
                    if( GetPackedBoolArrayState(value,i) ){
                        if(!source.isVisible){
                            source.Show();
                        }
                    }else{
                        source.Hide();
                    }
                    
                }     
                UpdateAudioSourceListStatus();
                if( GetNumberOfActiveAudioSources() >= editableAudioSources.Length ){
                    spawnAudioSourceButton.interactable = false;
                }else{
                    spawnAudioSourceButton.interactable = true;
                }
            }
            get => _activeAudioSources;
        }

        private void BecomeOwner(){
            if(!Networking.IsOwner(gameObject)){
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
        }

        public int SpawnAudioSource(){
            if( GetNumberOfActiveAudioSources() < editableAudioSources.Length ){
                BecomeOwner();
                for (int i = 0; i < editableAudioSources.Length; i++)
                {
                    if( !GetPackedBoolArrayState(ActiveAudioSources,i) ){
                        ActiveAudioSources = SetPackedBoolArrayState(ActiveAudioSources,i,true);

                        EditableAudioSource source = editableAudioSources[i].GetComponent<EditableAudioSource>();
                        source.ResetSettings();
                        Vector3 spawnPosition = spawn.transform.position;
                        spawnPosition += new Vector3(Random.Range(RAND_MIN,RAND_MAX), Random.Range(RAND_MIN,RAND_MAX), Random.Range(RAND_MIN,RAND_MAX));
                        Vector3 spawnRotation = spawn.transform.eulerAngles;
                        source.SetAudioSourcePosition(spawnPosition + spawn.transform.up * 0.3f );
                        source.SetAudioSourceRotation(new Vector3(0f,0f,0f));
                        source.SetMenuPosition(spawnPosition);
                        source.SetMenuRotation(spawnRotation);

                        return i;
                    } 
                }
            }
            return -1;
        }

        private void UpdateAudioSourceListStatus(){
            audioSourceListStatus.text = "("+GetNumberOfActiveAudioSources()+"/"+editableAudioSources.Length+" spawned)";
        }

        private int GetNumberOfActiveAudioSources(){
            return CountActiveStateInPackedBoolArray(ActiveAudioSources,editableAudioSources.Length);
        }

        public void DisableAudioSource(int index){
            Debug.Log("AudioDesigner: AudioSources disabling audio sourc with ID: "+index);
            BecomeOwner();
            ActiveAudioSources = SetPackedBoolArrayState(ActiveAudioSources,index,false);
        }

        public void DisableAllAudioSources(){
            BecomeOwner();
            ActiveAudioSources = 0;
        }

        //utility functions:

        private bool GetPackedBoolArrayState(uint packedArray, int index){
            return ((packedArray >> index) & 1) == 1;
        }

        private uint SetPackedBoolArrayState(uint packedArray, int index, bool value){
            if(value) return packedArray | ((uint)1 << index);
            return packedArray & ~((uint)1 << index);
        }

        private int CountActiveStateInPackedBoolArray(uint packedArray, int max){
            int count = 0;
            for (int i = 0; i < max; i++)
            {
                if( GetPackedBoolArrayState(packedArray,i) ) count++;
            }
            return count;
        }

        private string PackedBoolArrayToString(uint packedArray, int digits){
            string result ="";
            for (int i = digits-1; i >= 0; i--)
            {
                if( GetPackedBoolArrayState(packedArray,i) ) result+="1";
                else result+="0";
            }
            return result;
        }

    }
}