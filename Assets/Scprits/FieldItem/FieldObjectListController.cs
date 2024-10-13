using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldObjectListController : MonoBehaviour
{
    public static FieldObjectListController Instance;
    void Awake()
    {
        Instance = this;
    }

    public List<FieldApplicationSpawner> FieldApplicationSpawners;
    public FieldApplicationSpawner GetOneEmptyRandom()
    {
        var emptyCtrls = FieldApplicationSpawners.Where(x => x.IsEmpty).ToList();
        var cnt = emptyCtrls.Count();
        if(cnt == 0)
        {
            return null;
        }

        var index = Random.Range(0, cnt);
        return emptyCtrls[index];
    }

}
