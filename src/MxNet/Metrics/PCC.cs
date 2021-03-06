﻿using System.Linq;
using NumSharp;

namespace MxNet.Metrics
{
    public class PCC : EvalMetric
    {
        private NDArray gcm;

        private int k;
        private NDArray lcm;

        public PCC(string output_name = null, string label_name = null)
            : base("pcc", output_name, label_name, true)
        {
            k = 2;
        }

        public float SumMetric => CalcMcc(lcm) * num_inst;

        public float GlobalSumMetric => CalcMcc(gcm) * global_num_inst;

        private void Grow(int inc)
        {
            lcm = nd.Pad(lcm, PadMode.Constant, new Shape(0, inc, 0, inc));
            gcm = nd.Pad(gcm, PadMode.Constant, new Shape(0, inc, 0, inc));
            k += inc;
        }

        private float CalcMcc(NDArray cmatArr)
        {
            var cmat = cmatArr.AsNumpy();
            var n = cmat.sum();
            var x = cmat.sum(1);
            var y = cmat.sum(0);
            var cov_xx = np.sum(x * (n - x)).Data<float>()[0];
            var cov_yy = np.sum(y * (n - y)).Data<float>()[0];

            if (cov_xx == 0 || cov_yy == 0)
                return float.NaN;

            var i = cmatArr.Diag().AsNumpy();
            var cov_xy = np.sum(i * n - x * y);
            return np.power(cov_xy / (cov_xx * cov_yy), 0.5);
        }

        public override void Update(NDArray labels, NDArray preds)
        {
            var label = labels.AsType(DType.Int32).AsNumpy();
            var pred = np.argmax(preds.AsNumpy(), 1).astype(NPTypeCode.Int32);
            var n = np.max(pred.max(), label.max()).Data<int>()[0];
            if (n >= k)
                Grow(n + 1 - k);

            var bcm = np.zeros(k, k);
            pred.Data<int>().Zip(label.Data<int>(), (i, j) =>
            {
                bcm[i, j] += 1;
                return true;
            });

            lcm += bcm;
            gcm += bcm;

            num_inst += 1;
            global_num_inst += 1;
        }

        public override void Reset()
        {
            global_num_inst = 0;
            gcm = nd.Zeros(new Shape(k, k));
            ResetLocal();
        }

        public override void ResetLocal()
        {
            num_inst = 0;
            lcm = nd.Zeros(new Shape(k, k));
        }
    }
}