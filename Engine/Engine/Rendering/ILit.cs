﻿using SE.Lighting;

namespace SE.Rendering
{

    // TODO: Get rid of this somehow, it's a dumb dependency.
    public interface ILit
    {
        public bool IgnoreLight { get; set; }

        public ShadowCasterType ShadowType { get; set; }

        public ShadowCaster Shadow { get; set; }
    }
}
