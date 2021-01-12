// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorDemo.MathEditor
{
    public struct Vector2
    {
        public float X, Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(Vector2 v2, float z)
            : this(v2.X, v2.Y, z)
        {
        }
    }

    public struct Vector4
    {
        public float X, Y, Z, W;
        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4(Vector3 v3, float w)
            : this(v3.X, v3.Y, v3.Z, w)
        {
        }

        public Vector4(Vector2 v2, float z, float w)
            : this(v2.X, v2.Y, z, w)
        {
        }

        public Vector4(Vector2 v21, Vector2 v22)
            : this(v21.X, v21.Y, v22.X, v22.Y)
        {
        }

    }
}
