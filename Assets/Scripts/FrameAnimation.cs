using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[ExecuteInEditMode]
public class FrameAnimation : MonoBehaviour
{

    [Range(0.01f, 5f)]
    public float interval = 0.25f;

    [SerializeField]
    private GameObject[] models;

    [SerializeField]
    private int currentFrame = 0;

    private float lastFrameUpdate = 0f;

    void Awake()
    {
        if(models == null) {
            ReinitModels();
        }
    }

    void Start()
    {
        this.UpdateAsObservable()
        .Where((_, i) => {
            lastFrameUpdate += Time.deltaTime;

            if(lastFrameUpdate >= interval){
                lastFrameUpdate = lastFrameUpdate-interval;
                return true;
            }

            return false;
        })
        .Select(x => (currentFrame+1) % (models.Length))
        .Subscribe(ActivateFrame)
        .AddTo(this);
    }

    public void ReinitModels()
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

        ActivateFrame(currentFrame);
    }

    public void ActivateFrame(int frame){
        if(models != null && frame >= 0 && frame < models.Length){
            for (int i = 0; i < models.Length; i++)
            {
                models[i].SetActive(i == frame);
            }
            currentFrame = frame;
        }
    }
}
