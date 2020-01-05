﻿//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
    // Boolean Toggle UI element, responsible for keeping track of the value.
    public class BooleanToggle : MonoBehaviour {
        [Tooltip("UI element.")]
        public Toggle toggle;
        // Value of the toggle.
        [HideInInspector]
        public bool value=false;

        void Start() {
            // Subscribe Button Event.
            toggle.onValueChanged.AddListener((value) => ToggleValueOnChange(value));
        }
        // Update value On UI click.
        private void ToggleValueOnChange(bool boolean) {
            this.value = boolean;
            #if DEBUG
            Debug.Log("Boolean Toggle: Value: " + this.value);
            #endif
            // Inform UI Manager.
            UIManager.instance.OnValueChanged();
        }
    }
}