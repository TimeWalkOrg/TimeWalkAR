﻿using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Lookup Filter (Color Grading)")]
public class CC_LookupFilter : CC_Base
{
	public Texture lookupTexture;

	protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (lookupTexture == null)
		{
			Graphics.Blit(source, destination);
			return;
		}

		material.SetTexture("_LookupTex", lookupTexture);
		Graphics.Blit(source, destination, material, IsLinear ? 1 : 0);
	}
}
