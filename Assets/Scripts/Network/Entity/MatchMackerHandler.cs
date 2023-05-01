using UnityEngine;

public class MatchMackerHandler : MonoBehaviour
{
    private MatchMaker matchMaker = new MatchMaker();

    private void Start()
    {
        matchMaker.Start();
    }

    private void Update()
    {
        matchMaker.Update();
    }

    private void OnDestroy()
    {
        matchMaker.OnDestroy();
    }
}
