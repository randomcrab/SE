﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Engine.Utility;
using SEParticles;
using SEParticles.Modules;
using SEParticles.Shapes;
using Curve = SE.Utility.Curve;
using Vector4 = System.Numerics.Vector4;

namespace SE.Components
{
    public class ParticleSystem : Component
    {
        public Texture2D Texture;
        public Rectangle SourceRect;

        public Emitter Emitter;

        public bool AddedToParticleEngine { get; internal set; } = false;
        public int ParticleEngineIndex { get; internal set; } = -1;

        protected override void OnInitialize()
        {
            Emitter = new Emitter(shape: new CircleShape(16.0f, EmissionDirection.Out, true, true));
            Emitter.Texture = Texture;
            Emitter.StartRect = SourceRect;

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
            colorCurve.Add(0.8f, new Vector4(240.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(1.0f, new Vector4(360.0f, 1.0f, 0.5f, 0.0f));

            Emitter.Config.Color.SetRandomBetween(
                new Vector4(0.0f, 1.0f, 0.5f, 1.0f),
                new Vector4(360.0f, 1.0f, 0.5f, 1.0f));
            Emitter.Config.Scale.SetRandomBetween(0.25f, 0.667f);
            Emitter.Config.Life.SetRandomBetween(0.2f, 1.0f);
            Emitter.Config.Speed.SetRandomBetween(32.0f, 128.0f);

            //Emitter.AddModule(RotationModule.RandomCurve(angleCurve));
            //Emitter.AddModule(ForwardVelocityModule.RandomConstant(64.0f, 128.0f));
            //Emitter.AddModule(ScaleModule.RandomConstant(0.333f, 0.667f));
            //Emitter.AddModule(AttractorModule.Basic(new Vector2(512.0f, 512.0f), 64.0f, 1024.0f));

            ColorModule baseColorModule = ColorModule.Lerp(
                new Vector4(0f, 1.0f, 0.5f, 0.0f));

            Emitter.AddModule(baseColorModule);
            //Emitter.AddModule(baseColorModule);

            Emitter.Enabled = true;
        }

        protected override void OnEnable()
        {
            Emitter.Enabled = true;
        }

        protected override void OnDestroy()
        {
            Emitter.Enabled = false;
        }

        protected override void OnDisable()
        {
            Emitter.Enabled = false;
        }

        private float time;
        protected override void OnUpdate()
        {
            Emitter.Position = Owner.Transform.GlobalPositionInternal;

            time -= Time.DeltaTime;
            while (time <= 0.0f) {
                Emitter.Emit(32);
                time += 0.1f;
            }
        }
    }
}