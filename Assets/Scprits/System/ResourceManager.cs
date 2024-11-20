using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoSingletoneBase<ResourceManager>
{
    public List<FieldAppIcon> FieldAppIcons;
    public LineRenderer LineRenderer;
    [SerializeField] List<GameObject> ScreenEffects;
    public void TurnOnEffect(string effectName)
    {
        foreach (var f in ScreenEffects)
        {
            if (f.gameObject.name == effectName)
            {
                f.gameObject.SetActive(true);
                return;
            }
        }
    }
    public void TurnOffEffect(string effectName)
    {
        foreach (var f in ScreenEffects)
        {
            if (f.gameObject.name == effectName)
            {
                f.gameObject.SetActive(false);
                return;
            }
        }
    }
    public FieldAppIcon CreateFieldIcon(ApplicationType type)
    {
        var prefab = FieldAppIcons.FirstOrDefault(x => x.ApplicationType == type);
        return Instantiate<FieldAppIcon>(prefab);
    }
}
