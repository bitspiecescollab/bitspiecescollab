using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;
using Monocle;

using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;


namespace Celeste.Mod.CustomOshiro {
  static class Util {
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
      if (val.CompareTo(min) < 0) {
        return min;
      } else if (val.CompareTo(max) > 0) {
        return max;
      } else {
        return val;
      }
    }
  }
}
