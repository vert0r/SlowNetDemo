﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.SmartVars;

namespace emotitron.Network.NST
{
	public interface INSTTransformElement
	{
		GameObject GameObject { get; }
		TransformElement TransElement { get; }
	}

	public interface ITransformElements
	{
		GenericX Localized { get; set; }
		bool this[int axisId] { get; }
		Vector3 ClampAxes(Vector3 unclamped);
		void Apply(GenericX pos, GameObject targetGO);
		void Apply(GenericX pos);
	}

	public interface IPositionElement : ITransformElements
	{

	}

	public interface IRotationElement : ITransformElements
	{
		RotationType RotationType { get; }
	}
}

