
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

namespace AudioDesigner
{
    public class MainMenu : UdonSharpBehaviour
    {
        public AudioDesigner audioDesigner;
        public AudioSources audioSourcesManager;
        public GameObject bulkImportExportMenu;
        public InputField importExportTextField;
        public Text importExportLogField;
        public GameObject PresetUI;
        public GameObject[] presetButtons;
        private IUdonEventReceiver EventReceiver;
        private DataList presets;
        void Start()
        {
            EventReceiver = (IUdonEventReceiver)this;
            VRCStringDownloader.LoadUrl(audioDesigner.presetsDownloadURL, EventReceiver);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result){
            presets = ParsePresets(result.Result);
            int nextButtonID = 0;
            if( presets != null ){
                for (int i = 0; i < presets.Count; i++)
                {
                    if( nextButtonID < presetButtons.Length ){
                        DataDictionary presetConfig = (DataDictionary)presets[i];
                        if( presetConfig.TryGetValue("Name", TokenType.String, out DataToken tmp) ){
                            string name = tmp.String;
                            if( presetConfig.TryGetValue("BulkConfig", TokenType.DataList, out DataToken tmp2) ){
                                //show another button
                                presetButtons[nextButtonID].GetComponentInChildren<Text>().text = name;
                                presetButtons[nextButtonID].SetActive(true);
                                PresetUI.SetActive(true);
                                nextButtonID++;
                            }
                        }
                    }else{
                        Debug.Log("AudioDesigner MainMenu: More presets in config than buttons => ignoring the rest!");
                    }
                }

            }else{
                Debug.Log("AudioDesigner MainMenu: Error parsing downloaded presets!");
            }
        }

        public override void OnStringLoadError(IVRCStringDownload result){
            Debug.Log("AudioDesigner MainMenu: Error downloading string: " + result.ErrorCode + " - " + result.Error);
        }

        public DataList ParsePresets(string json){
            //Debug.Log("AudioDesigner MainMenu: Trying to parse the following presets json config:\n"+json);
            DataToken presets;
            if( VRCJson.TryDeserializeFromJson(json, out presets) ){
                if( presets.TokenType == TokenType.DataList ){
                    return presets.DataList;
                }else if( presets.TokenType == TokenType.DataDictionary ){
                    //if a single audio source presets was pasted into the bulk import then just return a list with a single entry
                    DataList list = new DataList();
                    list.Add(presets.DataDictionary);
                    return list;
                }else{
                    return null;
                }
            }else{
                return null;
            }
        }

        //this can probably be done better ;-)
        public void OnPresetButton1(){ LoadPreset(0); }
        public void OnPresetButton2(){ LoadPreset(1); }
        public void OnPresetButton3(){ LoadPreset(2); }
        public void OnPresetButton4(){ LoadPreset(3); }
        public void OnPresetButton5(){ LoadPreset(4); }
        public void OnPresetButton6(){ LoadPreset(5); }

        public void LoadPreset(int index){
            if( presets[index].TokenType == TokenType.DataDictionary ){
                if( ((DataDictionary)presets[index]).TryGetValue("BulkConfig", TokenType.DataList, out DataToken tmp) ){
                    DataList presetBulkConfig = tmp.DataList;
                    ImportBulkConfig(presetBulkConfig);
                }
            }
        }

        public void OnAddAudioSource(){
            audioSourcesManager.SpawnAudioSource();
        }

        public void OnClearButton(){
            importExportLogField.text = "";
            importExportTextField.text = "";
        }

        
        public void OnBulkImportExportButton(){
            //toggle import/export panel
            bulkImportExportMenu.SetActive(!bulkImportExportMenu.activeSelf);
        }

        public void OnExportButton(){
            importExportLogField.text = "";
            DataList config = ExportBulkConfig();
            if( VRCJson.TrySerializeToJson(config, JsonExportType.Beautify, out DataToken json) ){
                importExportTextField.text = json.String;
            }else{
                importExportLogField.text = "ERROR: couldn't export config to JSON!";
            }
        }

        public void OnImportButton(){
            importExportLogField.text = "";
            DataList configList = ParseConfig(importExportTextField.text);
            if( configList != null ){
                if( ImportBulkConfig(configList) ){
                    importExportLogField.text = "SUCCESS: The valid settings have been imported";
                }else{
                    importExportLogField.text = "ERROR: Couldn't find any valid settings to import!";
                }
            }else{
                importExportLogField.text = "ERROR: Not valid JSON!";
            }
        }

        private DataList ExportBulkConfig(){
            DataList configList = new DataList();
            for (int i = 0; i < audioSourcesManager.editableAudioSources.Length; i++)
            {
                if( audioSourcesManager.editableAudioSources[i] != null ){
                    EditableAudioSource source = audioSourcesManager.editableAudioSources[i].GetComponent<EditableAudioSource>();
                    if( source != null ){
                        if( source.isVisible ){
                            configList.Add(source.ExportConfig());
                        }
                    }
                }
            }
            return configList;
        }

        public DataList ParseConfig(string json){
            Debug.Log("Main Menu: Trying to parse the following json config:\n"+json);
            DataToken config;
            if( VRCJson.TryDeserializeFromJson(json, out config) ){
                if( config.TokenType == TokenType.DataList ){
                    return config.DataList;
                }else if( config.TokenType == TokenType.DataDictionary ){
                    //if a single audio source config was pasted into the bulk import then just return a list with a single entry
                    DataList list = new DataList();
                    list.Add(config.DataDictionary);
                    return list;
                }else{
                    return null;
                }
            }else{
                return null;
            }
        }

        private bool ImportBulkConfig(DataList configList){
            bool atLeastOneSuccessfull = false;
            audioSourcesManager.DisableAllAudioSources();
            for (int i = 0; i < configList.Count && i < audioSourcesManager.editableAudioSources.Length; i++)
            {
                int nextNewAudioSourceID = audioSourcesManager.SpawnAudioSource();
                
                EditableAudioSource source = audioSourcesManager.editableAudioSources[nextNewAudioSourceID].GetComponent<EditableAudioSource>();
                
                if( source.ImportConfig((DataDictionary)configList[i],false) ){
                    atLeastOneSuccessfull = true;
                }else{
                    audioSourcesManager.DisableAudioSource(nextNewAudioSourceID);
                }
            }
            return atLeastOneSuccessfull;
        }
        
    }
}
