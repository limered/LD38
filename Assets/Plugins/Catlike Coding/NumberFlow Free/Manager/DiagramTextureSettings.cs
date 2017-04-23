﻿// Copyright 2014, Catlike Coding, http://catlikecoding.com
using UnityEngine;
using System;

namespace CatlikeCoding.NumberFlow {

	/// <summary>
	/// Settings to use for all textures generated by a NumberFlow diagram.
	/// </summary>
	[Serializable]
	public class DiagramTextureSettings {

		/// <summary>
		/// Width in pixels.
		/// </summary>
		[Range(2, 4096)]
		public int width;

		/// <summary>
		/// Height in pixels.
		/// </summary>
		[Range(2, 4096)]
		public int height;

		/// <summary>
		/// Whether to generate mipmaps.
		/// </summary>
		public bool mipmap;

		/// <summary>
		/// Whether the texture is in linear color space.
		/// For use with linear lighting for textures that should not be interpreted as being in gamme space.
		/// </summary>
		public bool linear;

		/// <summary>
		/// Normal format.
		/// </summary>
		public DiagramNormalFormat normalFormat;

		/// <summary>
		/// Wrap mode.
		/// </summary>
		public TextureWrapMode wrapMode;

		/// <summary>
		/// Filter mode.
		/// </summary>
		public FilterMode filterMode;

		/// <summary>
		/// Anisotropic filtering level.
		/// </summary>
		[Range(1, 9)]
		public int anisoLevel;
	}
}