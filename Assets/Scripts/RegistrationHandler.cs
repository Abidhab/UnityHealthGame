using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegistrationHandler : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField phoneNumberInputField;
    public TMP_InputField countryInputField;
    public Button continueButton;
    public TextMeshProUGUI errorText;

    public CSVHandler csvHandler; // Reference to CSVHandler

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false); // Hide error text initially
        }

        // Optionally, find and assign CSVHandler script
        if (csvHandler == null)
        {
            csvHandler = FindObjectOfType<CSVHandler>();
        }

        if (csvHandler == null)
        {
            Debug.LogError("CSVHandler not found. Please ensure it is attached to a GameObject in the scene.");
        }
    }

    public void OnContinueButtonClicked()
    {
        string name = nameInputField.text;
        string email = emailInputField.text;
        string phoneNumber = phoneNumberInputField.text;
        string country = countryInputField.text;

        csvHandler.SaveToCSV(name, email, phoneNumber, country);
        UiManager.instance.ActivatePanel(2);
    }

    public void ClearInputFields()
    {
        nameInputField.text = "";
        emailInputField.text = "";
        phoneNumberInputField.text = "";
        countryInputField.text = "";
    }
}
