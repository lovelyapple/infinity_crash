using UnityEngine;

public class MonoSingletoneBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    // シングルトンインスタンスを取得するためのプロパティ
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // インスタンスがまだ作成されていない場合は、新しいインスタンスを作成する
                _instance = FindObjectOfType<T>();
            }
            return _instance;
        }
    }

    // Awakeメソッドでの初期化
    public void Awake()
    {
        _instance = this as T;
        GameObject.DontDestroyOnLoad(_instance.gameObject);
        OnAwake();
    }

    public virtual void OnAwake()
    {

    }
}
