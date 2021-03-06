﻿namespace MxNet
{
    public class NDArrayOrSymbol
    {
        private readonly object X;

        public NDArrayOrSymbol(NDArray x)
        {
            IsNDArray = true;
            IsSymbol = false;
            X = x;
        }

        public NDArrayOrSymbol(Symbol x)
        {
            IsNDArray = false;
            IsSymbol = true;
            X = x;
        }

        public bool IsSymbol { get; set; }

        public bool IsNDArray { get; set; }

        public NDArray NdX
        {
            get
            {
                if (IsNDArray)
                    return (NDArray) X;

                return null;
            }
        }

        public Symbol SymX
        {
            get
            {
                if (IsSymbol)
                    return (Symbol) X;

                return null;
            }
        }

        public static implicit operator NDArrayOrSymbol(NDArray x)
        {
            if (x == null)
                return null;
            return new NDArrayOrSymbol(x);
        }

        public static implicit operator NDArrayOrSymbol(Symbol x)
        {
            if (x == null)
                return null;
            return new NDArrayOrSymbol(x);
        }

        public static implicit operator NDArray(NDArrayOrSymbol x)
        {
            if (x == null)
                return null;
            return x.NdX;
        }

        public static implicit operator Symbol(NDArrayOrSymbol x)
        {
            if (x == null)
                return null;
            return x.SymX;
        }
    }
}