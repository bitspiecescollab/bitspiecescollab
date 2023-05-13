using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

using MonoMod.RuntimeDetour;
using MonoMod.Utils;

using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;


namespace Celeste.Mod.CustomOshiro {
  [CustomEntity("CustomOshiro/FloatyOshiro")]
  [TrackedAs(typeof(AngryOshiro))]
  class FloatyOshiro : AngryOshiro {
    public static void Load() {
      On.Celeste.AngryOshiro.ChaseCoroutine += onChaseCoroutine;
      On.Celeste.AngryOshiro.ChaseUpdate += onChaseUpdate;
      using (new DetourContext("CustomOshiro") {
        Before = { "*" }
      }) {
        On.Celeste.AngryOshiro.Render += onRender;
      }
    }

    public static void Unload() {
      On.Celeste.AngryOshiro.ChaseCoroutine -= onChaseCoroutine;
      On.Celeste.AngryOshiro.ChaseUpdate -= onChaseUpdate;
      On.Celeste.AngryOshiro.Render -= onRender;
    }

    public bool active = false;
    public float ratio = 0f;
    public Vector2 from, to;

    public float speed;
    public float fadeSpeed;
    public string flag;
    public Color recolor;

    private Tween fadeTween;
    private float fadeEased = 0f;

    public FloatyOshiro(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
      DynamicData dd = DynamicData.For(this);

      this.from = data.Nodes[0] + offset;
      this.to = data.Nodes[1] + offset;
      this.recolor = data.Attr("recolor", "#FFFFFFFF").HexToColor();

      this.speed = data.Float("speed", 1.0f) * 8f;
      this.fadeSpeed = data.Float("fadeSpeed", 0.75f);
      this.flag = data.Attr("flag", null);
      this.flag = this.flag == "" ? null : this.flag;

      base.Add(this.fadeTween = Tween.Create(
        Tween.TweenMode.Persist, Ease.QuadIn, this.fadeSpeed
      ));
      this.fadeTween.OnUpdate = (Tween t) => {
        this.fadeEased = t.Eased;
      };

      // new Ray(this.from, this.to - this.from).Intersects(new Plane);
      // this.ratio =

      this.Collider = new Circle(data.Float("hitboxRadius", 14.0f), 0f, 0f);
      base.Collider.Position = new Vector2(0f, 0f);

      string texture = data.Attr("texture", "oshiro_boss");
      if (texture != "oshiro_boss") {
        base.Remove(this.Sprite);
        base.Add(this.Sprite = GFX.SpriteBank.Create(texture));
      }

      bool noLight = data.Bool("noLighting", false);
      if (noLight) {
        dd.Get<VertexLight>("light").Alpha = 0f;
      }

      this.active = false;
      this.fadeOut(instant: true);

      bool defaultEnabled = data.Bool("enabled", true);
      if (defaultEnabled) {
        // todo: include offset here!
        this.motionStart();
      }
    }

    public void motionStart(float offset = 0f, bool instant = false) {
      if (this.active) { return; }
      if (this.flag != null && (Scene as Level).Session.GetFlag(this.flag)) {
        return;
      }

      this.ratio = offset;
      this.active = true;
      this.fadeIn(instant: instant);

      if (this.flag != null) {
        (Scene as Level).Session.SetFlag(this.flag, true);
      }
    }

    public void motionStop(bool instant = false) {
      if (!this.active) { return; }
      if (this.flag != null && !(Scene as Level).Session.GetFlag(this.flag)) {
        return;
      }

      this.active = false;
      this.fadeOut(instant: instant);

      if (this.flag != null) {
        (Scene as Level).Session.SetFlag(this.flag, false);
      }
    }

    public void motionLoop(bool instant = false) {
      if (!this.active || fadeTween.Active) { return; }

      this.fadeOut(instant: instant);
    }

    public void fadeIn(bool instant = false) {
      this.Visible = true;
      this.Collidable = true;
      this.fadeEased = 0f;

      if (instant == false) {
        this.fadeTween.OnComplete = (Tween t) => {
          this.Visible = true;
          this.Collidable = true;
        };
        this.fadeTween.Start();
      } else {
        this.fadeTween.Stop();
        this.Visible = true;
        this.Collidable = true;
      }
    }

    public void fadeOut(bool instant = false) {
      if (!active) { return; }

      this.Visible = true;
      this.Collidable = true;
      this.fadeEased = 1f;

      if (instant == false) {
        this.fadeTween.OnComplete = (Tween t) => {
          this.Visible = false;
          this.Collidable = false;

          if (active) {
            this.motionStart();
          }
        };
        this.fadeTween.Start(true);
      } else {
        this.fadeTween.Stop();
        this.Visible = false;
        this.Collidable = false;
      }
    }

    private void updateFlags() {
      // note: we don't setflag when fading... this may cause a bug later!
      if (this.flag == null || this.fadeTween.Active) { return; }

      // todo: we need to not do this if the active state was changed inbetween frames last time, but the flag wasn't updated
      bool flag = (Scene as Level).Session.GetFlag(this.flag);
      if (flag && !this.active) {
        this.motionStart();
      } else if (!flag && this.active) {
        this.motionStop();
      }

      (Scene as Level).Session.SetFlag(this.flag, this.active);
    }

    public Vector2 computePos() {
      Vector2 ray_delta = this.to - this.from;
      ray_delta *= ratio;
      return this.from + ray_delta;
    }

    private static IEnumerator onChaseCoroutine(
      On.Celeste.AngryOshiro.orig_ChaseCoroutine orig, AngryOshiro self
    ) {
      if (self is FloatyOshiro self_o) {
        // never dash dumbass
        self_o.Sprite.Play("idle", false, false);
        yield break;
      } else {
        yield return new SwapImmediately(orig(self));
      }
    }

    private static int onChaseUpdate(
      On.Celeste.AngryOshiro.orig_ChaseUpdate orig, AngryOshiro self
    ) {
      if (self is FloatyOshiro self_o) {
        self_o.updateFlags();

        if (self_o.Visible && self_o.ratio > 1) {
          float norm = Vector2.Distance(self_o.to - self_o.from, Vector2.Zero);
          float delta_ratio = (self_o.speed * Engine.DeltaTime) / norm;
          self_o.ratio += delta_ratio;

          // if (self_o.ratio > 1f) {
          //   self_o.ratio = 0f;
          // } else if (self_o.ratio < 0f) {
          //   self_o.ratio = 1f;
          // }

          Vector2 pos = self_o.computePos();
          self_o.CenterX = pos.X;
          self_o.CenterY = pos.Y;
        }

        return 0;
      } else {
        return orig(self);
      }
    }

    private static void onRender(
      On.Celeste.AngryOshiro.orig_Render orig, AngryOshiro self
    ) {
      if (self is FloatyOshiro self_o) {
        float norm = (self_o.to - self_o.from).Length();
        float speed = self_o.speed / norm;

        // float edgeRatio = 0.5f - Math.Abs(self_o.ratio - 0.5f);
        if ((1f - self_o.ratio) <= speed * self_o.fadeSpeed) {
          self_o.motionLoop();
        }

        // float fadeDistance = norm / self_o.speed;
        // float tiles = self_o.ratio * norm;

        // float edgeTiles = ((norm / 2) - Math.Abs(tiles - (norm / 2)));
        // if (edgeTiles)

        Color spriteColor = self_o.recolor;
        spriteColor = spriteColor * Math.Min(self_o.fadeEased, 1f);
        self_o.Sprite.SetColor(spriteColor);

        orig(self);
        return;
      }

      orig(self);
    }
  }

}
