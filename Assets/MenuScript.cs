using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public TMP_InputField inputColonne;
    public TMP_InputField inputRighe;

    void Start()
    {
        // Imposta il tipo di contenuto degli InputField su numeri interi
        inputColonne.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputRighe.contentType = TMP_InputField.ContentType.IntegerNumber;
    }
}
