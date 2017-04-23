using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class LoopFrameAnimation : MonoBehaviour
{
    [SerializeField]
    private string stateName;
    public string StateName { get { return stateName; } }

    [SerializeField]
    private string stateGroup;
    public string StateGroup { get { return stateGroup; } }

    [Range(0.01f, 5f)]
    public float interval = 0.25f;

    [SerializeField]
    protected GameObject[] models;

    [SerializeField]
    protected int currentFrame = 0;

    protected float lastFrameUpdate = 0f;

    public BoolReactiveProperty ActiveState = new BoolReactiveProperty();

    void Awake()
    {
        if (models != null)
        {
            foreach (var model in models)
            {
                model.SetActive(false);
            }
        }
    }

    void Start()
    {
        this.UpdateAsObservable()
                .Where(_ => ActiveState.Value && models != null && models.Length > 0)
                .Where((_, i) =>
                {
                    lastFrameUpdate += Time.deltaTime;

                    if (lastFrameUpdate >= interval)
                    {
                        lastFrameUpdate = lastFrameUpdate - interval;
                        return true;
                    }

                    return false;
                })
                .Select(x => (currentFrame + 1) % (models.Length))
                .Subscribe(ActivateFrame)
                .AddTo(this);


        ActiveState.Subscribe(x =>
        {
            if (models != null)
            {
                for (int i = 0; i < models.Length; i++)
                {
                    models[i].SetActive(x ? i == currentFrame : false);
                }
            }
        }).AddTo(this);
    }

    public virtual void ReinitModels()
    {
        List<Tuple<int, GameObject>> modelList = new List<Tuple<int, GameObject>>();
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.Contains("-") && child.name.LastIndexOf("-") < child.name.Length - 2)
            {
                var numberString = child.name.Substring(child.name.LastIndexOf("-") + 1, 2);
                int number;
                if (int.TryParse(numberString, out number))
                {
                    modelList.Add(Tuple.Create(number, child.gameObject));
                }
            }
        }

        modelList.Sort((x1, x2) => x1.Item1 - x2.Item1);
        models = modelList.Select(x => x.Item2).ToArray();
    }

    public virtual void ActivateFrame(int frame)
    {
        if (models != null && frame >= 0 && frame < models.Length)
        {
            for (int i = 0; i < models.Length; i++)
            {
                models[i].SetActive(i == frame);
            }
            currentFrame = frame;
        }
    }
}
