﻿namespace MxNet.Gluon.NN
{
    public class GlobalAvgPool1D : _Pooling
    {
        public GlobalAvgPool1D(string layout = "NCW", string prefix = null, ParameterDict @params = null)
            : base(new[] {1}, null, new[] {0}, true, true, PoolingType.Avg, layout, null, prefix, @params)
        {
        }
    }
}