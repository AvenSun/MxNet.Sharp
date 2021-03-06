﻿using System;

namespace MxNet.Metrics
{
    public class RMSE : EvalMetric
    {
        public RMSE(string output_name = null, string label_name = null) : base("rmse", output_name, label_name, true)
        {
        }

        public override void Update(NDArray labels, NDArray preds)
        {
            if (labels.Shape.Dimension == 1)
                labels = labels.Reshape(labels.Shape[0], 1);
            if (preds.Shape.Dimension == 1)
                preds = preds.Reshape(preds.Shape[0], 1);

            var rmse = (float) Math.Sqrt(nd.Square(labels - preds).Mean());

            sum_metric += rmse;
            global_sum_metric += rmse;
            num_inst += 1;
            global_num_inst += 1;
        }
    }
}