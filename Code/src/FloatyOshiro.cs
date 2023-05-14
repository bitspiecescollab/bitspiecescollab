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


namespace Celeste.Mod.BitsPieces.FloatyOshiro {
  [CustomEntity("BitsPieces.FloatyOshiro/FloatyOshiro")]
  [TrackedAs(typeof(AngryOshiro))]
  class FloatyOshiro : AngryOshiro {
    public static void Load() {
      On.Celeste.AngryOshiro.ChaseCoroutine += onChaseCoroutine;
      On.Celeste.AngryOshiro.ChaseUpdate += onChaseUpdate;
      On.Celeste.AngryOshiro.WaitingUpdate += onWaitingUpdate;
      On.Celeste.AngryOshiro.Render += onRender;
    }

    public static void Unload() {
      On.Celeste.AngryOshiro.ChaseCoroutine -= onChaseCoroutine;
      On.Celeste.AngryOshiro.ChaseUpdate -= onChaseUpdate;
      On.Celeste.AngryOshiro.WaitingUpdate -= onWaitingUpdate;
      On.Celeste.AngryOshiro.Render -= onRender;
    }

    public bool defaultEnabled;
    public bool running = false;
    public float ratio = 0f;
    public Vector2 from, to;

    public float speed;
    public float fadeSpeed;
    public string flag;
    public Color recolor;

    private Tween fadeTween;
    private float fadeEased;

    private Tween wobbleTween;
    private float wobbleSpeed;
    private float wobbleAmplitude;

    public int framesPerParticle;
    public float particleRadius;
    public ParticleType trailParticle;

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
        Logger.Log(LogLevel.Info, "FloatyOshiro", $"tween = {t.Eased}/{t.Percent}, visible = {this.Visible}");
        // workaround for below Tween::OnComplete bug
        DynamicData.For(t).Set("Mode", Tween.TweenMode.Persist);
        this.fadeEased = t.Eased;
      };

      this.wobbleSpeed = data.Float("wobbleSpeed", 0.75f);
      this.wobbleAmplitude = data.Float("wobbleAmplitude", 3f * 8);
      base.Add(this.wobbleTween = Tween.Create(
        Tween.TweenMode.Looping, Ease.SineInOut, this.wobbleSpeed
      ));
      this.wobbleTween.Start();

      this.ratio = (data.Position - this.from).Length() / (this.to - this.from).Length();
      this.ratio = this.ratio.Clamp(0f, 1f);

      Vector2 pos = this.computePos();
      base.CenterX = pos.X;
      base.CenterY = pos.Y;

      this.Collider = new Circle(data.Float("hitboxRadius", 14.0f), 0f, 0f);
      base.Collider.Position = new Vector2(0f, 0f);

      string texture = data.Attr("texture", "oshiro_boss");
      if (texture != "oshiro_boss") {
        base.Remove(this.Sprite);
        base.Add(this.Sprite = GFX.SpriteBank.Create(texture));
      }

      this.Sprite.FlipX = this.to.X < this.from.X;

      bool noLight = data.Bool("noLighting", false);
      if (noLight) {
        dd.Get<VertexLight>("light").Alpha = 0f;
      }

      this.framesPerParticle = data.Int("framesPerParticle", 3);
      this.particleRadius = data.Float("particleRadius", 10f);
      trailParticle = new ParticleType {
        Color = data.Attr("particleColor1", "#44B7FF").HexToColor(),
        Color2 = data.Attr("particleColor2", "#75C9FF").HexToColor(),
        ColorMode = ParticleType.ColorModes.Blink,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.7f,
        LifeMax = 1.5f,
        Size = 1f,
        SpeedMin = speed*0.5f*0.75f,
        SpeedMax = speed*0.5f*1.25f,
        Acceleration = Vector2.Zero,  // is set as needed
        DirectionRange = (float) Math.PI / 16f
      };

      this.running = false;
      this.fadeOut(instant: true);

      this.defaultEnabled = data.Bool("enabled", true);
    }

    public override void Added(Scene scene) {
      base.Added(scene);
      if (this.defaultEnabled) {
        this.motionStart(this.ratio);
      }

      if (this.flag != null && (scene as Level)?.Session != null) {
        (scene as Level).Session.SetFlag(this.flag, this.defaultEnabled);
      }
    }

    public void motionStart(float offset = 0f, bool instant = false, bool force = false) {
      if (this.running && !force) { return; }

      this.ratio = offset;
      this.running = true;
      this.fadeIn(instant: instant);

      if (this.flag != null && (Scene as Level)?.Session != null) {
        (Scene as Level).Session.SetFlag(this.flag, true);
      }
    }

    public void motionStop(bool instant = false, bool force = false) {
      if (!this.running && !force) { return; }

      this.running = false;
      this.fadeOut(instant: instant);

      if (this.flag != null && (Scene as Level)?.Session != null) {
        (Scene as Level).Session.SetFlag(this.flag, false);
      }
    }

    public void motionLoop(bool instant = false) {
      if (!this.running || fadeTween.Active) { return; }

      this.fadeOut(instant: instant, restart: true);

      if (this.flag != null && (Scene as Level)?.Session != null) {
        (Scene as Level).Session.SetFlag(this.flag, true);
      }
    }

    public void fadeIn(bool instant = false) {
      this.Visible = true;
      this.Collidable = true;

      if (!instant) {
        this.fadeEased = 0f;
        this.fadeTween.OnComplete = (Tween t) => {
        };
        this.fadeTween.Start(reverse: false);
        this.fadeTween.Reset();
        this.fadeTween.Start(reverse: false);
      } else {
        this.fadeTween.Stop();
        this.fadeEased = 1f;
      }
    }

    public void fadeOut(bool instant = false, bool restart = false) {
      this.Visible = true;
      this.Collidable = true;

      if (!instant) {
        this.fadeEased = 1f;
        this.fadeTween.OnComplete = (Tween t) => {
          this.Visible = false;
          this.Collidable = false;

          if (restart) {
            this.motionStart(instant: instant, force: true);

            // bug: Tween calls OnComplete *before* running finalizer because Monocle
            // is very well designed with no problems whatsoever. this means
            // that we can't actually restart the tween in this event handler,
            // because it will get immediately disabled by the finalizer code in
            // Tween::Update.
            // instead, workaround this by temporarily setting the tween mode to
            // an invalid state so no finalizer code is ran
            DynamicData.For(t).Set("Mode", (Tween.TweenMode) 420);
          }
        };
        this.fadeTween.Start(reverse: true);
      } else {
        this.fadeTween.Stop();
        this.fadeEased = 0f;
        this.Visible = false;
        this.Collidable = false;
      }
    }

    private void updateFlags() {
      if (this.flag == null || this.fadeTween.Active || (Scene as Level)?.Session == null) {
        return;
      }

      bool flag = (Scene as Level).Session.GetFlag(this.flag);
      if (flag && !this.running) {
        this.motionStart();
      } else if (!flag && this.running) {
        this.motionStop();
      }

      (Scene as Level).Session.SetFlag(this.flag, this.running);
    }

    public Vector2 computePos() {
      Vector2 ray_delta = this.to - this.from;
      ray_delta *= ratio;

      Vector2 pos = this.from + ray_delta;
      Vector2 wobble = ray_delta.Perpendicular() * this.wobbleTween.Eased * this.wobbleAmplitude;

      return pos + wobble;
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

        if (self_o.Visible && self_o.fadeEased > 0) {
          float norm = Vector2.Distance(self_o.to - self_o.from, Vector2.Zero);
          float delta_ratio = (self_o.speed * Engine.DeltaTime) / norm;
          self_o.ratio += delta_ratio;

          Vector2 pos = self_o.computePos();
          self_o.CenterX = pos.X;
          self_o.CenterY = pos.Y;

          if (Engine.FrameCounter % (ulong) self_o.framesPerParticle == 0) {
            self_o.trailParticle.Direction = (self_o.from - self_o.to).Angle();
            (self_o.Scene as Level).Particles.Emit(
              type: self_o.trailParticle,
              amount: 1,
              position: self_o.Center,
              positionRange: Vector2.Normalize(new Vector2(1, 1)) * self_o.particleRadius,
              color: new Color(1f, 1f, 1f)
            );
          }
        }

        return 0;
      } else {
        return orig(self);
      }
    }

    // dummy mode freezing
    private static int onWaitingUpdate(
      On.Celeste.AngryOshiro.orig_WaitingUpdate orig, AngryOshiro self
    ) {
      if (self is FloatyOshiro self_o) {
        Player entity = self_o.Scene.Tracker.GetEntity<Player>();
        // don't freeze if player is n-units from left screen edge
        // just rm that dumbass check
        return (entity != null && entity.Speed != Vector2.Zero) ? 0 : 4;
      }

      return orig(self);
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

        Color spriteColor = self_o.recolor;
        spriteColor = spriteColor * self_o.fadeEased;
        self_o.Sprite.SetColor(spriteColor);

        orig(self);
        return;
      }

      orig(self);
    }
  }

}
