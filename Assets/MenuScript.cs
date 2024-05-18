using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public TMP_InputField inputColonne;
    public TMP_InputField inputRighe;
    public TMP_Text risultatoText;
    public TMP_Text avvisoText;
    public TMP_Dropdown tendina;
    public TMP_Text avviso1;

    void Start()
    {
        // Aggiungi il metodo di calcolo come listener per l'evento onValueChanged degli InputField
        inputColonne.onValueChanged.AddListener(UpdateRisultato);
        inputRighe.onValueChanged.AddListener(UpdateRisultato);
        // Aggiungi il metodo di gestione del dropdown come listener per l'evento onValueChanged del Dropdown
        tendina.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public void PlayButtonClicked()
    {
        // Salva i valori degli input field
        int gridXSize = int.Parse(inputColonne.text);
        int gridZSize = int.Parse(inputRighe.text);

        GridSizeData.GridXSize = gridXSize;
        GridSizeData.GridZSize = gridZSize;

        // Ottieni l'opzione selezionata dal Dropdown
        string selectedOption = tendina.options[tendina.value].text;

        // Carica la scena appropriata in base all'opzione selezionata
        if (selectedOption == "Piazza con 5 via di fuga")
        {
            SceneManager.LoadScene(1);
        }
        else if (selectedOption == "Piazza con 10 via di fuga")
        {
            SceneManager.LoadScene(2);
        }
        else if (selectedOption == "Strada Stretta")
        {
            SceneManager.LoadScene(3);
        }
    }

    void UpdateRisultato(string newValue)
    {
        // Controlla se il testo negli InputField può essere convertito in numeri
        if (int.TryParse(inputColonne.text, out int gridXSize) && int.TryParse(inputRighe.text, out int gridZSize))
        {
            // Calcola il risultato della moltiplicazione
            int risultato = gridXSize * gridZSize;

            // Mostra il risultato nell'elemento Text UI
            risultatoText.text = $"{risultato}";

            // Mostra il messaggio di avviso positivo e nasconde il messaggio di errore
            avviso1.text = "I parametri vanno bene";
            avviso1.color = Color.green;
            avviso1.gameObject.SetActive(true);
            avvisoText.gameObject.SetActive(false);
        }
        else
        {
            // Se almeno uno dei due InputField non contiene un numero, nascondi il risultato
            risultatoText.text = "";

            // Mostra un messaggio di errore
            avvisoText.text = "Inserire numeri validi in entrambi gli InputField";
            avvisoText.color = Color.red;
            avvisoText.gameObject.SetActive(true);
            avviso1.gameObject.SetActive(false);
        }
    }

    void OnDropdownValueChanged(int index)
    {
        // Questo metodo può rimanere vuoto se non hai bisogno di azioni immediate al cambio di selezione
    }
}
