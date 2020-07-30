﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

using UnityEngine;

namespace Danbi {
  public class DanbiShaderHelper : MonoBehaviour {
    public static void CreateComputeBuffer<T>(ComputeBuffer buffer, List<T> data, int stride)
      where T : struct {

      if (!buffer.Null()) {
        // if there's no data or buffer which doesn't match the given criteria, release it.
        if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
          buffer.Release();
          buffer = null;
        }

        if (data.Count != 0) {
          // If the buffer has been released or wasn't there to begin with, create it!
          if (buffer.Null()) {
            buffer = new ComputeBuffer(data.Count, stride);
          }
          buffer.SetData(data);
        }

      }
    }

    public static ComputeBuffer CreateComputeBuffer_Ret<T>(List<T> data, int stride)
      where T : struct {
      var res = new ComputeBuffer(data.Count, stride);
      res.SetData(data);
      return res;
    }
  };
};

