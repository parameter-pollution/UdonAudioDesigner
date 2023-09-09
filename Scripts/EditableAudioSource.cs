
using AudioDesigner;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public class EditableAudioSource : UdonSharpBehaviour
{
    public GameObject soundSource;
    public GameObject menu;
    public ObjectMenu objectMenu;
    public AudioSources audioSources;
    [HideInInspector]
    public bool isVisible = false;
    private int id = -1;

    void Start()
    {
    }

    public void Show(){
        //detach sound source object from main menu by moving it to the root of the hirarchy
        transform.SetParent(null);
        //show the sound source 3d object and menu
        soundSource.SetActive(true);
        menu.SetActive(true);
        isVisible = true;
    }

    public void Hide(){
        //hide the sound source 3d object and menu
        objectMenu.Cleanup();
        soundSource.SetActive(false);
        menu.SetActive(false);
        isVisible = false;
    }

    public void SetID(int identifier){
        Debug.Log("AudioDesigner: EditableAudioSource ID set to: "+identifier);
        id = identifier;
    }

    public DataDictionary ExportConfig(){
        return objectMenu.ExportConfig();
    }

    public bool ImportConfig(DataDictionary config, bool ignorePosition){
        return objectMenu.ImportConfig(config,ignorePosition);
    }

    public void ResetSettings(){
        objectMenu.LoadDefaults();
    }

    public void DisableMe(){
        audioSources.DisableAudioSource(id);
    }

    public void SetAudioSourcePosition(Vector3 position){
        objectMenu.SetAudioSourcePosition(position);
    }

    public void SetAudioSourceRotation(Vector3 rotation){
        objectMenu.SetAudioSourceRotation(rotation);
    }

    public void SetMenuPosition(Vector3 position){
        objectMenu.SetMenuPosition(position);
    }

    public void SetMenuRotation(Vector3 rotation){
        objectMenu.SetMenuRotation(rotation);
    }

}
