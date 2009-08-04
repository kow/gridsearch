/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using OpenMetaverse.Assets;

namespace OpenMetaverse.Imaging
{
    /// <summary>
    /// A set of textures that are layered on texture of each other and "baked"
    /// in to a single texture, for avatar appearances
    /// </summary>
    public class Baker
    {
        #region Properties
        /// <summary>Final baked texture</summary>
        public AssetTexture BakedTexture { get { return bakedTexture; } }
        /// <summary>Component layers</summary>
        public List<AppearanceManager.TextureData> Textures { get { return textures; } }
        /// <summary>Width of the final baked image and scratchpad</summary>
        public int BakeWidth { get { return bakeWidth; } }
        /// <summary>Height of the final baked image and scratchpad</summary>
        public int BakeHeight { get { return bakeHeight; } }
        /// <summary>Bake type</summary>
        public BakeType BakeType { get { return bakeType; } }
        /// <summary>Is this one of the 3 skin bakes</summary>
        private bool IsSkin { get { return bakeType == BakeType.Head || bakeType == BakeType.LowerBody || bakeType == BakeType.UpperBody; } }
        #endregion

        #region Private fields
        /// <summary>Final baked texture</summary>
        private AssetTexture bakedTexture;
        /// <summary>Component layers</summary>
        private List<AppearanceManager.TextureData> textures = new List<AppearanceManager.TextureData>();
        /// <summary>Width of the final baked image and scratchpad</summary>
        private int bakeWidth;
        /// <summary>Height of the final baked image and scratchpad</summary>
        private int bakeHeight;
        /// <summary>Bake type</summary>
        private BakeType bakeType;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="bakeType"><Bake type/param>
        public Baker(BakeType bakeType)
        {
            this.bakeType = bakeType;

            if (bakeType == BakeType.Eyes)
            {
                bakeWidth = 128;
                bakeHeight = 128;
            }
            else
            {
                bakeWidth = 512;
                bakeHeight = 512;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds layer for baking
        /// </summary>
        /// <param name="tdata">TexturaData struct that contains texture and its params</param>
        public void AddTexture(AppearanceManager.TextureData tdata)
        {
            lock (textures)
            {
                textures.Add(tdata);
            }
        }

        public void Bake()
        {
            bakedTexture = new AssetTexture(new ManagedImage(bakeWidth, bakeHeight,
                ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha | ManagedImage.ImageChannels.Bump));

            // Base color for eye bake is white, color of layer0 for others
            if (bakeType == BakeType.Eyes)
            {
                InitBakedLayerColor(Color.White);
            }
            else if (textures.Count > 0)
            {
                InitBakedLayerColor(textures[0].Color);
            }

            // If we don't have skin textures, apply defaults
            bool noSkinTexture = textures.Count == 0 || textures[0].Texture == null;

            if (noSkinTexture && bakeType == BakeType.Head)
            {
                DrawLayer(LoadResourceLayer("head_color.tga"), false);
                AddAlpha(bakedTexture.Image, LoadResourceLayer("head_alpha.tga"));
                MultiplyLayerFromAlpha(bakedTexture.Image, LoadResourceLayer("head_skingrain.tga"));
            }

            if (noSkinTexture && bakeType == BakeType.UpperBody)
            {
                DrawLayer(LoadResourceLayer("upperbody_color.tga"), false);
            }

            if (noSkinTexture && bakeType == BakeType.LowerBody)
            {
                DrawLayer(LoadResourceLayer("lowerbody_color.tga"), false);
            }



            // Layer each texture on top of one other, applying alpha masks as we go
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].Texture == null) continue;

                ManagedImage texture = textures[i].Texture.Image.Clone();

                // Resize texture to the size of baked layer
                // FIXME: if texture is smaller than the layer, don't stretch it, tile it
                if (texture.Width != bakeWidth || texture.Height != bakeHeight)
                {
                    try { texture.ResizeNearestNeighbor(bakeWidth, bakeHeight); }
                    catch (Exception) { continue; }
                }

                // Aply tint except for skin texture that is on layer 0
                if (!(IsSkin && i == 0))
                {
                    ApplyTint(texture, textures[i].Color);
                }

                // Apply parametrized alpha masks
                if (textures[i].AlphaMasks != null && textures[i].AlphaMasks.Count > 0)
                {
                    // Combined mask for the layer, fully transparent to begin with
                    ManagedImage combinedMask = new ManagedImage(bakeWidth, bakeHeight, ManagedImage.ImageChannels.Alpha);
                    int addedMasks = 0;

                    // First add mask in normal blend mode
                    foreach (KeyValuePair<VisualAlphaParam, float> kvp in textures[i].AlphaMasks)
                    {
                        if (!MaskBelongsToBake(kvp.Key.TGAFile)) continue;

                        if (kvp.Key.MultiplyBlend == false)
                        {
                            ApplyAlpha(combinedMask, kvp.Key, kvp.Value);
                            addedMasks++;
                        }
                    }

                    // If there were no mask in normal blend mode make aplha fully opaque
                    if (addedMasks == 0) for (int l = 0; l < combinedMask.Alpha.Length; l++) combinedMask.Alpha[l] = 255;

                    // Add masks in multiply blend mode
                    foreach (KeyValuePair<VisualAlphaParam, float> kvp in textures[i].AlphaMasks)
                    {
                        if (!MaskBelongsToBake(kvp.Key.TGAFile)) continue;

                        if (kvp.Key.MultiplyBlend == true)
                        {
                            ApplyAlpha(combinedMask, kvp.Key, kvp.Value);
                            addedMasks++;
                        }
                    }

                    // Finally, apply combined alpha mask to the cloned texture
                    if (addedMasks > 0)
                        AddAlpha(texture, combinedMask);
                }

                // Only skirt and head bake have alpha channels
                bool useAlpha = i == 0 && (bakeType == BakeType.Head || BakeType == BakeType.Skirt);
                DrawLayer(texture, useAlpha);
            }

            // We are done, encode asset for finalized bake
            bakedTexture.Encode();
        }

        public static ManagedImage LoadResourceLayer(string fileName)
        {
            try
            {
                Stream stream = Helpers.GetResourceStream(fileName, Settings.RESOURCE_DIR);
                Bitmap bitmap = LoadTGAClass.LoadTGA(stream);
                stream.Close();
                stream.Dispose();
                return new ManagedImage(bitmap);
            }
            catch (Exception e)
            {
                Logger.Log(String.Format("Failed loading resource file: {0} ({1})", fileName, e.Message),
                    Helpers.LogLevel.Error, e);
                return null;
            }
        }

        /// <summary>
        /// Converts avatar texture index (face) to Bake type
        /// </summary>
        /// <param name="index">Face number (AvatarTextureIndex)</param>
        /// <returns>BakeType, layer to which this texture belongs to</returns>
        public static BakeType BakeTypeFor(AvatarTextureIndex index)
        {
            switch (index)
            {
                case AvatarTextureIndex.HeadBodypaint:
                    return BakeType.Head;

                case AvatarTextureIndex.UpperBodypaint:
                case AvatarTextureIndex.UpperGloves:
                case AvatarTextureIndex.UpperUndershirt:
                case AvatarTextureIndex.UpperShirt:
                case AvatarTextureIndex.UpperJacket:
                    return BakeType.UpperBody;

                case AvatarTextureIndex.LowerBodypaint:
                case AvatarTextureIndex.LowerUnderpants:
                case AvatarTextureIndex.LowerSocks:
                case AvatarTextureIndex.LowerShoes:
                case AvatarTextureIndex.LowerPants:
                case AvatarTextureIndex.LowerJacket:
                    return BakeType.LowerBody;

                case AvatarTextureIndex.EyesIris:
                    return BakeType.Eyes;

                case AvatarTextureIndex.Skirt:
                    return BakeType.Skirt;

                case AvatarTextureIndex.Hair:
                    return BakeType.Hair;

                default:
                    return BakeType.Unknown;
            }
        }
        #endregion

        #region Private layer compositing methods

        private bool MaskBelongsToBake(string mask)
        {
            if (bakeType == BakeType.LowerBody)
                Logger.DebugLog("foo");
            if ((bakeType == BakeType.LowerBody && mask.Contains("upper"))
                || (bakeType == BakeType.LowerBody && mask.Contains("shirt"))
                || (bakeType == BakeType.UpperBody && mask.Contains("lower")))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool DrawLayer(ManagedImage source, bool addSourceAlpha)
        {
            if (source == null) return false;

            bool sourceHasColor;
            bool sourceHasAlpha;
            bool sourceHasBump;
            int i = 0;

            sourceHasColor = ((source.Channels & ManagedImage.ImageChannels.Color) != 0 &&
                    source.Red != null && source.Green != null && source.Blue != null);
            sourceHasAlpha = ((source.Channels & ManagedImage.ImageChannels.Alpha) != 0 && source.Alpha != null);
            sourceHasBump = ((source.Channels & ManagedImage.ImageChannels.Bump) != 0 && source.Bump != null);

            addSourceAlpha = (addSourceAlpha && sourceHasAlpha);

            byte alpha = Byte.MaxValue;
            byte alphaInv = (byte)(Byte.MaxValue - alpha);

            byte[] bakedRed = bakedTexture.Image.Red;
            byte[] bakedGreen = bakedTexture.Image.Green;
            byte[] bakedBlue = bakedTexture.Image.Blue;
            byte[] bakedAlpha = bakedTexture.Image.Alpha;
            byte[] bakedBump = bakedTexture.Image.Bump;

            byte[] sourceRed = source.Red;
            byte[] sourceGreen = source.Green;
            byte[] sourceBlue = source.Blue;
            byte[] sourceAlpha = sourceHasAlpha ? source.Alpha : null;
            byte[] sourceBump = sourceHasBump ? source.Bump : null;

            for (int y = 0; y < bakeHeight; y++)
            {
                for (int x = 0; x < bakeWidth; x++)
                {
                    if (sourceHasAlpha)
                    {
                        alpha = sourceAlpha[i];
                        alphaInv = (byte)(Byte.MaxValue - alpha);
                    }

                    if (sourceHasColor)
                    {
                        bakedRed[i] = (byte)((bakedRed[i] * alphaInv + sourceRed[i] * alpha) >> 8);
                        bakedGreen[i] = (byte)((bakedGreen[i] * alphaInv + sourceGreen[i] * alpha) >> 8);
                        bakedBlue[i] = (byte)((bakedBlue[i] * alphaInv + sourceBlue[i] * alpha) >> 8);
                    }

                    if (addSourceAlpha)
                    {
                        if (sourceAlpha[i] < bakedAlpha[i])
                        {
                            bakedAlpha[i] = sourceAlpha[i];
                        }
                    }

                    if (sourceHasBump)
                        bakedBump[i] = sourceBump[i];

                    ++i;
                }
            }

            return true;
        }

        /// <summary>
        /// Make sure images exist, resize source if needed to match the destination
        /// </summary>
        /// <param name="dest">Destination image</param>
        /// <param name="src">Source image</param>
        /// <returns>Sanitization was succefull</returns>
        private bool SanitazeLayers(ManagedImage dest, ManagedImage src)
        {
            if (dest == null || src == null) return false;

            if ((dest.Channels & ManagedImage.ImageChannels.Alpha) == 0)
            {
                dest.ConvertChannels(dest.Channels | ManagedImage.ImageChannels.Alpha);
            }

            if (dest.Width != src.Width || dest.Height != src.Height)
            {
                try { src.ResizeNearestNeighbor(dest.Width, dest.Height); }
                catch (Exception) { return false; }
            }

            return true;
        }


        private void ApplyAlpha(ManagedImage dest, VisualAlphaParam param, float val)
        {
            ManagedImage src = LoadResourceLayer(param.TGAFile);

            if (dest == null || src == null || src.Alpha == null) return;

            if ((dest.Channels & ManagedImage.ImageChannels.Alpha) == 0)
            {
                dest.ConvertChannels(ManagedImage.ImageChannels.Alpha | dest.Channels);
            }

            Logger.DebugLog(param.TGAFile + " val " + val + " mode " + param.MultiplyBlend + " domain " + param.Domain + " skipz " + param.SkipIfZero);


            if (dest.Width != src.Width || dest.Height != src.Height)
            {
                try { src.ResizeNearestNeighbor(dest.Width, dest.Height); }
                catch (Exception) { return; }
            }

            for (int i = 0; i < dest.Alpha.Length; i++)
            {
                if (param.SkipIfZero)
                    if (src.Alpha[i] == 0)
                        continue;

                byte alpha = src.Alpha[i] <= ((1 - val) * 255) ? (byte)0 : (byte)255;

                if (param.MultiplyBlend)
                {
                    dest.Alpha[i] = (byte)((dest.Alpha[i] * alpha) >> 8);
                    //if (alpha < dest.Alpha[i])
                    //{
                    //    dest.Alpha[i] = alpha;
                    //}
                }
                else
                {
                    if (alpha > dest.Alpha[i])
                    {
                        dest.Alpha[i] = alpha;
                    }
                }
            }
        }

        private void AddAlpha(ManagedImage dest, ManagedImage src)
        {
            if (!SanitazeLayers(dest, src)) return;

            for (int i = 0; i < dest.Alpha.Length; i++)
            {
                if (src.Alpha[i] < dest.Alpha[i])
                {
                    dest.Alpha[i] = src.Alpha[i];
                }
            }
        }

        private void MultiplyLayerFromAlpha(ManagedImage dest, ManagedImage src)
        {
            if (!SanitazeLayers(dest, src)) return;

            for (int i = 0; i < dest.Red.Length; i++)
            {
                dest.Red[i] = (byte)((dest.Red[i] * src.Alpha[i]) >> 8);
                dest.Green[i] = (byte)((dest.Green[i] * src.Alpha[i]) >> 8);
                dest.Blue[i] = (byte)((dest.Blue[i] * src.Alpha[i]) >> 8);
            }
        }

        private void ApplyTint(ManagedImage dest, Color src)
        {
            if (dest == null) return;

            for (int i = 0; i < dest.Red.Length; i++)
            {
                dest.Red[i] = (byte)((dest.Red[i] * src.R) >> 8);
                dest.Green[i] = (byte)((dest.Green[i] * src.G) >> 8);
                dest.Blue[i] = (byte)((dest.Blue[i] * src.B) >> 8);
            }
        }

        /// <summary>
        /// Fills a baked layer as a solid *appearing* color. The colors are 
        /// subtly dithered on a 16x16 grid to prevent the JPEG2000 stage from 
        /// compressing it too far since it seems to cause upload failures if 
        /// the image is a pure solid color
        /// </summary>
        /// <param name="color">System.Drawing.Color of the base of this layer</param>
        private void InitBakedLayerColor(Color color)
        {
            InitBakedLayerColor(color.R, color.G, color.B);
        }

        /// <summary>
        /// Fills a baked layer as a solid *appearing* color. The colors are 
        /// subtly dithered on a 16x16 grid to prevent the JPEG2000 stage from 
        /// compressing it too far since it seems to cause upload failures if 
        /// the image is a pure solid color
        /// </summary>
        /// <param name="r">Red value</param>
        /// <param name="g">Green value</param>
        /// <param name="b">Blue value</param>
        private void InitBakedLayerColor(byte r, byte g, byte b)
        {
            byte rByte = r;
            byte gByte = g;
            byte bByte = b;

            byte rAlt, gAlt, bAlt;

            rAlt = rByte;
            gAlt = gByte;
            bAlt = bByte;

            if (rByte < Byte.MaxValue)
                rAlt++;
            else rAlt--;

            if (gByte < Byte.MaxValue)
                gAlt++;
            else gAlt--;

            if (bByte < Byte.MaxValue)
                bAlt++;
            else bAlt--;

            int i = 0;

            byte[] red = bakedTexture.Image.Red;
            byte[] green = bakedTexture.Image.Green;
            byte[] blue = bakedTexture.Image.Blue;
            byte[] alpha = bakedTexture.Image.Alpha;
            byte[] bump = bakedTexture.Image.Bump;

            for (int y = 0; y < bakeHeight; y++)
            {
                for (int x = 0; x < bakeWidth; x++)
                {
                    if (((x ^ y) & 0x10) == 0)
                    {
                        red[i] = rAlt;
                        green[i] = gByte;
                        blue[i] = bByte;
                        alpha[i] = Byte.MaxValue;
                        bump[i] = 0;
                    }
                    else
                    {
                        red[i] = rByte;
                        green[i] = gAlt;
                        blue[i] = bAlt;
                        alpha[i] = Byte.MaxValue;
                        bump[i] = 0;
                    }

                    ++i;
                }
            }

        }
        #endregion
    }
}
