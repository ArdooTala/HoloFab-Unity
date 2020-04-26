#define DEBUG
// #define DEBUG2
// #undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	public class InteractionManager : Type_Manager<InteractionManager>
									#if WINDOWS_UWP
		                            // , InputSystemGlobalHandlerListener
		                            , IMixedRealityPointerHandler
		                            // , IMixedRealityInputHandler
		                            // , IMixedRealityInputHandler<Vector2>
		                            // , IMixedRealityInputHandler<Vector3>
									#endif
	{
		private Interactible_Placeable[] placeables;
		private Interactible_Movable[] movables;
		[HideInInspector]
		public Interactible_Placeable activePlaceable;
		[HideInInspector]
		public Interactible_Movable activeMovable;
        
		// Select
		[HideInInspector]
		public RaycastHit hit;
		[HideInInspector]
		public bool flagHit = false;
        
		// Click
		private bool flagClick = false;
        
		// Drag
		private Vector3 startDragPosition, currentDragPosition;
		// Rotate
		private Vector3 startRelativeDrag;
		private float startAngle;
        
		#if WINDOWS_UWP
		////////////////////////////////////////////////////////////////////////
		protected virtual void OnEnable(){
			RegisterHandlers();
		}
        
		protected virtual void OnDisable(){
			UnregisterHandlers();
		}
		private void RegisterHandlers() {
			// CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
			CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
		}
        
		private void UnregisterHandlers() {
			// CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
			CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
		}
        
		// public void OnPositionInputChanged(InputEventData<Vector2> eventData) {
		// 	Debug.Log("Interaction Manager: Input Position Changed vec 3 " + eventData.InputData);
		// }
		// public void OnPositionInputChanged(InputEventData<Vector3> eventData) {
		// 	Debug.Log("Interaction Manager: Input Position Changed Vec 2 " + eventData.InputData);
		// }
		//
		// public void OnInputChanged(InputEventData<Vector3> eventData) {
		// 	Debug.Log("Interaction Manager: Input Changed vec 3 " + eventData.InputData);
		// }
		//
		// public void OnInputChanged(InputEventData<Vector2> eventData) {
		// 	Debug.Log("Interaction Manager: Input Changed Vec 2 " + eventData.InputData);
		// }
		////////////////////////////////////////////////////////////////////////
		// public void OnInputDown(InputEventData eventData) {
		// 	Debug.Log("Interaction Manager: Input Down");
		// }
		// public void OnInputUp(InputEventData eventData) {
		// 	Debug.Log("Interaction Manager: Input Up");
		// }
		////////////////////////////////////////////////////////////////////////
		public void OnPointerClicked(MixedRealityPointerEventData eventData) {
			Debug.Log("Interaction Manager: OnPointer Clicked");
			// eventData.Use();
		}
		public void OnPointerDown(MixedRealityPointerEventData eventData) {
			this.currentDragPosition = eventData.Pointer.Position;
			Quaternion rotation = eventData.Pointer.Rotation;
            
			Debug.Log("Interaction Manager: OnPointer Down: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
			// eventData.Use();
            
			this.flagClick = true;
			ExtractClickInfo();
		}
		public void OnPointerDragged(MixedRealityPointerEventData eventData) {
			this.currentDragPosition = eventData.Pointer.Position;
			Quaternion rotation = eventData.Pointer.Rotation;
            
			Debug.Log("Interaction Manager: OnPointer Drag: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
			// eventData.Use();
		}
        
		public void OnPointerUp(MixedRealityPointerEventData eventData) {
			this.currentDragPosition = eventData.Pointer.Position;
			Quaternion rotation = eventData.Pointer.Rotation;
            
			Debug.Log("Interaction Manager: OnPointer Up: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
			// eventData.Use();
            
			// Reset Dargging
			StopMoving();
		}
		#endif
		////////////////////////////////////////////////////////////////////////
		void Update(){
			// #if WINDOWS_UWP && DEBUG2
			// MixedRealityInputAction[] actions = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions;
			// if (actions.Length > 0)
			// 	Debug.Log("Interactible Manager: Mixed Reality events found: " + actions.Length);
			// #endif
            
			CheckSelection();
			CheckClick();
			CheckDrag();
            
			#if DEBUG2
			if (this.activeMovable != null)
				Debug.Log("Interaction Manager: Active Movable: " + this.activeMovable.gameObject.name);
			if (this.activePlaceable != null)
				Debug.Log("Interaction Manager: Active Placeable: " + this.activePlaceable.gameObject.name);
			#endif
            
			this.flagClick = false; // Force unclick
		}
		////////////////////////////////////////////////////////////////////////
		private void CheckSelection(){
			// Send a ray and find if anything is being selected.
			Ray ray = UnityUtilities.GenerateSelectionRay();
			if (Physics.Raycast(ray.origin, ray.direction, out this.hit)) {
				#if DEBUG2
				Debug.Log("Interaction Manager: Gaze hit gameObject: " + this.hit.transform.gameObject.name);
				#endif
				this.flagHit = true;
			} else
				this.flagHit = false; // If nothing hit - reset history.
		}
		// A function to register clicks (cross platform).
		private void CheckClick(){
			#if !WINDOWS_UWP
			if (Input.GetMouseButtonDown(0)) {
				this.flagClick = true;
				ExtractClickInfo();
			}
			#endif
			// In UWP handled by pointer events
		}
		// Extract information about cursor hit info.
		private void ExtractClickInfo(){
			#if DEBUG
			Debug.Log("Interaction Manager: Click: " + this.hit.transform.gameObject.name);
			#endif
			// If clicked on scanned mesh - stop placment
			if ((this.activePlaceable != null) && (ObjectManager.instance.CheckEnvironmentObject(this.hit.transform.gameObject))) {
				#if DEBUG
				Debug.Log("Interaction Manager: Click On Scan Mesh");
				#endif
				TryStopPlacing();
			} else {
				// Find interactibles if any.
				this.activeMovable = CheckMovableHit(this.hit.transform.gameObject);
				this.activePlaceable = CheckPlaceableHit(this.hit.transform.gameObject);
			}
		}
		private void CheckDrag(){
			#if !WINDOWS_UWP
			// Don't bother checking dragging if drag object wasn't found.
			if (this.activeMovable != null) {
				// Dragging taken care in Movable.
				// Only monitor stopping dragging.
				if (Input.GetMouseButtonUp(0)) {
					// Reset
					StopMoving();
				}
			}
			#endif
		}
		////////////////////////////////////////////////////////////////////////
		private Interactible_Placeable CheckPlaceableHit(GameObject goHit) {
			this.placeables = FindObjectsOfType<Interactible_Placeable>();
			foreach (Interactible_Placeable placeable in this.placeables)
				if (placeable.CheckTrigger(goHit)) {
					return placeable;
				}
			return null;
		}
		private void TryStopPlacing(){
			if (this.activePlaceable != null) {
				if (this.activePlaceable.OnTrySnap())
					this.activePlaceable = null;
			}
			// this.placeables = FindObjectsOfType<Interactible_Placeable>();
			// foreach (Interactible_Placeable placeable in this.placeables)
			// 	placeable.OnTrySnap();
		}
		////////////////////////////////////////////////////////////////////////
		// Find Movable object that is hit (if any).
		private Interactible_Movable CheckMovableHit(GameObject goHit) {
			this.movables = FindObjectsOfType<Interactible_Movable>();
			foreach (Interactible_Movable movable in this.movables)
				if (movable.CheckTrigger(goHit))
					return movable;
			return null;
		}
		// Deactivate Movement on Active Mover.
		private void StopMoving(){
			if (this.activeMovable != null) {
				this.activeMovable.StopMoving();
				this.activeMovable = null;
			}
			// Deactivate all movables - overkill, since we already have active one.
			// this.movables = FindObjectsOfType<Interactible_Movable>();
			// foreach (Interactible_Movable movable in this.movables)
			// 	movable.StopMoving();
		}
		////////////////////////////////////////////////////////////////////////
		public Vector3 DragMoveDifference(bool flagDragStart){
			#if !WINDOWS_UWP
			this.currentDragPosition = CurrentProjectedPlanePoint(out bool _flagHit);
			#endif
            
			if (flagDragStart)
				this.startDragPosition = this.currentDragPosition;
			return this.currentDragPosition - this.startDragPosition;
		}
		public float DragRotateDifference(bool flagDragStart){
			Vector3 relativeDrag;
			#if !WINDOWS_UWP
			this.currentDragPosition = CurrentProjectedPlanePoint(out bool _flagHit);
			// #else
			// relativeDrag = this.currentDragPosition - this.startDragPosition;
			#endif
			relativeDrag = this.currentDragPosition - this.activeMovable.transform.position;
            
			if (flagDragStart)
				this.startRelativeDrag = relativeDrag;
            
			// a trick to check direction of rotation?
			// TODO: Should be done once?
			Vector3 controlVector = Quaternion.AngleAxis(1, this.activeMovable.orientationPlane.normal) * this.startRelativeDrag;
			float currentAngle = Vector3.Angle(this.startRelativeDrag, relativeDrag);
			float controlAngle = Vector3.Angle(controlVector, relativeDrag);
            
			if (flagDragStart)
				this.startAngle = currentAngle;
			float angleDifference = currentAngle - this.startAngle;
            
			if (controlAngle > angleDifference) angleDifference *= -1;
            
			return angleDifference;
		}
		private Vector3 CurrentProjectedPlanePoint(out bool _flagHit){
			Ray cameraMouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			// NB! Isn't it the same as mouse Ray
			if (this.activeMovable.orientationPlane.Raycast(cameraMouseRay, out float rayDistance)) {
				_flagHit = true;
				return cameraMouseRay.GetPoint(rayDistance);
			}
			_flagHit = false;
			return Vector3.zero;
		}
	}
}