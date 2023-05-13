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

    public static string ToHexA(this Color color) {
      return $"#{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}";
    }

    public static Color HexToColorA(this string hex) {
      hex = hex.TrimStart('#');
      if (hex.Length < 6) {
        // todo: wtf
        return Color.White;
      }

      float r = (float)(Calc.HexToByte(hex[0]) * 16 + Calc.HexToByte(hex[1])) / 255f;
      float g = (float)(Calc.HexToByte(hex[2]) * 16 + Calc.HexToByte(hex[3])) / 255f;
      float b = (float)(Calc.HexToByte(hex[4]) * 16 + Calc.HexToByte(hex[5])) / 255f;

      if (hex.Length < 8) {
        return new Color(r, g, b);
      }
      float a = (float)(Calc.HexToByte(hex[6]) * 16 + Calc.HexToByte(hex[7])) / 255f;
      return new Color(r, g, b, a);
    }
  }
}
