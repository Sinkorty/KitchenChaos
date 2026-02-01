using System;
using UnityEngine;

public interface IHasProgress
{
    public event EventHandler<OnProgressChangedEvnetArgs> OnProgressChanged;
    public class OnProgressChangedEvnetArgs : EventArgs
    {
        public float progressNormalized;
    }
}
