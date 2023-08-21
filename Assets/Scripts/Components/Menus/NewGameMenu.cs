using Assets.Scripts.Components.Menus;
using Assets.Scripts.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Assets.Scripts.Components.Menus
{
    public class NewGameMenu : ExitableMenu
    {

        public bool multiplayer = false;

        //NOTE: TMP_InputField, not InputField
        TMP_InputField nameInput, worldSizeInput;
        TMP_Dropdown worldSizeDropdown;

        Dictionary<string, int> worldSizePresets = new()
        {
            { "Small", 100 },
            { "Medium", 200 },
            { "Large", 300 },
            { "Custom", 200 }
        };

        int worldSize;

        // Start is called before the first frame update
        void Start()
        {
            //Get inputs
            IEnumerable<TMP_InputField> inputs = GetComponentsInChildren<TMP_InputField>();
            nameInput = inputs.Where(i => i.contentType == TMP_InputField.ContentType.Alphanumeric).First();
            worldSizeInput = inputs.Where(i => i.contentType == TMP_InputField.ContentType.IntegerNumber).First();
            
            //Get dropdowns
            IEnumerable<TMP_Dropdown> dropdowns = GetComponentsInChildren<TMP_Dropdown>();
            worldSizeDropdown = dropdowns.First();

            //Set up world size dropdown
            worldSizeDropdown.ClearOptions();
            worldSizeDropdown.AddOptions(new List<string>(worldSizePresets.Keys));
            worldSizeDropdown.SetValueWithoutNotify(1);
            worldSizeDropdown.RefreshShownValue();
            worldSizeDropdown.onValueChanged.AddListener(OnWorldSizeDropdownChanged);

            //Set up world size input
            worldSizeInput.onValueChanged.AddListener(OnWorldSizeInputChanged);

            //Init world size
            worldSize = worldSizePresets.Values.ElementAt(worldSizeDropdown.value);
            worldSizeInput.SetTextWithoutNotify(worldSize.ToString());
        }

        void OnWorldSizeDropdownChanged(int value)
        {
            worldSize = worldSizePresets.Values.ElementAt(value);
            worldSizeInput.SetTextWithoutNotify(worldSize.ToString());
        }

        void OnWorldSizeInputChanged(string value)
        {
            worldSize = int.Parse(value);

            //If the value is not a preset, set the dropdown to custom
            if (!worldSizePresets.ContainsValue(worldSize))
            {
                worldSizeDropdown.SetValueWithoutNotify(worldSizePresets.Count - 1);
                worldSizeDropdown.RefreshShownValue();
            }
        }

    }
}