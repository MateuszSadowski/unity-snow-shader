float getClampedInterpolationStep(float baseLevel, float falloff, float value)
{
    float min = baseLevel - falloff;
    float max = baseLevel + falloff;
    return clamp((value - min) / (max - min), 0.0, 1.0);
}