
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections.Generic;
#endif

namespace AudioDesigner
{
    public class AudioDesigner : UdonSharpBehaviour
    {
        public AudioClip[] audioClips;
        public VRCUrl presetsDownloadURL;
        

        void Start()
        {
        }

    //we need to populate the audio source menu audio clip dropdowns in-editor because we can't use lists in udonsharp yet to do it at runtime
    #if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void OnValidate(){
            Debug.Log("OnValidate was called");

            List<Dropdown.OptionData> dropdownList = new List<Dropdown.OptionData>();

            //first entry is always URL/videoplayer
            Dropdown.OptionData videoPlayerOption = new Dropdown.OptionData();
            videoPlayerOption.text = "URL (VideoPlayer)";
            dropdownList.Add(videoPlayerOption);

            //then add all the soundclips that have been added to the list
            foreach (AudioClip audioClip in audioClips)
            {
                Dropdown.OptionData dropdownOption = new Dropdown.OptionData();
                dropdownOption.text = audioClip.name;
                dropdownList.Add(dropdownOption);
            }

            Component[] audioSourceMenus = GetComponentsInChildren<ObjectMenu>(true);

            foreach (ObjectMenu menu in audioSourceMenus){
                menu.InitializeAudioClipDropdown(dropdownList);
            }
        }
    #endif

    }
}
