﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using UnityEngine.UIElements;

namespace Danbi
{
#pragma warning disable 3001
#pragma warning disable 3002
    public static class DanbiComputeShaderHelper
    {
        public static void PrepareRenderTextures((int width, int height) screenResolutions,
                                                 out int samplingCounter,
                                                 ref RenderTexture resRT_lowRes,
                                                 ref RenderTexture convergedRT_highRes)
        {
            // TODO: Need to recreate 
            // Create render textures along the screen resolutions.
            if (resRT_lowRes.Null())
            {
                resRT_lowRes = new RenderTexture(screenResolutions.width,
                                                 screenResolutions.height,
                                                 0,
                                                 //  RenderTextureFormat.Default,
                                                 RenderTextureFormat.ARGBFloat,
                                                 RenderTextureReadWrite.Linear)
                {
                    enableRandomWrite = true
                };

                resRT_lowRes.Create();
            }
            // 2. Create HighRes rt
            if (convergedRT_highRes.Null())
            {
                convergedRT_highRes = new RenderTexture(screenResolutions.width,
                                                        screenResolutions.height,
                                                        0,
                                                        // RenderTextureFormat.ARGB32,
                                                        RenderTextureFormat.ARGBFloat,
                                                        RenderTextureReadWrite.Linear)
                {
                    enableRandomWrite = true
                };
                convergedRT_highRes.Create();
            }
            // 3. reset SamplingCounter
            samplingCounter = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clearedRT"></param>
        public static void ClearRenderTexture(RenderTexture clearedRT)
        {
            // To clear the target render texture, we have to set this as a main frame buffer.
            // so we swap to the previous RT.
            var prevRT = RenderTexture.active;
            RenderTexture.active = clearedRT;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prevRT;
        }

        public static void CreateComputeBuffer<T>(ComputeBuffer buffer, List<T> data, int stride)
          where T : struct
        {
            if (!buffer.Null())
            {
                // if there's no data or buffer which doesn't match the given criteria, release it.
                if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
                {
                    buffer.Release();
                    buffer = null;
                }

                if (data.Count != 0)
                {
                    // If the buffer has been released or wasn't there to begin with, create it!
                    if (buffer.Null())
                    {
                        buffer = new ComputeBuffer(data.Count, stride);
                    }
                    buffer.SetData(data);
                }
            }
        }

        public static ComputeBuffer CreateComputeBuffer_Ret<T>(List<T> data, int stride)
          where T : struct
        {
            var res = new ComputeBuffer(data.Count, stride);
            res.SetData(data);
            System.GC.SuppressFinalize(res);
            // https://forum.unity.com/threads/disposing-of-computebuffer-causes-result-to-screw-up.760178/
            // So i did not quite get the problem you are having, 
            // but i had a lot of problems with releasing an AppendBuffer too. 
            // For me the data from previous calculations still stayed in the buffer 
            // causing it to still have the data of old chunks until the buffer count was higher than my given max and the game broke.
            return res;
        }

        public static ComputeBuffer CreateComputeBuffer_Ret<T>(T data, int stride)
          where T : struct
        {
            var res = new ComputeBuffer(1, stride);
            res.SetData(new List<T> { data });
            System.GC.SuppressFinalize(res);
            // res.SetCounterValue(0);
            return res;
        }

        public static Matrix4x4 OpenCVKMatrixToOpenGLKMatrix(float alpha, float beta, float x0, float y0,
                                                        float near, float far)
        {
            #region comments
            //Our 3x3 intrinsic camera matrix K needs two modifications before it's ready to use in OpenGL.
            //    First, for proper clipping, the (3,3) element of K must be -1. OpenGL's camera looks down the negative z - axis, 
            //    so if K33 is positive, vertices in front of the camera will have a negative w coordinate after projection. 
            //    In principle, this is okay, but because of how OpenGL performs clipping, all of these points will be clipped.

            // If K33 isn't -1, your intrinsic and extrinsic matrices need some modifications. 
            //    Getting the camera decomposition right isn't trivial,
            //    so I'll refer the reader to my earlier article on camera decomposition,
            //    which will walk you through the steps.
            //    Part of the result will be the negation of the third column of the intrinsic matrix, 
            //    so you'll see those elements negated below.



            //   K= \alpha 0  u_0 
            //      \beta 0  v_0
            //       0    0    1  


            //     u0, v0 are the image principle point ,  with f being the focal length and 
            //     being scale factors relating pixels to distance.
            //     Multiplying a point  
            //     by this matrix and dividing by resulting z-coordinate then gives the point projected into the image.
            //The OpenGL parameters are quite different.  Generally the projection is set using the glFrustum command,
            //    which takes the left, right, top, bottom, near and far clip plane locations as parameters
            //    and maps these into "normalized device coordinates" which range from[-1, 1].
            //    The normalized device coordinates are then transformed by the current viewport, 
            //    which maps them onto the final image plane.Because of the differences,
            //    obtaining an OpenGL projection matrix which matches a given set of intrinsic parameters 
            //   is somewhat complicated.


            // construct a projection matrix, this is identical to the 
            // projection matrix computed for the intrinsicx, except an
            // additional row is inserted to map the z-coordinate to
            // OpenGL. 

            //https://github.com/Emerix/AsymFrustum
            //https://answers.unity.com/questions/1359718/what-do-the-values-in-the-matrix4x4-for-cameraproj.html
            // Set an off-center projection, where perspective's vanishing
            // point is not necessarily in the center of the screen.
            //
            // left/right/top/bottom define near plane size, i.e.
            // how offset are corners of camera's near plane.
            // Tweak the values and you can see camera's frustum change.
            //https://stackoverflow.com/questions/2286529/why-does-sign-matter-in-opengl-projection-matrix
            //https://docs.microsoft.com/en-us/windows/win32/opengl/glfrustum?redirectedfrom=MSDN
            //
            //        -The intersection of the optical axis with the image place is called principal point or
            //image center.
            //(note: the principal point is not always the "actual" center of the image)

            //Less commonly, we may wish to translate the 2D normalized device coordinates by
            //[cx, cy]. This can be modeled in the projection matrix as   in p. 95 in Foundations of Computer Graphics
            // In a shifted camera, we translate the normalized device coordinates and
            // keep the[−1..1] region in these shifted coordinates, as shown in Fig. 10.7 in the above book.
            // The [shifted] 3D frustum is defined by specifying an image rectangle on the near
            // plane as in Fig. 10.9 of the book.

            //left, right, top and bottom actually specify the boundary / size of the near-clipping plane. 
            // The "near" distance defines how far away from the camera origin the clipping plane is located.

            //The normalized device coordinates uses a left - handed system
            //    while OpenGL(and mathematics in general) uses a right - handed system.
            //    Unity however already uses a left-handed system.
            //    But since the projection matrix should be compatible with all sorts of APIs, they define it the usual way.
            //    That's why Unity's "camera / view matrix" artifically inverts the z-axis.
            //    That means inside the shader after the model and view transformation the z values are actually negative.
            #endregion comments

            Matrix4x4 PerspK = new Matrix4x4();

            // Debug.Log($"Print the initially created matrix=\n {PerspK}");

            float A = (near + far);
            float B = near * far;

            //http://ksimek.github.io/2012/08/14/decompose/

            // Starting from an all-positive diagonal, follow these four steps:

            //If the image x-axis and your camera x - axis point in opposite directions, negate the first column of K and the first row of R.
            //If the image y-axis and uour camera y - axis point in opposite directions, negate the second column of K and the second row of R.
            //If the camera looks down the negative - z axis, negate the third column of K. 
            //Also negate the third column of R.
            //If the determinant of R is -1, negate it.

            PerspK[0, 0] = alpha;   // scaling factor in x
            PerspK[1, 1] = beta;    // scaling factor in y;

            PerspK[0, 2] = -x0;   // negate the third column of openCV K, because the camera looks down the negative z axis                                
            PerspK[1, 2] = -y0;    // negate the third column of openCV K
            PerspK[2, 2] = A;
            PerspK[2, 3] = B;
            PerspK[3, 2] = -1.0f; // // negate the third column of openCV K

            //Notice that element(3, 2) of the projection matrix is ‘-1’. 
            // This is because the camera looks in the negative-z direction, 
            //  which is the opposite of the convention used by Hartley and Zisserman.
            return PerspK;
        }

        // Based On the Foundation of 3D Computer Graphics (book)
        public static Matrix4x4 GetOrthoMat(float left, float right, float bottom, float top, float near, float far)
        {
            // construct an orthographic matrix which maps from projected
            // coordinates to normalized device coordinates in the range
            // [-1, 1]^3. 

            // Translate the box view volume so that its center is at the origin of the frame
            float tx = -(left + right) / (right - left);
            float ty = -(bottom + top) / (top - bottom);
            float tz = -(near + far) / (far - near);

            // Then scale the translated view volume into the normalized coordinates; The sign of the z coordinate
            // is changed so that the negative z (In openGL space) becomes positive (in the NDC space). 
            float m00 = 2.0f / (right - left);
            float m11 = 2.0f / (top - bottom);
            float m22 = -2.0f / (far - near);


            Matrix4x4 Ortho = new Matrix4x4();   // member fields are init to zero
            Ortho[0, 0] = m00;
            Ortho[1, 1] = m11;
            Ortho[2, 2] = m22;
            Ortho[0, 3] = tx;
            Ortho[1, 3] = ty;
            Ortho[2, 3] = tz;
            Ortho[3, 3] = 1.0f;

            return Ortho;
        }

        public static Vector3 GetScale(Matrix4x4 m) => new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);

        public static Quaternion GetRotation(Matrix4x4 m)
        {
            Vector3 s = GetScale(m);

            // Normalize Scale from Matrix4x4
            float m00 = m[0, 0] / s.x;
            float m01 = m[0, 1] / s.y;
            float m02 = m[0, 2] / s.z;
            float m10 = m[1, 0] / s.x;
            float m11 = m[1, 1] / s.y;
            float m12 = m[1, 2] / s.z;
            float m20 = m[2, 0] / s.x;
            float m21 = m[2, 1] / s.y;
            float m22 = m[2, 2] / s.z;

            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m00 + m11 + m22)) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m00 - m11 - m22)) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m00 + m11 - m22)) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m00 - m11 + m22)) / 2;
            q.x *= Mathf.Sign(q.x * (m21 - m12));
            q.y *= Mathf.Sign(q.y * (m02 - m20));
            q.z *= Mathf.Sign(q.z * (m10 - m01));

            // q.Normalize()
            float qMagnitude = Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
            q.w /= qMagnitude;
            q.x /= qMagnitude;
            q.y /= qMagnitude;
            q.z /= qMagnitude;

            return q;
        }

        public static Vector3 GetPosition(Matrix4x4 m)
        {
            return new Vector3(m[0, 3], m[1, 3], m[2, 3]);
        }
    };
};

