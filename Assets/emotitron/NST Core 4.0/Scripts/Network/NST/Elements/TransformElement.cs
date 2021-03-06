﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.SmartVars;
using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	public enum ElementType { Position, Rotation }

	[System.Serializable]
	public abstract class TransformElement
	{
		[Tooltip("Elements need a name in order to be called in code by name. You can identify elements using the NSTTransformElement.elementIDLookup dictionary, but it is easier to use the NSTTransformElement.elementLookup using the name, and caching the element returned.")]
		public string name;
		public bool isRoot;

		public int index;
		public float drawerHeight;
		public NetworkSyncTransform nst;
		public NSTElementsEngine nstElementsEngine;
		public ElementType elementType;

		#region Inspector Values

		[Tooltip("Which events will cause full updates to be sent on the next tick.")]
		public SendCullMask sendCullMask = SendCullMask.OnChanges | SendCullMask.OnTeleport | SendCullMask.OnCustomMsg | SendCullMask.OnRewindCast;

		[Tooltip("The spacing of keyframes.")]
		[Range(0, 32)]
		public int keyRate = 5;

		[Tooltip("Assign this if you want this to element sync to apply to a different gameobject than the one this component is attached to. Leave this empty to default to this child gameobject.")]
		public GameObject gameobject;

		[Tooltip("0 = No extrapolation. 1 = Full extrapolation. Extrapolation occurs when the buffer runs out of frames. Without extrapolation the object will freeze if no new position updates have arrived in time. With extrapolation the object will continue in the direction it was heading as of the last update until a new update arrives.")]
		[Range(0, 1)]
		public float extrapolation = .5f;


		[Tooltip("The max number of sequential frames that will be extrapolated. Too large a value and objects will wander too far during network hangs, too few and network objects will freeze when the buffer empties. Extrapolation should not be occurring often - if at all, so a smaller number is ideal (default = 1 frame).")]
		[Range(0, 16)]
		public int maxExtrapolates = 2;

		[Tooltip("Teleport command from server will force the server state of this element on the owner.")]
		public bool teleportOverride = true;

		#endregion

		public GameObject rewindGO;
		[HideInInspector] public float lastSentKeyTime;
		[HideInInspector] public Frame snapshot;
		[HideInInspector] public Frame target;
		[HideInInspector] public CompressedElement lastTeleportValue = CompressedElement.zero;

		public CompressedElement lastSentCompressed;
		public GenericX lastSentTransform;

		protected bool hasReceivedInitial;

		public abstract GenericX Localized { get; set; }

		public virtual void Initialize(NetworkSyncTransform _nst)
		{
			nst = _nst;
			nstElementsEngine = nst.nstElementsEngine;
		}

		public void Snapshot(Frame newTargetFrame, bool lateUpdate = false, bool midTeleport = false)
		{
			bool isNull = newTargetFrame.elements[index].transform.type == XType.NULL;

			// If the element carried no information for this frame, use the last updates value.
			if (isNull)
			{
				bool oldTargetIsNull = target == null || target.elements[index].transform.type == XType.NULL;

				newTargetFrame.elements[index].transform = oldTargetIsNull ? Localized : target.elements[index].transform;
				newTargetFrame.elements[index].compTrans = oldTargetIsNull ? Compress(newTargetFrame.elements[index].transform) : target.elements[index].compTrans;
			}

			// First run set both target and snapshot to the incoming.
			if (hasReceivedInitial == false)
			{
				target = newTargetFrame;
				hasReceivedInitial = true;
			}
			snapshot = target; // LocalizedRot;
			target = newTargetFrame;
		}

		public abstract void Apply(GenericX posOrRot, GameObject targetGO);
		public abstract void Apply(GenericX posOrRot);

		public abstract CompressedElement Compress();
		public abstract CompressedElement Compress(GenericX uncompressed);
		public abstract GenericX Decompress(CompressedElement comp);

		// TODO Skip the inbetween step here
		public abstract bool Write(ref UdpBitStream bitstream, Frame frame);
		public abstract bool Read(ref UdpBitStream bitstream, Frame frame, Frame currentFrame);
		public abstract void MirrorToClients(ref UdpBitStream outstream, Frame frame, bool hasChanged);

		/// <summary>
		/// Write the flag bool if this is not a forced update, and return true if the element should be written to the stream (the value of the flag).
		/// </summary>
		protected bool WriteUpdateFlag(ref UdpBitStream outstream, Frame frame, bool hasChanged)
		{
			bool forcedUpdate = IsUpdateForced(frame);

			// For non-forced updates we need to set the used flag.
			if (!forcedUpdate)
				outstream.WriteBool(hasChanged);

			// exit if we are not writing a compressed value.
			if (!hasChanged && !forcedUpdate)
				return false;

			return true;
		}

		/// <summary>
		/// This is the logic for when a frame must be sent using info available to all clients/server, so in these cases elements do not need to send a "used" bit
		/// ahead of each element, since an update is required.
		/// </summary>
		protected bool IsUpdateForced(Frame frame) // UpdateType updateType, int frameid)
		{
			UpdateType updateType = frame.updateType;
			int frameid = frame.frameid;
			bool hasAuthority = nst.na.HasAuthority;

			return
				sendCullMask.EveryTick() ||
				(sendCullMask.OnCustomMsg() && updateType.IsCustom()) ||
				(sendCullMask.OnTeleport() && updateType.IsTeleport()) ||
				(sendCullMask.OnRewindCast() && updateType.IsRewindCast()) ||
				(updateType.IsTeleport() && teleportOverride) || // teleport flagged updates must send all teleport elements
				(frameid != 0 && keyRate != 0 && frameid % keyRate == 0); // keyrate mod, but not for frame 0, since that is a special case.
		}

		protected bool ShouldApplyToGhost(Frame frame)
		{
			return (frame.nst.na.IsServer && frame.updateType.IsRewindCast());
		}

		public GenericX Extrapolate()
		{
			return Extrapolate(target.elements[index].transform, snapshot.elements[index].transform);
		}

		public void Teleport(GenericX genx)
		{
			Teleport(Compress(genx), genx);
		}

		public void Teleport(CompressedElement compressed, GenericX decompressed)
		{
			// if this is rootRotation, and has an rb that is not isKinematic, make it kinematic temporarily for this teleport
			bool setKinematic = (index == 0 && nst.rb != null && !nst.rb.isKinematic);

			if (setKinematic)
				nst.rb.isKinematic = true;

			Localized = decompressed;
			lastSentCompressed = compressed;
			lastSentTransform = decompressed;

			if (snapshot != null)
				snapshot.elements[index].transform = decompressed;

			if (target != null)
				target.elements[index].transform = decompressed;

			if (setKinematic)
				nst.rb.isKinematic = false;

		}

		public void Teleport()
		{
			Teleport(Localized);
		}

		public abstract GenericX Extrapolate(GenericX curr, GenericX prev);
		public abstract void UpdateInterpolation(float t);
		public abstract GenericX Lerp(GenericX start, GenericX end, float t);

	}



#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(TransformElement))]
	[CanEditMultipleObjects]

	public abstract class TransformElementDrawer : PropertyDrawer
	{
		public static Color positionHeaderBarColor = new Color(0.545f, 0.305f, 0.062f);
		public static Color rotationHeaderBarColor = new Color(0.447f, 0.184f, 0.529f);
		public static Color red = new Color(.7f, .6f, .6f);
		public static Color green = new Color(.6f, .7f, .6f);
		public static Color blue = new Color(.6f, .6f, .7f);
		public static Color purple = new Color(.3f, .2f, .3f);
		public static Color orange = new Color(.3f, .25f, .2f);

		protected const int LINEHEIGHT = 16;
		protected float rows = 16;

		protected float left;
		protected float margin;
		protected float realwidth;
		protected float colwidths;
		protected float currentLine;
		protected int savedIndentLevel;
		protected bool isPos;

		protected bool noUpdates;

		protected SerializedProperty name;
		protected SerializedProperty isRoot;
		protected SerializedProperty elementType;
		protected SerializedProperty keyRate;
		protected SerializedProperty sendCullMask;
		protected SerializedProperty gameobject;
		protected SerializedProperty extrapolation;
		protected SerializedProperty maxExtrapolates;

		protected SerializedProperty teleportOverride;

		public static GUIStyle lefttextstyle = new GUIStyle
		{
			alignment = TextAnchor.UpperLeft,
			richText = true
		};

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, property);
			var par = PropertyDrawerUtility.GetParent(property);

			GameObject parGO;

			// the parent may be an NST or a TransformElement
			if (par is NSTElementComponent)
				parGO = (par as NSTElementComponent).gameObject;
			else
				parGO = (par as NetworkSyncTransform).gameObject;

			TransformElement te = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TransformElement>(fieldInfo, property);

			name = property.FindPropertyRelative("name");
			isRoot = property.FindPropertyRelative("isRoot");
			elementType = property.FindPropertyRelative("elementType");
			keyRate = property.FindPropertyRelative("keyRate");
			sendCullMask = property.FindPropertyRelative("sendCullMask");
			gameobject = property.FindPropertyRelative("gameobject");
			extrapolation = property.FindPropertyRelative("extrapolation");
			maxExtrapolates = property.FindPropertyRelative("maxExtrapolates");
			teleportOverride = property.FindPropertyRelative("teleportOverride");

			isPos = (ElementType)elementType.intValue == ElementType.Position;
			string typeLabel = (isPos) ? "Position" : "Rotation";

			margin = 4;
			realwidth = r.width + 16 - 4;
			colwidths = realwidth / 4f;

			colwidths = Mathf.Max(colwidths, 65); // limit the smallest size so things like sliders aren't shrunk too small to draw.

			currentLine = r.yMin + margin * 2;

			Color headerblockcolor = (isPos ? positionHeaderBarColor : rotationHeaderBarColor);

			//GUI.Box(new Rect(margin, r.yMin + 2, realwidth, r.height - margin), GUIContent.none,  "ProjectBrowserTextureIconDropShadow");
			//GUI.Box(new Rect(margin, r.yMin + 2, realwidth, r.height - margin), GUIContent.none,  "HelpBox"); //"ProjectBrowserTextureIconDropShadow");

			if (!isRoot.boolValue)
			{
				EditorGUI.DrawRect(new Rect(margin + 3, r.yMin + 2 + 2, realwidth - 6, LINEHEIGHT + 8), headerblockcolor);
			}

			savedIndentLevel = EditorGUI.indentLevel;

			EditorGUI.indentLevel = 0;
			if (!isRoot.boolValue)
			{
				string headerLabel = typeLabel + " Element";

				EditorGUI.LabelField(new Rect(r.xMin, currentLine, colwidths * 4, LINEHEIGHT), new GUIContent(headerLabel), "WhiteBoldLabel");

				NSTElementComponentEditor.MakeAllNamesUnique(parGO, te);

				EditorGUI.PropertyField(new Rect(r.xMin, currentLine, r.width - 4, LINEHEIGHT), name, new GUIContent(" "));

				currentLine += LINEHEIGHT + 8;
			}

			else
			{
				EditorGUI.LabelField(new Rect(r.xMin, currentLine, r.width, LINEHEIGHT), new GUIContent("Root Rotation Updates"), "BoldLabel");
				currentLine += LINEHEIGHT + 4;
			}
			EditorGUI.indentLevel = 0;

			// Section for Send Culling enum flags

			left = 13;
			realwidth -= 16;
			sendCullMask.intValue = System.Convert.ToInt32(EditorGUI.EnumMaskPopup(new Rect(left, currentLine, realwidth, LINEHEIGHT), new GUIContent("Send On Events:"), (SendCullMask)sendCullMask.intValue));
			currentLine += LINEHEIGHT + 4;

			if (!isRoot.boolValue)
			{
				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), gameobject, new GUIContent("GameObject:"));
				currentLine += LINEHEIGHT + 4;
			}

			if (((SendCullMask)sendCullMask.intValue).EveryTick() == false)
			{
				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), keyRate, new GUIContent("Key Every:"));
				currentLine += LINEHEIGHT + 2;
			}

			if (keyRate.intValue == 0 && sendCullMask.intValue == 0)
			{
				noUpdates = true;
				EditorGUI.HelpBox(new Rect(left, currentLine, realwidth, 48), typeLabel + " Element Disabled. Select one ore more Send On Events, and/or set Key Every to a number greater than 0.", MessageType.Warning);
				currentLine += 50;

				return;
			}
			else
			{
				noUpdates = false;

				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), extrapolation, new GUIContent("Extrapolation:"));
				currentLine += LINEHEIGHT + 2;

				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), maxExtrapolates, new GUIContent("Max Extrapolations:"));
				currentLine += LINEHEIGHT + 2;

				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), teleportOverride, new GUIContent("Teleport Override:"));
				currentLine += LINEHEIGHT + 2;
			}

		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{

			SerializedProperty drawerHeight = property.FindPropertyRelative("drawerHeight");

			if (drawerHeight.floatValue > 1)
				return drawerHeight.floatValue;

			return base.GetPropertyHeight(property, label) * rows;  // assuming original is one row
		}
	}
#endif
}

