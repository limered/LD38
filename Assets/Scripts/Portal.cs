using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Triggers;
using UniRx;

public class Portal : MonoBehaviour
{
    public Portal target;

    [SerializeField]
    private bool entering = false;

    [SerializeField]
    private bool exiting = false;

    [SerializeField]
    private bool ignoreNextPortalEntry = false;

    private readonly Subject<Portal> gotoPortalSubject = new Subject<Portal>();

    void Start()
    {
        ignoreNextPortalEntry = entering = exiting = false;

        SetupCamera();
        SetupTrigger();

        OnPortal().Subscribe(portal =>
        {
            Debug.Log("Goto: " + portal.name);
            portal.Spawn(GameObject.FindGameObjectWithTag("Player"));
        }).AddTo(this);
    }

    public void Spawn(GameObject player)
    {
        ignoreNextPortalEntry = true;
        player.transform.position = gameObject.transform.position;
    }

    public IObservable<Portal> OnPortal()
    {
        return gotoPortalSubject.Where(x => x);
    }

    private void SetupCamera()
    {
        var portalCam = target.GetComponentInChildren<Camera>();
        var portalWindow = transform.FindChild("Portal_Window").GetComponent<Renderer>();
        var renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

        portalCam.targetTexture = renderTexture;
        portalWindow.material.mainTexture = renderTexture;
        renderTexture.Create();

        var player = GameObject.FindGameObjectWithTag("Player");
        portalCam.UpdateAsObservable().Subscribe(_ =>
        {
            //portalCam.transform.localRotation = player.transform.localRotation;
            var pos = target.transform.InverseTransformPoint(player.transform.position);

            //really?
            // portalCam.transform.position = new Vector3(-pos.x, portalCam.transform.position.y, -pos.z);

            float angle = SignedAngle(target.transform.forward, player.transform.forward, Vector3.up);
			portalCam.transform.localRotation = Quaternion.Euler(0f, angle, 0f);

        }).AddTo(this);
    }

    private float SignedAngle(Vector3 a, Vector3 b, Vector3 n)
    {
        // angle in [0,180]
        float angle = Vector3.Angle(a, b);
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        // angle in [-179,180]
        float signed_angle = angle * sign;

        while (signed_angle < 0) signed_angle += 360;

        return signed_angle;
    }

    private void SetupTrigger()
    {
        var entryCollider = this.transform.FindChild("Portal_Entry").gameObject.GetComponent<MeshCollider>();
        var exitCollider = this.transform.FindChild("Portal_Exit").gameObject.GetComponent<MeshCollider>();

        entryCollider.OnTriggerEnterAsObservable()
        .Subscribe(c =>
        {
            if (ignoreNextPortalEntry)
            {
                ignoreNextPortalEntry = false;
                return;
            }

            if (exiting)
            {
                exiting = false;
                gotoPortalSubject.OnNext(target);
            }
            else entering = true;
        })
        .AddTo(this);
        entryCollider.OnTriggerExitAsObservable().Subscribe(c => entering = false).AddTo(this);

        exitCollider.OnTriggerEnterAsObservable()
        .Subscribe(c =>
        {
            if (ignoreNextPortalEntry)
            {
                ignoreNextPortalEntry = false;
                return;
            }

            if (entering)
            {
                entering = false;
                gotoPortalSubject.OnNext(target);
            }
            else exiting = true;
        })
        .AddTo(this);
        exitCollider.OnTriggerExitAsObservable().Subscribe(c => exiting = false).AddTo(this);
    }
}
