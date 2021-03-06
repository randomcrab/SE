﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Core;
using SE.Engine.Utility;
using SE.Particles;
using SE.Particles.Modules;
using SE.Particles.Shapes;
using Curve = SE.Utility.Curve;
using Vector4 = System.Numerics.Vector4;
using Vector2 = System.Numerics.Vector2;
using System;

namespace SE.Components
{
    [HeadlessMode(HeadlessSupportMode.NoHeadless)]
    public class ParticleSystem : Component
    {
        public Texture2D Texture;
        public Rectangle SourceRect;

        public Emitter Emitter;

        protected override void OnInitialize()
        {
            //Emitter = new Emitter(shape: new CircleEmitterShape(64.0f, EmissionDirection.Out, true, true, 0.5f));

            CircleEmitterShape circleShape = new CircleEmitterShape(32.0f, EmissionDirection.Out, false, false);
            RectangleEmitterShape rectangleShape = new RectangleEmitterShape(
                new Vector2(128.0f, 128.0f), 
                EmissionDirection.Out, 
                true, 
                true);

            Emitter = new Emitter(4096, shape: circleShape);
            Emitter.Texture = Texture;
            //Emitter.Space = Space.Local;

            Curve angleCurve = new Curve();
            angleCurve.Keys.Add(0.0f, 0.0f);
            angleCurve.Keys.Add(0.25f, 0.1f);
            angleCurve.Keys.Add(0.5f, 1.0f);
            angleCurve.Keys.Add(1.0f, 10.0f);

            Curve forwardVelocityCurve = new Curve();
            forwardVelocityCurve.Keys.Add(0.0f, 0.0f);
            forwardVelocityCurve.Keys.Add(0.20f, 128.0f);
            forwardVelocityCurve.Keys.Add(0.5f, 512.0f);
            forwardVelocityCurve.Keys.Add(1.0f, 3000.0f);

            Curve4 colorCurve = new Curve4();
            colorCurve.Add(0.0f, new Vector4(0.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.25f, new Vector4(30.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.5f, new Vector4(120.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(0.8f, new Vector4(30.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(1.0f, new Vector4(240.0f, 1.0f, 0.5f, 0.0f));

            //Emitter.Config.Color.SetNormal(new Vector4(300.0f, 1.0f, 0.5f, 1.0f));

            Emitter.Config.Color.SetRandomBetween(
                new Vector4(0.0f, 1.0f, 0.5f, 0.0f),
                new Vector4(30.0f, 1.0f, 0.6f, 0.0f));

            //Emitter.Config.Color.SetNormal(new Vector4(0f, 1.0f, 0.5f, 1.0f));

            Emitter.Config.Scale.SetRandomBetween(0.0333f, 0.0667f);
            Emitter.Config.Life.SetRandomBetween(0.2f, 1.0f);
            Emitter.Config.Speed.SetRandomBetween(32.0f, 128.0f);
            Emitter.Config.Speed.SetNormal(256.0f);

            ScaleModule s = ScaleModule.Lerp(0.5f, 2.0f);

            //Emitter.AddModule(RotationModule.RandomCurve(angleCurve));
            //Emitter.AddModule(SpeedModule.Lerp(64.0f, 512.0f));
            Emitter.AddModule(s);
            Emitter.AddModule(TextureAnimationModule.OverLifetime(5, 5));
            //Emitter.AddModule(AttractorModule.Basic(new Vector2(512.0f, 512.0f), 64.0f, 1024.0f, 10, 8.0f));

            //ColorModule baseColorModule = ColorModule.RandomLerp(
            //    new Vector4(0f, 1.0f, 0.5f, 0.0f),
            //    new Vector4(360f, 1.0f, 0.5f, 0.0f));

            //Emitter.AddModule(baseColorModule);
            //Emitter.AddModule(baseColorModule);

            Curve alphaCurve = new Curve();
            alphaCurve.Keys.Add(0.0f, 0.0f);
            alphaCurve.Keys.Add(0.1f, 1.0f);
            alphaCurve.Keys.Add(0.667f, 1.0f);
            alphaCurve.Keys.Add(1.0f, 0.0f);

            Emitter.AddModule(HueModule.RandomLerp(0.0f, 30.0f));
            Emitter.AddModule(LightnessModule.Lerp(0.667f));
            Emitter.AddModule(AlphaModule.Curve(alphaCurve));

            //Emitter.RemoveModules(s, baseColorModule);

            //Emitter.Enabled = true;
        }

        protected override void OnEnable()
        {
            Emitter.Enabled = true;
        }

        protected override void OnDestroy()
        {
            Emitter?.DisposeAfter();
            Emitter = null; // Fix weird memory leak.
        }

        protected override void OnDisable()
        {
            if(!PendingDestroy)
                Emitter.Enabled = false;
        }

        private float time;
        protected override void OnUpdate()
        {
            Emitter.Position = Owner.Transform.GlobalPositionInternal;
            Emitter.Rotation += MathHelper.TwoPi * Time.DeltaTime;

            if(InputManager.KeyCodePressed(Microsoft.Xna.Framework.Input.Keys.O)) {
                Emitter.ParallelEmission = !Emitter.ParallelEmission;
            }

            time -= Time.DeltaTime;
            while (time <= 0.0f) {
                Emitter.Emit(96);
                time += 0.0333f;
            }
        }

        protected override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
            if(!PendingDestroy)
                Emitter?.Dispose();
        }
    }
}
