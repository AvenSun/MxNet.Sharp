﻿using MxNet.Image;

namespace MxNet.Gluon.Data.Vision.Transforms
{
    public class RandomResizedCrop : Block
    {
        private readonly ImgInterp _interpolation;
        private readonly (float, float) _ratio;
        private readonly (float, float) _scale;
        private readonly (int, int) _size;

        public RandomResizedCrop((int, int) size, (float, float)? scale = null, (float, float)? ratio = null,
            ImgInterp interpolation = ImgInterp.Bilinear) : base(null, null)
        {
            _size = size;
            _scale = scale.HasValue ? scale.Value : (0.08f, 1.0f);
            _ratio = ratio.HasValue ? ratio.Value : (3.0f / 4.0f, 4.0f / 3.0f);
            _interpolation = interpolation;
        }

        public override NDArrayOrSymbol Forward(NDArrayOrSymbol x, params NDArrayOrSymbol[] args)
        {
            return Img.RandomSizeCrop(x, _size, _scale, _ratio, _interpolation).Item1;
        }
    }
}