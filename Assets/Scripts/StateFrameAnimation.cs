using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class StateFrameAnimation : MonoBehaviour
{
    public StringReactiveProperty activeStates = new StringReactiveProperty();

    [SerializeField]
    private string[] groups; //#justdebugthings
    private readonly Dictionary<string, List<LoopFrameAnimation>> modelStateGroups = new Dictionary<string, List<LoopFrameAnimation>>();

    void Start()
    {
        LoadAnimations();

        activeStates
        .Where(x => x == null || x.Trim().Length > 0)
        .Select(x => x == null ? null : x.Trim())
        .SelectMany(x => string.IsNullOrEmpty(x) ? new string[] { null } : x.Split(',').Select(s => s.Trim()).Where(s2 => !string.IsNullOrEmpty(s2)).ToArray())
        .Subscribe(ActivateState)
        .AddTo(this);
    }

    private void LoadAnimations()
    {
        var animations = GetComponents<LoopFrameAnimation>();

        foreach (var animation in animations)
        {
            animation.ActiveState.Value = false;

            if (modelStateGroups.ContainsKey(animation.StateGroup))
            {
                modelStateGroups[animation.StateGroup].Add(animation);
            }
            else
            {
                modelStateGroups.Add(animation.StateGroup, new List<LoopFrameAnimation>() { animation });
            }
        }

        groups = modelStateGroups.Select(x => x.Key).ToArray();
    }

    public void ActivateState(string stateName)
    {
        if (stateName == null)
        {
            DeactivateAllStates();
            return;
        }

        var group = FindStateGroup(stateName);
        if (group == null)
        {
            Debug.LogWarning("Cannot find state: '" + stateName + "' in GameObject: '" + gameObject.name + "'");
            return;
        }
        else
        {
            foreach (var state in modelStateGroups[group])
            {
                state.ActiveState.Value = state.StateName != null && state.StateName.ToLower() == stateName.ToLower();
            }
        }
    }

    public void DeactivateAllStates()
    {
        foreach (var group in modelStateGroups.Keys)
        {
            foreach (var state in modelStateGroups[group])
            {
                state.ActiveState.Value = false;
            }
        }
    }

    private string FindStateGroup(string stateName)
    {
        foreach (var group in modelStateGroups.Keys)
        {
            foreach (var state in modelStateGroups[group])
            {
                if (state.StateName != null && state.StateName.ToLower() == stateName.ToLower())
                    return group;
            }
        }

        return null;
    }
}
