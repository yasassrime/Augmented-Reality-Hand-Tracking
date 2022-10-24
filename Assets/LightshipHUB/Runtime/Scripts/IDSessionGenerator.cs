using UnityEngine;
using UnityEngine.UI;

public class IDSessionGenerator : MonoBehaviour
{
  void Awake()
  {
    this.GetComponent<InputField>().text = GenerateRandomText();
  }

  private string GenerateRandomText()
  {
    string builder = "";

    for (int i = 0; i < 6; ++i)
    {
      int r = Random.Range(0, 26);
      builder += (char)('A' + r);
    }

    return builder;
  }
}
