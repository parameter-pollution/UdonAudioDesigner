
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using UnityEngine.UI;
using System.Data;
using VRC.SDKBase.Midi;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections.Generic;
#endif

namespace AudioDesigner
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class ObjectMenu : UdonSharpBehaviour
    {
        public GameObject soundSourceGrabable;
        public GameObject soundSource;
        public GameObject menuHandle;

        public AudioDesigner audioDesigner;
        public EditableAudioSource editableAudioSource;

        public GameObject videoPlayerScreen;
        public GameObject videoPlayerUI;
        public GameObject videoPlayerImportHelperPanel;
        public InputField videoPlayerImportHelperField;
        public UdonSharp.Video.USharpVideoPlayer videoPlayerAPI;

        public Dropdown audioClipDropdown;
        public Dropdown modeDropdown;
        public Slider volumeSlider;
        public Text volumeValue;
        public Slider volumetricRadiusSlider;
        public Text volumetricRadiusValue;
        public Toggle volumetricRadiusToggle;
        public GameObject volumetricRadiusGizmo;
        public Slider gainSlider;
        public Text gainValue;
        public Slider nearSlider;
        public Text nearValue;
        public Toggle nearToggle;
        public GameObject nearGizmo;
        public Slider farSlider;
        public Text farValue;
        public Toggle farToggle;
        public GameObject farGizmo;
        public Dropdown volumeFalloffDropdown;
        public Slider pitchSlider;
        public Text pitchValue;
        public Slider highPassCutoffSlider;
        public Text highPassCutoffValue;
        public Toggle highPassToggle;
        public Slider lowPassCutoffSlider;
        public Text lowPassCutoffValue;
        public Toggle lowPassToggle;
        public Dropdown reverbFilterDropdown;
        public Toggle reverbFilterToggle;
        public Text objectNameLabel;
        public Text positionValues;
        public Text rotationValues;
        public GameObject[] hideForHeadhphoneMode;
        public GameObject[] hideForVideoplayerSource;

        public AudioSource audioSource;
        public AudioHighPassFilter audioHighPassFilter;
        public AudioLowPassFilter audioLowPassFilter;
        public AudioReverbFilter audioReverbFilter;
        public LineRenderer connectionLine;
        public GameObject importExportMenu;
        public InputField importExportTextField;
        public Text importExportLogField;
        public Toggle importIgnorePosRotToggle;

        // ########## network synced variables
        
        [UdonSynced(UdonSyncMode.Linear)]
        private float playbackTimestamp = 0.0f;

        private int AUDIO_MODE_SPATIALIZED = 0;
        private int AUDIO_MODE_HEADPHONE = 1;
        string[] AUDIO_MODE_EXPORT_NAME = {
            "SPATIALIZED",
            "HEADPHONE"
        };
        [UdonSynced, FieldChangeCallback(nameof(AudioMode))]
        private int _audioMode = -1;

        [UdonSynced, FieldChangeCallback(nameof(IsPlaying))]
        private bool _isPlaying = false;
        string AUDIO_CLIP_TYPE_LOCAL = "LOCAL";
        string AUDIO_CLIP_TYPE_URL = "URL";
        [UdonSynced, FieldChangeCallback(nameof(AudioClipID))]
        private int _audioClipID = -1;
        [UdonSynced, FieldChangeCallback(nameof(Volume))]
        private float _volume = -1f;
        [UdonSynced, FieldChangeCallback(nameof(Gain))]
        private float _gain = -1f;
        [UdonSynced, FieldChangeCallback(nameof(VolumetricRadius))]
        private float _volumetricRadius = -1f;
        [UdonSynced, FieldChangeCallback(nameof(VolumetricRadiusVisualization))]
        private bool _volumetricRadiusVisualization = false;

        private int VOLUME_FALLOFF_MODE_OCULUS = 0;
        private int VOLUME_FALLOFF_MODE_LOGARITHMIC = 1;
        private int VOLUME_FALLOFF_MODE_LINEAR = 2;
        string[] VOLUME_FALLOFF_MODE_EXPORT_NAME = {
            "OCULUS",
            "LOGARITHMIC",
            "LINEAR"
        };
        [UdonSynced, FieldChangeCallback(nameof(VolumeFalloffMode))]
        private int _volumeFalloffMode = -1;

        [UdonSynced, FieldChangeCallback(nameof(Near))]
        private float _near = -1f;
        [UdonSynced, FieldChangeCallback(nameof(NearRadiusVisualization))]
        private bool _nearRadiusVisualization = false;
        [UdonSynced, FieldChangeCallback(nameof(Far))]
        private float _far = -1f;
        [UdonSynced, FieldChangeCallback(nameof(FarRadiusVisualization))]
        private bool _farRadiusVisualization = false;
        [UdonSynced, FieldChangeCallback(nameof(Pitch))]
        private float _pitch = -1f;
        [UdonSynced, FieldChangeCallback(nameof(HighPassFreq))]
        private float _highPassFreq = -1f;
        [UdonSynced, FieldChangeCallback(nameof(HighPassEnabled))]
        private bool _highPassEnabled = false;
        [UdonSynced, FieldChangeCallback(nameof(LowPassFreq))]
        private float _lowPassFreq = -1f;
        [UdonSynced, FieldChangeCallback(nameof(LowPassEnabled))]
        private bool _lowPassEnabled = false;
        [UdonSynced, FieldChangeCallback(nameof(ReverbPreset))]
        private int _reverbPreset = -1;
        [UdonSynced, FieldChangeCallback(nameof(ReverbEnabled))]
        private bool _reverbEnabled = false;
        

        // ####### oculus spatializer defines for SetSpatializerFloat function
        private int OCULUS_SPATIALIZER_PARAMETER_GAIN = 0;
        private int OCULUS_SPATIALIZER_PARAMETER_USEINVSQR = 1;
        private int OCULUS_SPATIALIZER_PARAMETER_NEAR = 2;
        private int OCULUS_SPATIALIZER_PARAMETER_FAR = 3;
        private int OCULUS_SPATIALIZER_PARAMETER_RADIUS = 4;
        private int OCULUS_SPATIALIZER_PARAMETER_DISABLE_RFL = 5;
        //private int OCULUS_SPATIALIZER_PARAMETER_VSPEAKERMODE = 6;
        //private int OCULUS_SPATIALIZER_PARAMETER_AMBISTAT = 7;
        //private int OCULUS_SPATIALIZER_PARAMETER_READONLY_GLOBAL_RFL_ENABLED = 8; // READ-ONLY
        //private int OCULUS_SPATIALIZER_PARAMETER_READONLY_NUM_VOICES = 9; // READ-ONLY
        private int OCULUS_SPATIALIZER_PARAMETER_SENDLEVEL = 10;
        //private int OCULUS_SPATIALIZER_PARAMETER_NUM = 11;

        private float FILTER_CUTOFF_MIN = 10f;
        private float FILTER_CUTOFF_MAX = 22000f;
        private float FAR_MIN_DISTANCE = 2f;
        private float FAR_MAX_DISTANCE = 500f;

        int[] reverbPresets = {
            (int)AudioReverbPreset.Off,
            (int)AudioReverbPreset.Generic,
            (int)AudioReverbPreset.PaddedCell,
            (int)AudioReverbPreset.Room,
            (int)AudioReverbPreset.Bathroom,
            (int)AudioReverbPreset.Livingroom,
            (int)AudioReverbPreset.Stoneroom,
            (int)AudioReverbPreset.Auditorium,
            (int)AudioReverbPreset.Concerthall,
            (int)AudioReverbPreset.Cave,
            (int)AudioReverbPreset.Arena,
            (int)AudioReverbPreset.Hangar,
            (int)AudioReverbPreset.CarpetedHallway,
            (int)AudioReverbPreset.Hallway,
            (int)AudioReverbPreset.StoneCorridor,
            (int)AudioReverbPreset.Alley,
            (int)AudioReverbPreset.Forest,
            (int)AudioReverbPreset.City,
            (int)AudioReverbPreset.Mountains,
            (int)AudioReverbPreset.Quarry,
            (int)AudioReverbPreset.Plain,
            (int)AudioReverbPreset.ParkingLot,
            (int)AudioReverbPreset.SewerPipe,
            (int)AudioReverbPreset.Underwater,
            (int)AudioReverbPreset.Drugged,
            (int)AudioReverbPreset.Dizzy,
            (int)AudioReverbPreset.Psychotic
        };
        string[] REVERB_PRESET_EXPORTNAME = {
            "Off",
            "Generic",
            "PaddedCell",
            "Room",
            "Bathroom",
            "Livingroom",
            "Stoneroom",
            "Auditorium",
            "Concerthall",
            "Cave",
            "Arena",
            "Hangar",
            "CarpetedHallway",
            "Hallway",
            "StoneCorridor",
            "Alley",
            "Forest",
            "City",
            "Mountains",
            "Quarry",
            "Plain",
            "ParkingLot",
            "SewerPipe",
            "Underwater",
            "Drugged",
            "Dizzy",
            "Psychotic"
        };

        private DataDictionary audioSourceConfig = new DataDictionary()
        {
            { "Position", new DataDictionary()
                {
                    {"x", 0.0f},
                    {"y", 0.0f},
                    {"z", 0.0f}
                }
            },
            { "Rotation", new DataDictionary()
                {
                    {"x", 0.0f},
                    {"y", 0.0f},
                    {"z", 0.0f}
                }
            },
            { "MenuPosition", new DataDictionary()
                {
                    {"x", 0.0f},
                    {"y", 0.0f},
                    {"z", 0.0f}
                }
            },
            { "MenuRotation", new DataDictionary()
                {
                    {"x", 0.0f},
                    {"y", 0.0f},
                    {"z", 0.0f}
                }
            },
            { "IsPlaying", false},
            { "AudioMode", ""},
            { "AudioClipType", ""},
            { "AudioClip", ""},
            { "Volume", 1f},
            { "Gain", 10f},
            { "VolumetricRadius", 0.1f},
            { "VolumetricRadiusVisualization", false},
            { "VolumeFalloffMode", ""},
            { "Near", 0.1f},
            { "NearVisualization", false},
            { "Far", 200f},
            { "FarVisualization", false},
            { "Pitch", 1f},
            { "Highpass", new DataDictionary()
                {
                    {"enabled", false},
                    {"cutoffFreq", 0f}
                }
            },
            { "Lowpass", new DataDictionary()
                {
                    {"enabled", false},
                    {"cutoffFreq", 0f}
                }
            },
            { "Reverb", new DataDictionary()
                {
                    {"enabled", false},
                    {"FilterPreset", ""}
                }
            }
        };

        private DataDictionary pendingConfig = null;
        private bool pendingIgnorePosRot = false;

        private float _nextSlowUpdateTime = 0f;
        private float _slowUpdateInterval = 0.1f;
        private float _nextReallySlowUpdateTime = 0f;
        private float _reallySlowUpdateInterval = 0.5f;
        private float MAX_ALLOWED_PLAYBACK_TIMESTAMP_DESYNC = 1.0f;

        private bool _initDone = false;

        //is always called in the beginning even if the object is inactive
        void Awake(){
            connectionLine.positionCount = 2;
            SetOculusSpatializerValues();
        }

        void Start()
        {
            //objectNameLabel.text = soundSourceGrabable.name;

        }


//in udonsharp we can't use Lists, so we have to populate the dropdown with the audio clips added in the audio designer while in editor
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    public void InitializeAudioClipDropdown(List<Dropdown.OptionData> dropDownList){
        audioClipDropdown.ClearOptions();
        audioClipDropdown.AddOptions(dropDownList);
    }
#endif

        void Update(){
            if( _initDone == false && Networking.IsNetworkSettled  ){
                Init();
                _initDone = true;
            }

            connectionLine.SetPosition(0, transform.position);
            connectionLine.SetPosition(1, soundSourceGrabable.transform.position);

            //stuff that should be updated less frequently
            if( Time.time >= _nextSlowUpdateTime ){
                _nextSlowUpdateTime = Time.time + _slowUpdateInterval;

                positionValues.text = "x: "+soundSourceGrabable.transform.position.x.ToString("F3")+" y: "+soundSourceGrabable.transform.position.y.ToString("F3")+" z: "+soundSourceGrabable.transform.position.z.ToString("F3");
                rotationValues.text = "x: "+soundSourceGrabable.transform.rotation.x.ToString("F3")+" y: "+soundSourceGrabable.transform.rotation.y.ToString("F3")+" z: "+soundSourceGrabable.transform.rotation.z.ToString("F3");
            }
            if( Time.time >= _nextReallySlowUpdateTime ){
                _nextReallySlowUpdateTime = Time.time + _reallySlowUpdateInterval;
            
                if( AmIOwner() ){
                    //object owner regularly sends local playback timestamp to others
                    playbackTimestamp = audioSource.time;
                }else{
                    if( Mathf.Abs(audioSource.time - playbackTimestamp) > MAX_ALLOWED_PLAYBACK_TIMESTAMP_DESYNC ){
                        Debug.Log("AudioDesigner: local playback timestamp too out of sync with owner => resync");
                        if( IsPlaying && audioSource.isPlaying == false ) audioSource.Play();
                        audioSource.time = playbackTimestamp;
                    }
                }
            }
        }

        private void Init(){
            if( AmIOwner() ){
                if( pendingConfig == null ){
                    LoadDefaults();
                }else{
                    ImportConfig(pendingConfig, pendingIgnorePosRot);
                    pendingConfig = null;
                }
                playbackTimestamp = audioSource.time;
            }else{
                if(AudioMode == AUDIO_MODE_SPATIALIZED){
                    SetOculusSpatializerValues();
                }
                if( IsPlaying ) audioSource.Play();
                audioSource.time = playbackTimestamp;
                videoPlayerAPI.ForceSyncVideo();
            }
        }

        public void Cleanup(){
            audioSource.Stop();
            audioSource.time = 0f;
            videoPlayerAPI.StopVideo();
            importExportMenu.SetActive(false);
        }

        public void OnResetButton(){
            LoadDefaults();
        }

        public void LoadDefaults(){
            BecomeOwner();
            playbackTimestamp = 0f;
            IsPlaying = false;
            AudioMode = AUDIO_MODE_SPATIALIZED;
            AudioClipID = 0;
            Volume = 1.0f;
            Gain = 10.0f;
            VolumetricRadius = 0.1f;
            VolumeFalloffMode = VOLUME_FALLOFF_MODE_OCULUS;
            Near = 0.1f;
            Far = 200f;
            Pitch = 1.0f;
            HighPassEnabled = false;
            HighPassFreq = 0f;
            LowPassEnabled = false;
            LowPassFreq = 22000f;
            ReverbEnabled = false;
            ReverbPreset = 8;
        }

        
        public void SetOculusSpatializerValues(){
    
            //bool useInvSqr = true;
            bool enableRfl = false;
            float reverbSendValue = 0f;
            float reverbSend = Mathf.Clamp( reverbSendValue, -60.0f, 20.0f);

            // See if we should enable spatialization
            audioSource.spatialize = true;
            
            audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_GAIN, Gain);
            ApplyFalloffMode(VolumeFalloffMode);

            audioSource.SetSpatializerFloat( OCULUS_SPATIALIZER_PARAMETER_NEAR, Near);
            audioSource.minDistance = Near;
            audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_FAR, Far);
            audioSource.maxDistance = Far;

            audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_RADIUS, VolumetricRadius);

            if(enableRfl == true)
                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_DISABLE_RFL, 0.0f);
            else
                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_DISABLE_RFL, 1.0f);

            audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_SENDLEVEL, reverbSend);
        }

        private bool BecomeOwner(){
            if(!Networking.IsOwner(gameObject)){
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
                bool result = Networking.IsOwner(gameObject);
                Debug.Log("BecomeOwner(): result: "+result);
                return result;
            }else{
                Debug.Log("BecomeOwner(): already owner");
                return true;
            }
        }

        private bool AmIOwner(){
            return Networking.IsOwner(gameObject);
        }

        // ######### Mode selector
        public int AudioMode
        {
            set
            {
                Debug.Log("AudioDesigner: AudioMode State changed to: "+value);
                _audioMode = value;

                if( value == AUDIO_MODE_SPATIALIZED ){
                    //we want spatialized mode
                    SetOculusSpatializerValues();
                    audioSource.spatialBlend = 1f;
                    foreach (GameObject uiElement in hideForHeadhphoneMode)
                    {
                        uiElement.SetActive(true);
                    }
                    //we switched to spatialize mode => update stat of spatial gizmos
                    nearGizmo.SetActive(NearRadiusVisualization);
                    farGizmo.SetActive(FarRadiusVisualization);
                }else if( value == AUDIO_MODE_HEADPHONE ){
                    //we want headphone mode
                    audioSource.spatialize = false;
                    audioSource.spatialBlend = 0f;
                    foreach (GameObject uiElement in hideForHeadhphoneMode)
                    {
                        uiElement.SetActive(false);
                    }
                    nearGizmo.SetActive(false);
                    farGizmo.SetActive(false);
                }

                modeDropdown.SetValueWithoutNotify(value);
            }
            get => _audioMode;
        }
        public void OnAudioModeDropdownSelect(){
            if( BecomeOwner() ) AudioMode = modeDropdown.value;
        }

        // ######### Play/Stop

        public bool IsPlaying
        {
            set
            {
                Debug.Log("AudioDesigner: isPlaying State changed to: "+value);
                _isPlaying = value;
                
                if( _initDone ){
                    if(value){
                        if(AudioMode == AUDIO_MODE_SPATIALIZED){
                            SetOculusSpatializerValues();
                        }
                        audioSource.Play();
                        audioSource.time = playbackTimestamp;
                    }else{
                        audioSource.Stop();
                    }
                }
            }
            get => _isPlaying;
        }

        public void OnPlayButton(){
            if( BecomeOwner() ){
                IsPlaying = true;
                playbackTimestamp = audioSource.time;
            }
        }

        public void OnStopButton(){
            if( BecomeOwner() ) IsPlaying = false;
        }

        // ######### Audio Clip selection
        public int AudioClipID
        {
            set
            {
                Debug.Log("AudioDesigner: AudioClipID State changed to: "+value);
                
                int previousAudioClipID = _audioClipID;

                _audioClipID = value;

                int urlClipDropdownID = 0;

                //if previous selection was URL/videoplayer
                if( previousAudioClipID == urlClipDropdownID ){
                    if(AmIOwner()) videoPlayerAPI.StopVideo();
                    videoPlayerScreen.SetActive(false);
                    videoPlayerUI.SetActive(false);
                    foreach (GameObject uiElement in hideForVideoplayerSource)
                    {
                        uiElement.SetActive(true);
                    }
                }

                //if URL/Video player was selected as audio clip (last entry in the list => index is the length of the list of the actual audio clips)
                if( value == urlClipDropdownID ){
                    audioSource.Stop();
                    videoPlayerScreen.SetActive(true);
                    videoPlayerUI.SetActive(true);
                    foreach (GameObject uiElement in hideForVideoplayerSource)
                    {
                        uiElement.SetActive(false);
                    }
                    //in case an ambisonic sound clip was active before switching to the video player
                    modeDropdown.interactable = true;
                //an actual audio clip has been selected    
                }else{

                    bool wasAmbisonic = false;
                    if( audioSource.clip && audioSource.clip.ambisonic){
                        wasAmbisonic = true;
                    }

                    if( audioDesigner.audioClips[_audioClipID-1].ambisonic ){
                        if( AmIOwner() ){
                            AudioMode = AUDIO_MODE_HEADPHONE;
                        }
                        //make sure that spatialization is off for ambisonics
                        audioSource.spatialize = false;
                        modeDropdown.interactable = false;
                    }else{
                        if( AmIOwner() ){
                            //switch to spatialized mode as the default after having just used an ambisonic clip
                            if( wasAmbisonic ){
                                AudioMode = AUDIO_MODE_SPATIALIZED;
                            }
                        }
                        modeDropdown.interactable = true;
                    }

                    audioSource.clip = audioDesigner.audioClips[_audioClipID-1];
                    //if audio was playing while switching the audio clip then automatically start playing the new clip too, so the player doesn't have to press the play button every time the clip is changed
                    //(switching the audio clip on an audio source stops audio source playback)
                    if( IsPlaying ){
                        if(AudioMode == AUDIO_MODE_SPATIALIZED){
                            SetOculusSpatializerValues();
                        }
                        audioSource.Play();
                        if( AmIOwner() ){
                            playbackTimestamp = audioSource.time;
                        }else{
                            audioSource.time = playbackTimestamp;
                        }
                    }else{
                        //if the user just switched to a new clip and it wasn't playing before just start playback. that's probably what the user is going to do next anyway
                        if( AmIOwner() ) IsPlaying = true;
                    }
                }


                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                audioClipDropdown.SetValueWithoutNotify(_audioClipID);
            }
            get => _audioClipID;
        }

        public void OnAudioClipDropdownSelect(){
            if( BecomeOwner() ){
                //playbackTimestamp = 0f;
                AudioClipID = audioClipDropdown.value;
            }
        }

        // ######## volume

        public float Volume
        {
            set
            {
                Debug.Log("AudioDesigner: Volume value changed to: "+value);
                _volume = value;

                audioSource.volume = value; //volumeSlider.value;
                volumeValue.text = value.ToString("F2");
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                volumeSlider.SetValueWithoutNotify(value);
            }
            get => _volume;
        }

        public void OnVolumeSliderValueChanged(){
            if( BecomeOwner() ) Volume = volumeSlider.value;
        }

        // ########## Gain

        public float Gain
        {
            set
            {
                Debug.Log("AudioDesigner: Gain value changed to: "+value);
                _gain = value;

                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_GAIN, value);
                gainValue.text = value.ToString("F2");
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                gainSlider.SetValueWithoutNotify(value);
            }
            get => _gain;
        }

        public void OnGainSliderValueChanged(){
            if( BecomeOwner() ) Gain = gainSlider.value;
        }

        // ########### Volumentric Radius
        public float VolumetricRadius
        {
            set
            {
                Debug.Log("AudioDesigner: Volumentric Radius value changed to: "+value);
                _volumetricRadius = value;

                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_RADIUS, value);
                SetGlobalScale(volumetricRadiusGizmo,value*2);
                volumetricRadiusValue.text = value.ToString("F2");
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                volumetricRadiusSlider.SetValueWithoutNotify(value);
            }
            get => _volumetricRadius;
        }
        public void OnVolumetricRadiusSliderValueChanged(){
            if( BecomeOwner() ) VolumetricRadius = volumetricRadiusSlider.value;
        }

        public bool VolumetricRadiusVisualization
        {
            set
            {
                Debug.Log("AudioDesigner: Volumentric Radius visualization value changed to: "+value);
                _volumetricRadiusVisualization = value;

                volumetricRadiusGizmo.SetActive(value);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                volumetricRadiusToggle.SetIsOnWithoutNotify(value);
            }
            get => _volumetricRadiusVisualization;
        }

        public void SetVolumetricRadiusVisualization(){
            if( BecomeOwner() ) VolumetricRadiusVisualization = volumetricRadiusToggle.isOn;
        }

        // ######  Volume Falloff Mode

        public int VolumeFalloffMode
        {
            set
            {
                Debug.Log("AudioDesigner: Volume Falloff Mode State changed to: "+value);
                _volumeFalloffMode = value;

                ApplyFalloffMode(value);

                //no idea why but for logarithmic mode you have to set the near value again after switching to it and also set it to > 0 or you won't hear anything
                if( value == VOLUME_FALLOFF_MODE_LOGARITHMIC && AmIOwner() ){
                    if( Near < 0.01f ){
                        Near = 0.01f;
                    }else{
                        Near = Near;
                    }
                }
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                volumeFalloffDropdown.SetValueWithoutNotify(value);
            }
            get => _volumeFalloffMode;
        }

        private void ApplyFalloffMode(int mode){
            if( mode == VOLUME_FALLOFF_MODE_OCULUS ){
                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_USEINVSQR, 1.0f);
            }else if( mode == VOLUME_FALLOFF_MODE_LOGARITHMIC ){
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_USEINVSQR, 0.0f);
            } else if( mode == VOLUME_FALLOFF_MODE_LINEAR ){
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_USEINVSQR, 0.0f);
            }
        }

        public void OnVolumeFalloffDropdownSelect(){
            if( BecomeOwner() ) VolumeFalloffMode = volumeFalloffDropdown.value;
        }

        // ############ Near

        public float Near
        {
            set
            {
                Debug.Log("AudioDesigner: Near value changed to: "+value);
                _near = value;

                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_NEAR, value);
                audioSource.minDistance = value;
                SetGlobalScale(nearGizmo, value*2);
                nearValue.text = value.ToString("F2");
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                nearSlider.SetValueWithoutNotify(value);
            }
            get => _near;
        }

        public void OnNearSliderValueChanged(){
            if( BecomeOwner() ) Near = nearSlider.value;
        }

        public bool NearRadiusVisualization
        {
            set
            {
                Debug.Log("AudioDesigner: Near Radius visualization value changed to: "+value);
                _nearRadiusVisualization = value;

                nearGizmo.SetActive(value);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                nearToggle.SetIsOnWithoutNotify(value);
            }
            get => _nearRadiusVisualization;
        }

        public void SetNearVisualization(){
            if( BecomeOwner() ) NearRadiusVisualization = nearToggle.isOn;
        }        

        // ############ Far
        
        public float Far
        {
            set
            {
                Debug.Log("AudioDesigner: Far value changed to: "+value);
                _far = value;

                audioSource.SetSpatializerFloat(OCULUS_SPATIALIZER_PARAMETER_FAR, value);
                audioSource.maxDistance = value;
                SetGlobalScale(farGizmo, value*2);
                farValue.text = value.ToString("F2");

                float rescaledValue = InverseRescaleLogarithmic(value, FAR_MIN_DISTANCE, FAR_MAX_DISTANCE);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                farSlider.SetValueWithoutNotify(rescaledValue);
            }
            get => _far;
        }

        public void OnFarSliderValueChanged(){
            if( BecomeOwner() ) Far =  Mathf.Round(RescaleLogarithmic(farSlider.value, FAR_MIN_DISTANCE, FAR_MAX_DISTANCE));
        }

        public bool FarRadiusVisualization
        {
            set
            {
                Debug.Log("AudioDesigner: Far Radius visualization value changed to: "+value);
                _farRadiusVisualization = value;

                farGizmo.SetActive(value);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                farToggle.SetIsOnWithoutNotify(value);
            }
            get => _farRadiusVisualization;
        }

        public void SetFarVisualization(){
            if( BecomeOwner() ) FarRadiusVisualization = farToggle.isOn;
        }

        // ########## Pitch

        public float Pitch
        {
            set
            {
                Debug.Log("AudioDesigner: Pitch value changed to: "+value);
                _pitch = value;

                audioSource.pitch = value;
                pitchValue.text = value.ToString("F2");

                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                pitchSlider.SetValueWithoutNotify(value);
            }
            get => _pitch;
        }

        public void OnPitchSliderValueChanged(){
            if( BecomeOwner() ) Pitch = pitchSlider.value;
        }

        public void OnResetPitchButton(){
            if( BecomeOwner() ) Pitch = 1.0f;
        }

        // ############ High Pass

        public float HighPassFreq
        {
            set
            {
                Debug.Log("AudioDesigner: High Pass Fequency value changed to: "+value);
                _highPassFreq = value;

                audioHighPassFilter.cutoffFrequency = value;
                highPassCutoffValue.text = value.ToString();

                float rescaledValue = InverseRescaleLogarithmic(value, FILTER_CUTOFF_MIN, FILTER_CUTOFF_MAX);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                highPassCutoffSlider.SetValueWithoutNotify(rescaledValue);
            }
            get => _highPassFreq;
        }

        public void OnHighPassCutoffSliderValueChanged(){
            if( BecomeOwner() ) HighPassFreq = Mathf.Round(RescaleLogarithmic(highPassCutoffSlider.value, FILTER_CUTOFF_MIN, FILTER_CUTOFF_MAX));
        }

        public bool HighPassEnabled
        {
            set
            {
                Debug.Log("AudioDesigner: High Pass Enabled value changed to: "+value);
                _highPassEnabled = value;

                audioHighPassFilter.enabled = value;
                highPassCutoffSlider.interactable = value;
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                highPassToggle.SetIsOnWithoutNotify(value);
            }
            get => _highPassEnabled;
        }

        public void OnHighPassToggle(){
            if( BecomeOwner() ) HighPassEnabled = highPassToggle.isOn;
        }

        // ############ Low Pass

        public float LowPassFreq
        {
            set
            {
                Debug.Log("AudioDesigner: Low Pass Fequency value changed to: "+value);
                _lowPassFreq = value;

                audioLowPassFilter.cutoffFrequency = value;
                lowPassCutoffValue.text = value.ToString();

                float rescaledValue = InverseRescaleLogarithmic(value, FILTER_CUTOFF_MIN, FILTER_CUTOFF_MAX);
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                lowPassCutoffSlider.SetValueWithoutNotify(rescaledValue);
            }
            get => _lowPassFreq;
        }

        public void OnLowPassCutoffSliderValueChanged(){
            if( BecomeOwner() ) LowPassFreq = Mathf.Round(RescaleLogarithmic(lowPassCutoffSlider.value, FILTER_CUTOFF_MIN, FILTER_CUTOFF_MAX));
        }

        public bool LowPassEnabled
        {
            set
            {
                Debug.Log("AudioDesigner: Low Pass Enabled value changed to: "+value);
                _lowPassEnabled = value;

                audioLowPassFilter.enabled = value;
                lowPassCutoffSlider.interactable = value;
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                lowPassToggle.SetIsOnWithoutNotify(value);
            }
            get => _lowPassEnabled;
        }

        public void OnLowPassToggle(){
            if( BecomeOwner() ) LowPassEnabled = lowPassToggle.isOn;
        }

        // ########### Reverb

        public int ReverbPreset
        {
            set
            {
                Debug.Log("AudioDesigner: Reverb Preset State changed to: "+value);
                _reverbPreset = value;

                audioReverbFilter.reverbPreset = (AudioReverbPreset)reverbPresets[value];
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                reverbFilterDropdown.SetValueWithoutNotify(value);
            }
            get => _reverbPreset;
        }


        public void OnReverbPresetSelection(){
             if( BecomeOwner() ) ReverbPreset = reverbFilterDropdown.value;
        }

        public bool ReverbEnabled
        {
            set
            {
                Debug.Log("AudioDesigner: Reverb Enabled value changed to: "+value);
                _reverbEnabled = value;

                audioReverbFilter.enabled = value;
                reverbFilterDropdown.interactable = value;
                
                //we don't want to cause an event by changing the value, otherwise we will have an endless loop
                reverbFilterToggle.SetIsOnWithoutNotify(value);
            }
            get => _reverbEnabled;
        }

        public void OnReverbFilterToggle(){
            if( BecomeOwner() ) ReverbEnabled = reverbFilterToggle.isOn;
        }

        //misc
        public void OnDeleteButton(){
            Debug.Log("AudioDesigner: Delete button pressed");
            editableAudioSource.DisableMe();
        }

        public void SetAudioSourcePosition(Vector3 position){
            if(!Networking.IsOwner(soundSourceGrabable)){
                Networking.SetOwner(Networking.LocalPlayer, soundSourceGrabable);
                RequestSerialization();
            }
            soundSourceGrabable.transform.position = position;
        }

        public void SetAudioSourceRotation(Vector3 rotation){
            if(!Networking.IsOwner(soundSourceGrabable)){
                Networking.SetOwner(Networking.LocalPlayer, soundSourceGrabable);
                RequestSerialization();
            }
            soundSourceGrabable.transform.eulerAngles = rotation;
        }

        public void SetMenuPosition(Vector3 position){
            if(!Networking.IsOwner(menuHandle)){
                Networking.SetOwner(Networking.LocalPlayer, menuHandle);
                RequestSerialization();
            }
            menuHandle.transform.position = position;
        }

        public void SetMenuRotation(Vector3 rotation){
            if(!Networking.IsOwner(menuHandle)){
                Networking.SetOwner(Networking.LocalPlayer, menuHandle);
                RequestSerialization();
            }
            menuHandle.transform.eulerAngles = rotation;
        }

        
        //data import/export

        //toggle import/export menu
        public void OnImportExportButton(){
            importExportMenu.SetActive(!importExportMenu.activeSelf);
        }

        public void OnExportButton(){
            importExportLogField.text = "";
            DataDictionary config = ExportConfig();
            if( VRCJson.TrySerializeToJson(config, JsonExportType.Beautify, out DataToken json) ){
                importExportTextField.text = json.String;
            }else{
                importExportLogField.text = "ERROR: couldn't export config to JSON!";
            }
        }

        public void OnClearButton(){
            importExportLogField.text = "";
            importExportTextField.text = "";
        }

        public void OnImportButton(){
            importExportLogField.text = "";
            DataDictionary config = ParseConfig(importExportTextField.text);
            if( config != null ){
                if( ImportConfig(config,importIgnorePosRotToggle.isOn) ){
                    importExportLogField.text = "SUCCESS: The valid settings have been imported";
                }else{
                    importExportLogField.text = "ERROR: Couldn't find any valid settings to import!";
                }
            }else{
                importExportLogField.text = "ERROR: Not valid JSON!";
            }
        }

        public DataDictionary ParseConfig(string json){
            Debug.Log("Trying to parse the following json config:\n"+json);
            DataToken config;
            if( VRCJson.TryDeserializeFromJson(json, out config) ){
                if( config.TokenType == TokenType.DataDictionary ){
                    return config.DataDictionary;
                }else{
                    return null;
                }
            }else{
                return null;
            }
        }

        public bool ImportConfig(DataDictionary config, bool ignorePosition){
            //we are going to change the audio source settings => become owner so we can set them
            BecomeOwner();

            //in case this object hasn't fully initialized yet the settings in here might be overwritten by the init function
            //by setting the pendingConfig we tell the init code to load this instead
            pendingConfig = config;
            pendingIgnorePosRot = ignorePosition;

            //reset values that are not imported/exported
            playbackTimestamp = 0f;

            bool atLeastOneSuccessfull = false;
            DataToken tmp;
            
            //since the config DataDictionary was created by VRCJSON's TryDeserializeFromJson(),
            // all numbers in this DataDictionary will be of type Double and have to be converted to the right type

            if( ignorePosition == false ){      

                //import Position settings
                if (config.TryGetValue("Position", TokenType.DataDictionary, out tmp)){
                    Debug.Log("Position Dictionary setting found");
                    DataToken tmp2;
                    bool xIsValid=false, yIsValid=false, zIsValid=false;
                    float x=0f,y=0f,z=0f;
                    if (tmp.DataDictionary.TryGetValue("x", TokenType.Double, out tmp2)){
                        x = (float)tmp2.Double;
                        xIsValid = true;
                    }else{
                        Debug.Log("Error getting Position X setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("y", TokenType.Double, out tmp2)){
                        y = (float)tmp2.Double;
                        yIsValid = true;
                    }else{
                        Debug.Log("Error getting Position Y setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("z", TokenType.Double, out tmp2)){
                        z = (float)tmp2.Double;
                        zIsValid = true;
                    }else{
                        Debug.Log("Error getting Position Z setting: "+tmp2.ToString());
                    }
                    if( xIsValid && yIsValid && zIsValid ){
                        SetAudioSourcePosition(new Vector3(x,y,z));
                        Debug.Log("Valid Position setting found: ("+x.ToString()+","+y.ToString()+","+z.ToString()+")");
                    }else{
                        Debug.Log("Error: No fully valid Position setting found => ignoring it");
                    }
                }

                //import Rotation settings
                if (config.TryGetValue("Rotation", TokenType.DataDictionary, out tmp)){
                    Debug.Log("Rotation Dictionary setting found");
                    DataToken tmp2;
                    bool xIsValid=false, yIsValid=false, zIsValid=false;
                    float x=0f,y=0f,z=0f;
                    if (tmp.DataDictionary.TryGetValue("x", TokenType.Double, out tmp2)){
                        x = (float)tmp2.Double;
                        xIsValid = true;
                    }else{
                        Debug.Log("Error getting Rotation X setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("y", TokenType.Double, out tmp2)){
                        y = (float)tmp2.Double;
                        yIsValid = true;
                    }else{
                        Debug.Log("Error getting Rotation Y setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("z", TokenType.Double, out tmp2)){
                        z = (float)tmp2.Double;
                        zIsValid = true;
                    }else{
                        Debug.Log("Error getting Rotation Z setting: "+tmp2.ToString());
                    }
                    if( xIsValid && yIsValid && zIsValid ){
                        SetAudioSourceRotation(new Vector3(x,y,z));
                        Debug.Log("Valid Rotation setting found: ("+x.ToString()+","+y.ToString()+","+z.ToString()+")");
                    }else{
                        Debug.Log("Error: No fully valid Rotation setting found => ignoring it");
                    }
                }

                

                //import Menu Position settings
                if (config.TryGetValue("MenuPosition", TokenType.DataDictionary, out tmp)){
                    Debug.Log("MenuPosition Dictionary setting found");
                    DataToken tmp2;
                    bool xIsValid=false, yIsValid=false, zIsValid=false;
                    float x=0f,y=0f,z=0f;
                    if (tmp.DataDictionary.TryGetValue("x", TokenType.Double, out tmp2)){
                        x = (float)tmp2.Double;
                        xIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuPosition X setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("y", TokenType.Double, out tmp2)){
                        y = (float)tmp2.Double;
                        yIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuPosition Y setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("z", TokenType.Double, out tmp2)){
                        z = (float)tmp2.Double;
                        zIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuPosition Z setting: "+tmp2.ToString());
                    }
                    if( xIsValid && yIsValid && zIsValid ){
                        SetMenuPosition(new Vector3(x,y,z));
                        Debug.Log("Valid MenuPosition setting found: ("+x.ToString()+","+y.ToString()+","+z.ToString()+")");
                    }else{
                        Debug.Log("Error: No fully valid MenuPosition setting found => ignoring it");
                    }
                }

                //import Menu Rotation settings
                if (config.TryGetValue("MenuRotation", TokenType.DataDictionary, out tmp)){
                    Debug.Log("MenuRotation Dictionary setting found");
                    DataToken tmp2;
                    bool xIsValid=false, yIsValid=false, zIsValid=false;
                    float x=0f,y=0f,z=0f;
                    if (tmp.DataDictionary.TryGetValue("x", TokenType.Double, out tmp2)){
                        x = (float)tmp2.Double;
                        xIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuRotation X setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("y", TokenType.Double, out tmp2)){
                        y = (float)tmp2.Double;
                        yIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuRotation Y setting: "+tmp2.ToString());
                    }
                    if (tmp.DataDictionary.TryGetValue("z", TokenType.Double, out tmp2)){
                        z = (float)tmp2.Double;
                        zIsValid = true;
                    }else{
                        Debug.Log("Error getting MenuRotation Z setting: "+tmp2.ToString());
                    }
                    if( xIsValid && yIsValid && zIsValid ){
                        SetMenuRotation(new Vector3(x,y,z));
                        Debug.Log("Valid MenuRotation setting found: ("+x.ToString()+","+y.ToString()+","+z.ToString()+")");
                    }else{
                        Debug.Log("Error: No fully valid MenuRotation setting found => ignoring it");
                    }
                }
            }else{
                Debug.Log("Ignore position/rotation import setting is enabled => ignoring position and rotation");
            }

            //import AudioMode setting
            if (config.TryGetValue("AudioMode", TokenType.String, out tmp)){
                string audioModeName = tmp.String;
                Debug.Log("AudioMode setting found: "+audioModeName);
                bool found = false;
                for (int i = 0; i < AUDIO_MODE_EXPORT_NAME.Length; i++)
                {
                    if( AUDIO_MODE_EXPORT_NAME[i] == audioModeName ){
                        AudioMode = i;
                        found = true;
                        atLeastOneSuccessfull = true;
                        Debug.Log("Valid AudioMode setting found: "+audioModeName+" => ID: "+i.ToString());
                        break;
                    }
                }
                if(found == false){
                    Debug.Log("No valid AudioMode setting found for audio mode setting name: "+audioModeName);
                }
            }else{
                Debug.Log("Error getting AudioMode setting: "+tmp.ToString());
            }

            videoPlayerImportHelperPanel.SetActive(false);
            //import AudioCLip settings
            if (config.TryGetValue("AudioClipType", TokenType.String, out tmp)){
                string audioClipType = tmp.String;
                Debug.Log("AudioClipType setting found: "+audioClipType);
                if (config.TryGetValue("AudioClip", TokenType.String, out tmp)){
                    string audioClipValue = tmp.String;
                    if( audioClipType == AUDIO_CLIP_TYPE_URL ){
                        videoPlayerImportHelperPanel.SetActive(true);
                        videoPlayerImportHelperField.text = audioClipValue;
                        //set audio source to the player (last ID)
                        AudioClipID = 0;
                    }else if( audioClipType == AUDIO_CLIP_TYPE_LOCAL ){
                        bool found = false;
                        for (int i = 0; i < audioDesigner.audioClips.Length; i++)
                        {
                            if( audioDesigner.audioClips[i].name == audioClipValue ){
                                AudioClipID = i+1;
                                atLeastOneSuccessfull = true;
                                found = true;
                                Debug.Log("AudioClip set to: "+audioClipValue);
                                break;
                            }
                        }
                        if(found == false){
                            Debug.Log("No AudioClip with this name found in the local list of audio clips: "+audioClipValue);
                        }
                    }
                }else{
                    Debug.Log("AudioClipType is set, but no AudioClip setting => ignoring it: "+tmp.ToString());
                }
                
            }else{
                Debug.Log("Error getting AudioClipType setting (so also ignoring AudioClip setting): "+tmp.ToString());
            }
            
            //import Volume setting
            if (config.TryGetValue("Volume", TokenType.Double, out tmp)){
                float volume = (float)tmp.Double;
                Debug.Log("Volume setting found: "+volume.ToString());
                Volume = volume;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting Volume setting: "+tmp.ToString());
            }

            //import Gain setting
            if (config.TryGetValue("Gain", TokenType.Double, out tmp)){
                float gain = (float)tmp.Double;
                Debug.Log("Gain setting found: "+gain.ToString());
                Gain = gain;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting Gain setting: "+tmp.ToString());
            }

            //import VolumetricRadius setting
            if (config.TryGetValue("VolumetricRadius", TokenType.Double, out tmp)){
                float volumetricRadius = (float)tmp.Double;
                Debug.Log("VolumetricRadius setting found: "+volumetricRadius.ToString());
                VolumetricRadius = volumetricRadius;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting VolumetricRadius setting: "+tmp.ToString());
            }

            //import VolumetricRadiusVisualization setting
            if (config.TryGetValue("VolumetricRadiusVisualization", TokenType.Boolean, out tmp)){
                bool volumetricRadiusVisualization = tmp.Boolean;
                Debug.Log("VolumetricRadiusVisualization setting found: "+volumetricRadiusVisualization.ToString());
                VolumetricRadiusVisualization = volumetricRadiusVisualization;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting VolumetricRadiusVisualization setting: "+tmp.ToString());
            }

            //import VolumeFalloffMode setting
            if (config.TryGetValue("VolumeFalloffMode", TokenType.String, out tmp)){
                string volumeFalloffMode = tmp.String;
                Debug.Log("VolumeFalloffMode setting found: "+volumeFalloffMode);
                bool found = false;
                for (int i = 0; i < VOLUME_FALLOFF_MODE_EXPORT_NAME.Length; i++)
                {
                    if( VOLUME_FALLOFF_MODE_EXPORT_NAME[i] == volumeFalloffMode ){
                        VolumeFalloffMode = i;
                        found = true;
                        atLeastOneSuccessfull = true;
                        Debug.Log("Valid VolumeFalloffMode setting found: "+volumeFalloffMode+" => ID: "+i.ToString());
                        break;
                    }
                }
                if(found == false){
                    Debug.Log("No valid VolumeFalloffMode setting found for audio mode setting name: "+volumeFalloffMode);
                }
            }else{
                Debug.Log("Error getting VolumeFalloffMode setting: "+tmp.ToString());
            }

            //import Near setting
            if (config.TryGetValue("Near", TokenType.Double, out tmp)){
                float near = (float)tmp.Double;
                Debug.Log("Near setting found: "+near.ToString());
                Near = near;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting Near setting: "+tmp.ToString());
            }

            //import NearVisualization setting
            if (config.TryGetValue("NearVisualization", TokenType.Boolean, out tmp)){
                bool nearVisualization = tmp.Boolean;
                Debug.Log("NearVisualization setting found: "+nearVisualization.ToString());
                NearRadiusVisualization = nearVisualization;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting NearVisualization setting: "+tmp.ToString());
            }

            //import Far setting
            if (config.TryGetValue("Far", TokenType.Double, out tmp)){
                float far = (float)tmp.Double;
                Debug.Log("Far setting found: "+far.ToString());
                Far = far;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting Far setting: "+tmp.ToString());
            }

            //import FarVisualization setting
            if (config.TryGetValue("FarVisualization", TokenType.Boolean, out tmp)){
                bool farVisualization = tmp.Boolean;
                Debug.Log("FarVisualization setting found: "+farVisualization.ToString());
                FarRadiusVisualization = farVisualization;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting FarVisualization setting: "+tmp.ToString());
            }

            //import Pitch setting
            if (config.TryGetValue("Pitch", TokenType.Double, out tmp)){
                float pitch = (float)tmp.Double;
                Debug.Log("Pitch setting found: "+pitch.ToString());
                Pitch = pitch;
                atLeastOneSuccessfull = true;
            }else{
                Debug.Log("Error getting Pitch setting: "+tmp.ToString());
            }
            
            //import Highpass settings
            if (config.TryGetValue("Highpass", TokenType.DataDictionary, out tmp)){
                Debug.Log("Highpass Dictionary setting found");
                DataToken tmp2;
                if (tmp.DataDictionary.TryGetValue("enabled", TokenType.Boolean, out tmp2)){
                    Debug.Log("Highpass enabled setting found: "+tmp2.Boolean.ToString());
                    HighPassEnabled = tmp2.Boolean;
                    atLeastOneSuccessfull = true;
                }else{
                    Debug.Log("Error getting Highpass enabled setting: "+tmp2.ToString());
                }
                if (tmp.DataDictionary.TryGetValue("cutoffFreq", TokenType.Double, out tmp2)){
                    float cutoffFreq = (int)tmp2.Double;
                    Debug.Log("Highpass cutoffFreq setting found: "+cutoffFreq.ToString());
                    HighPassFreq = cutoffFreq;
                    atLeastOneSuccessfull = true;
                }else{
                    Debug.Log("Error getting Highpass cutoffFreq setting: "+tmp2.ToString());
                }
            }

            //import Lowpass settings
            if (config.TryGetValue("Lowpass", TokenType.DataDictionary, out tmp)){
                Debug.Log("Lowpass Dictionary setting found");
                DataToken tmp2;
                if (tmp.DataDictionary.TryGetValue("enabled", TokenType.Boolean, out tmp2)){
                    Debug.Log("Lowpass enabled setting found: "+tmp2.Boolean.ToString());
                    LowPassEnabled = tmp2.Boolean;
                    atLeastOneSuccessfull = true;
                }else{
                    Debug.Log("Error getting Lowpass enabled setting: "+tmp2.ToString());
                }
                if (tmp.DataDictionary.TryGetValue("cutoffFreq", TokenType.Double, out tmp2)){
                    float cutoffFreq = (int)tmp2.Double;
                    Debug.Log("Lowpass cutoffFreq setting found: "+cutoffFreq.ToString());
                    LowPassFreq = cutoffFreq;
                    atLeastOneSuccessfull = true;
                }else{
                    Debug.Log("Error getting Lowpass cutoffFreq setting: "+tmp2.ToString());
                }
            }

            //import Reverb settings
            if (config.TryGetValue("Reverb", TokenType.DataDictionary, out tmp)){
                Debug.Log("Reverb Dictionary setting found");
                DataToken tmp2;
                if (tmp.DataDictionary.TryGetValue("enabled", TokenType.Boolean, out tmp2)){
                    Debug.Log("Reverb enabled setting found: "+tmp2.Boolean.ToString());
                    ReverbEnabled = tmp2.Boolean;
                    atLeastOneSuccessfull = true;
                }else{
                    Debug.Log("Error getting Reverb enabled setting: "+tmp2.ToString());
                }
                if (tmp.DataDictionary.TryGetValue("FilterPreset", TokenType.String, out tmp2)){
                    string filterPresetName = tmp2.String;
                    Debug.Log("Reverb FilterPreset setting found: "+filterPresetName);
                    
                    bool found = false;
                    for (int i = 0; i < REVERB_PRESET_EXPORTNAME.Length; i++)
                    {
                        if( REVERB_PRESET_EXPORTNAME[i] == filterPresetName ){
                            ReverbPreset = i;
                            found = true;
                            atLeastOneSuccessfull = true;
                            Debug.Log("Valid Reverb FilterPreset setting found: "+filterPresetName+" => ID: "+i.ToString());
                            break;
                        }
                    }
                    if(found == false){
                        Debug.Log("No valid reverb preset setting found for reverb preset setting name: "+filterPresetName);
                    }
                }else{
                    Debug.Log("Error getting Reverb FilterPreset setting: "+tmp2.ToString());
                }
            }

             //import IsPlaying setting
            if (config.TryGetValue("IsPlaying", TokenType.Boolean, out tmp)){
                bool isPlaying = tmp.Boolean;
                Debug.Log("IsPlaying setting found: "+isPlaying.ToString());
                if( AudioClipID != 0 ){
                    //only start playing audio source if it's not set to the video player, because we can't initialize it with the URL at runtime in udon anyway
                    IsPlaying = isPlaying;
                    atLeastOneSuccessfull = true;
                }
            }else{
                Debug.Log("Error getting IsPlaying setting: "+tmp.ToString());
            }

            return atLeastOneSuccessfull;
        }

        public DataDictionary ExportConfig(){

            DataDictionary positionSettings = (DataDictionary)audioSourceConfig["Position"];
            positionSettings["x"] = soundSourceGrabable.transform.position.x;
            positionSettings["y"] = soundSourceGrabable.transform.position.y;
            positionSettings["z"] = soundSourceGrabable.transform.position.z;

            DataDictionary rotationSettings = (DataDictionary)audioSourceConfig["Rotation"];
            rotationSettings["x"] = soundSourceGrabable.transform.eulerAngles.x;
            rotationSettings["y"] = soundSourceGrabable.transform.eulerAngles.y;
            rotationSettings["z"] = soundSourceGrabable.transform.eulerAngles.z;

            DataDictionary menuPositionSettings = (DataDictionary)audioSourceConfig["MenuPosition"];
            menuPositionSettings["x"] = menuHandle.transform.position.x;
            menuPositionSettings["y"] = menuHandle.transform.position.y;
            menuPositionSettings["z"] = menuHandle.transform.position.z;

            DataDictionary menuRotationSettings = (DataDictionary)audioSourceConfig["MenuRotation"];
            menuRotationSettings["x"] = menuHandle.transform.eulerAngles.x;
            menuRotationSettings["y"] = menuHandle.transform.eulerAngles.y;
            menuRotationSettings["z"] = menuHandle.transform.eulerAngles.z;

            audioSourceConfig["IsPlaying"] = IsPlaying;
            
            audioSourceConfig["AudioMode"] = AUDIO_MODE_EXPORT_NAME[AudioMode];
            //check if clip ID is out of range of the audioCLips array => URL/Video Player mode
            if( AudioClipID == 0 ){
                //video player mode => set export type to url and export url
                audioSourceConfig["AudioClipType"] = AUDIO_CLIP_TYPE_URL;
                audioSourceConfig["AudioClip"] = videoPlayerAPI.GetCurrentURL().Get();
            }else{
                //local audio file is currently selected => set export type to local and export the currently selected audio clip's name
                audioSourceConfig["AudioClipType"] = AUDIO_CLIP_TYPE_LOCAL;
                audioSourceConfig["AudioClip"] =  audioDesigner.audioClips[AudioClipID-1].name;
            }  
            
            audioSourceConfig["Volume"] = Volume;
            audioSourceConfig["Gain"] = Gain;
            audioSourceConfig["VolumetricRadius"] = VolumetricRadius;
            audioSourceConfig["VolumetricRadiusVisualization"] = VolumetricRadiusVisualization;
            audioSourceConfig["VolumeFalloffMode"] = VOLUME_FALLOFF_MODE_EXPORT_NAME[VolumeFalloffMode];
            audioSourceConfig["Near"] = Near;
            audioSourceConfig["NearVisualization"] = NearRadiusVisualization;
            audioSourceConfig["Far"] = Far;
            audioSourceConfig["FarVisualization"] = FarRadiusVisualization;
            audioSourceConfig["Pitch"] = Pitch;

            DataDictionary highpassSettings = (DataDictionary)audioSourceConfig["Highpass"];
            highpassSettings["enabled"] = HighPassEnabled;
            highpassSettings["cutoffFreq"] = HighPassFreq;

            DataDictionary lowpassSettings = (DataDictionary)audioSourceConfig["Lowpass"];
            lowpassSettings["enabled"] = LowPassEnabled;
            lowpassSettings["cutoffFreq"] = LowPassFreq;

            DataDictionary reverbSettings = (DataDictionary)audioSourceConfig["Reverb"];
            reverbSettings["enabled"] = ReverbEnabled;
            reverbSettings["FilterPreset"] = REVERB_PRESET_EXPORTNAME[ReverbPreset];

            return audioSourceConfig;
        }


        //########## utility functions
        private float RescaleLogarithmic(float value, float min, float max){
            return Mathf.Exp(Mathf.Lerp(Mathf.Log(min), Mathf.Log(max), value));
        }

        private float InverseRescaleLogarithmic(float value, float min, float max){
            return Mathf.InverseLerp(Mathf.Log(min), Mathf.Log(max), Mathf.Log(value));
        }

        private void SetGlobalScale(GameObject item, float globalScale){
            item.transform.localScale = Vector3.one;
            float scale = globalScale;
            item.transform.localScale = new Vector3 (scale,scale,scale);
            //Debug.Log("SetGlobalScale: " + item.name + ", lossyScale: " + transform.lossyScale.x.ToString() + "," + transform.lossyScale.y.ToString() + "," + transform.lossyScale.z.ToString() + "|=> scale: " + scale.ToString());
        }

    }
}
