﻿using System;

namespace MxNet.Gluon.NN
{
    public class MaxPool1D : _Pooling
    {
        public MaxPool1D(int pool_size = 2, int? strides = null, int padding = 0, string layout = "NCW",
            bool ceil_mode = false, string prefix = null, ParameterDict @params = null)
            : base(new[] {pool_size}
                , strides.HasValue ? new[] {strides.Value} : new[] {pool_size}
                , new[] {padding}, ceil_mode, false, PoolingType.Max, layout, null, prefix, @params)
        {
            if (layout != "NCW" && layout != "NWC")
                throw new Exception("Only NCW and NWC layouts are valid for 1D Pooling");
        }
    }
}