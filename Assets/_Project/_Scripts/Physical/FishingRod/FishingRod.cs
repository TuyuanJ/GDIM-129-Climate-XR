using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MagnetFishing
{

    [Serializable]
    public class Fish
    {
        public string Name;
    }

    public class FishingRod : MonoBehaviour
    {
        [SerializeField] private Hook _hook;
        [SerializeField] private Transform _rodTipTransform;
        [SerializeField] private List<Fish> uncaughtFishes = new List<Fish>();

        private Dictionary<string, int> caughtFishes = new Dictionary<string, int>();
        private Fish currentFish;

        private Vector3 _startingPos;
        private Quaternion _startingRot;


        // Public method to get the caught fishes
        public Dictionary<string, int> GetCaughtFishes()
        {
            return caughtFishes;
        }

        private void Awake()
        {
            _startingPos = transform.position;
            _startingRot = transform.rotation;
        }

        private void OnEnable()
        {
            DisableHook();
            transform.SetPositionAndRotation(_startingPos, _startingRot);

            GameSignals.HOOK_RELEASED.AddListener(ThrowHook);
            GameSignals.FISH_CAUGHT.AddListener(FishCaught);
            GameSignals.FISH_GOT_AWAY.AddListener(FishGotAway);
        }

        private void OnDisable()
        {
            DisableHook();

            GameSignals.HOOK_RELEASED.RemoveListener(ThrowHook);
            GameSignals.FISH_CAUGHT.RemoveListener(FishCaught);
            GameSignals.FISH_GOT_AWAY.RemoveListener(FishGotAway);
        }

        private void Start()
        {
            DisableHook();
            SelectFish(); // Select the first fish
        }

        private void FishCaught(ISignalParameters parameters)
        {
            if (currentFish != null)
            {
                // Remove from uncaught and add/update in caught
                uncaughtFishes.Remove(currentFish);
                if (caughtFishes.ContainsKey(currentFish.Name))
                {
                    caughtFishes[currentFish.Name]++;
                }
                else
                {
                    caughtFishes.Add(currentFish.Name, 1);
                }

                UnityEngine.Debug.Log(currentFish.Name + " Caught!");
            }
            DisableHook();
            SelectFish(); // Select a new fish
        }

        private void FishGotAway(ISignalParameters parameters)
        {
            UnityEngine.Debug.Log(currentFish != null ? currentFish.Name + " Got Away!" : "Fish Got Away!");
            DisableHook();
            SelectFish(); // Select a new fish
        }

        private void ThrowHook(ISignalParameters parameters)
        {
            if (parameters.HasParameter("ForcePercentage"))
            {
                float forcePercentage = (float)parameters.GetParameter("ForcePercentage");

                DisableHook();

                if (_hook != null && _hook.gameObject != null)
                {
                    _hook.gameObject.SetActive(true);
                }
                _hook.ThrowHook(_rodTipTransform, forcePercentage, _rodTipTransform.position += new Vector3(0, 0.15f, 0));
            }
        }

        private void DisableHook(ISignalParameters parameters = null)
        {
            if (_hook != null && _hook.gameObject != null)
            {
                _hook.gameObject.SetActive(false);
            }
        }

        // called when front trigger is pressed
        public void Activate(ActivateEventArgs args)
        {
            GameSignals.ROD_ACTIVATED.Dispatch();
        }

        // called when front trigger is released
        public void DeActivate(DeactivateEventArgs args)
        {
            GameSignals.ROD_DEACTIVATED.Dispatch();
        }

        // called when rod is selected
        public void FirstSelectEnter(SelectEnterEventArgs args)
        {
            GameSignals.ROD_SELECTED.Dispatch();
        }

        // caled when rod is deselected
        public void LastSelectExit()
        {
            StartCoroutine(ReturnToStartingPos());
        }

        private IEnumerator ReturnToStartingPos()
        {
            GameSignals.ROD_DESELECTED.Dispatch();

            yield return new WaitForSeconds(0.75f);
            
            DisableHook();
            transform.SetPositionAndRotation(_startingPos, _startingRot);
        }
        public void ReelInHook()
        {
            if (_hook != null && _hook.inWater)
            {
                _hook.ReelInOneTick(_rodTipTransform);

                // distance is determined by mini game completion, ima leave this blank just for now

                //if (Vector3.Distance(_hook.transform.position, _rodTipTransform.position) > 0.75f)
                //{
                //    _hook.ReelInOneTick(_rodTipTransform);
                //}
                //else // Later on we can have it where if it gets close enough, it reels up! Or just have it destroy like this and then spawn in the trash
                //{
                //    _hook.ToggleInWater(false);
                //    DestroyHook();
                //}
            }
        }

        private void SelectFish()
        {
            if (uncaughtFishes.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, uncaughtFishes.Count);
                currentFish = uncaughtFishes[index];
            }
        }
        //debug only, display all fish that caught
        private void DisplayCaughtFishes()
        {
            foreach (var pair in caughtFishes)
            {
                UnityEngine.Debug.Log($"Caught {pair.Key}: {pair.Value} times.");
            }
        }
    }
}
