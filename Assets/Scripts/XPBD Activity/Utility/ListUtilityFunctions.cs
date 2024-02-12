using UnityEngine;
using System.Collections.Generic;


namespace XPBD_Activity.Utility
{
    public static class ListUtilityFunctions
    {
        public static void Shuffle<T>(List<T> inputList)
        {
            for (int i = 0; i < inputList.Count - 1; i++)
            {
                T temp = inputList[i];
                int rand = Random.Range(i, inputList.Count);
                inputList[i] = inputList[rand];
                inputList[rand] = temp;
            }
        }
    }
}