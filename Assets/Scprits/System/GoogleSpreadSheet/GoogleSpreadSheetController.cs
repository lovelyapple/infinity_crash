using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSpreadSheetController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Label;
    string url => "https://docs.google.com/spreadsheets/d/1lhTx4ttO_pyxFy5zN9L98Xm-7G6NAq5WMK05Dvu5yW8/gviz/tq?tqx=out:csv&sheet=data";

    List<string> datas = new List<string>(); //データ格納用のStgring型のList
    [ContextMenu("Read")]
    public void Read()
    {
        Label.text = "";
        datas.Clear();
        StartCoroutine(GetData());
    }
    IEnumerator GetData()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url)) //UnityWebRequest型オブジェクト
        {
            yield return req.SendWebRequest(); //URLにリクエストを送る

            if (IsWebRequestSuccessful(req)) //成功した場合
            {
                ParseData(req.downloadHandler.text); //受け取ったデータを整形する関数に情報を渡す
                DisplayText(); //データを表示する
            }
            else                            //失敗した場合
            {
                Debug.Log("error");
            }
        }
    }
    void DisplayText()
    {
        foreach (string data in datas)
        {
            Label.text += data + "\n";
        }
    }
    void ParseData(string csvData)
    {
        string[] rows = csvData.Split(new[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries); //スプレッドシートを1行ずつ配列に格納
        foreach (string row in rows)
        {
            string[] cells = row.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);//一行ずつの情報を1セルずつ配列に格納
            foreach (string cell in cells)
            {
                string trimCell = cell.Trim('"'); //セルの文字列からダブルクォーテーションを除去
                if (!string.IsNullOrEmpty(trimCell)) //除去した文字列が空白でなければdatasに追加していく
                {
                    datas.Add(trimCell);
                }
            }
        }
    }
    bool IsWebRequestSuccessful(UnityWebRequest req)
    {
        /*プロトコルエラーとコネクトエラーではない場合はtrueを返す*/
        return req.result != UnityWebRequest.Result.ProtocolError &&
               req.result != UnityWebRequest.Result.ConnectionError;
    }
}
