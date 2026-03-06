using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string ANIMATOR_IS_FLASHING = "isFlashing";

    [SerializeField] private StoveCounter stoveCounter;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        animator.SetBool(ANIMATOR_IS_FLASHING, false);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEvnetArgs e)
    {
        float showFlashProgressAmount = .5f;
        bool isFlashing = e.progressNormalized > showFlashProgressAmount && stoveCounter.IsFried();

        animator.SetBool(ANIMATOR_IS_FLASHING, isFlashing);
    }
}
