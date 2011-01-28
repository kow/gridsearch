/*
 * Copyright (C) 2010, Linden Research, Inc. and Open Metaverse Foundation
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation;
 * version 2.1 of the License only.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;

namespace OpenMetaverse.Rendering
{
    public partial class LindenRenderer : IRendering
    {
        public void TransformTexCoords(List<Vertex> vertices, Vector3 center, Primitive.TextureEntryFace teFace)
        {
            float r = teFace.Rotation;
            float os = teFace.OffsetU;
            float ot = teFace.OffsetV;
            float ms = teFace.RepeatU;
            float mt = teFace.RepeatV;
            float cosAng = (float)Math.Cos(r);
            float sinAng = (float)Math.Sin(r);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex vertex = vertices[i];

                if (teFace.TexMapType == MappingType.Default)
                {
                    TransformTexCoord(ref vertex.TexCoord, cosAng, sinAng, os, ot, ms, mt);
                }
                else if (teFace.TexMapType == MappingType.Planar)
                {
                    Vector3 vec = vertex.Position;
                    vec.X *= vec.X;
                    vec.Y *= vec.Y;
                    vec.Z *= vec.Z;

                    TransformPlanarTexCoord(ref vertex.TexCoord, vertex, center, vec);
                }

                vertices[i] = vertex;
            }
        }

        private static void TransformTexCoord(ref Vector2 texCoord, float cosAng, float sinAng, float offsetS,
            float offsetT, float magS, float magT)
        {
            float s = texCoord.X;
            float t = texCoord.Y;

            // Texture transforms are done about the center of the face
            s -= 0.5f;
            t -= 0.5f;

            // Rotation
            float temp = s;
            s = s * cosAng + t * sinAng;
            t = -temp * sinAng + t * cosAng;

            // Scale
            s *= magS;
            t *= magT;

            // Offset
            s += offsetS + 0.5f;
            t += offsetT + 0.5f;

            texCoord.X = s;
            texCoord.Y = t;
        }

        private static void TransformPlanarTexCoord(ref Vector2 texCoord, Vertex vertex, Vector3 center,
            Vector3 vec)
        {
            Vector3 binormal;
            float d = Vector3.Dot(vertex.Normal, Vector3.UnitX);
            if (d >= 0.5f || d <= -0.5f)
            {
                binormal = new Vector3(0f, 1f, 0f);

                if (vertex.Normal.X < 0f)
                {
                    binormal.X = -binormal.X;
                    binormal.Y = -binormal.Y;
                    binormal.Z = -binormal.Z;
                }
            }
            else
            {
                binormal = new Vector3(1f, 0f, 0f);

                if (vertex.Normal.Y > 0f)
                {
                    binormal.X = -binormal.X;
                    binormal.Y = -binormal.Y;
                    binormal.Z = -binormal.Z;
                }
            }

            Vector3 tangent = binormal % vertex.Normal;

            texCoord.Y = -(Vector3.Dot(tangent, vec) * 2f - 0.5f);
            texCoord.X = 1f + (Vector3.Dot(binormal, vec) * 2f - 0.5f);
        }
    }
}
